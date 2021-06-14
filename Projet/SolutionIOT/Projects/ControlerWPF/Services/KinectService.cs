using ControlerWPF.Models;
using Microsoft.Kinect;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ControlerWPF.Services
{
    public class KinectService
    {

        public enum DetectorSync
        {
            Sync,
            WaitSync
        }

        class PointsSkeleton
        {
            public SkeletonPoint EL { get; set; }
            public SkeletonPoint SL { get; set; }
            public SkeletonPoint ER { get; set; }
            public SkeletonPoint SR { get; set; }
            public SkeletonPoint HR { get; set; }
            public SkeletonPoint HL { get; set; }

            public void Update(Skeleton body)
            {
                EL = body.Joints[JointType.ElbowLeft].Position;
                SL = body.Joints[JointType.ShoulderLeft].Position;
                ER = body.Joints[JointType.ElbowRight].Position;
                SR = body.Joints[JointType.ShoulderRight].Position;
                HR = body.Joints[JointType.WristRight].Position;
                HL = body.Joints[JointType.WristLeft].Position;
            }

            public void Update(PointsSkeleton points)
            {
                this.EL = points.EL;
                this.SL = points.SL;
                this.ER = points.ER;
                this.SR = points.SR;
                this.HR = points.HR;
                this.HL = points.HL;
            }
        }

        public class DataMotors
        {
            public float PlageVariationLeft { get; set; }
            public float PlageVariationRigth { get; set; }
            public float VariationLeft { get; set; }
            public float VariationRigth { get; set; }

            public DataMotors()
            {
                PlageVariationLeft = 0;
                PlageVariationRigth = 0;
                VariationLeft = 0;
                VariationRigth = 0;
            }
        }

        //parameters
        private readonly double PrecisionMesure = 0.15;
        private readonly double PrecisionMesureInv = -0.15;
        private readonly double DeadZone = 20;
        private readonly double DeadZoneInv = -20;
        private readonly int MinVariation = -2000;
        private readonly int MaxVariation = 2000;

        //Actions status
        private DetectorSync Status;
        private readonly Dictionary<DetectorSync, Func<PointsSkeleton,MotorValues>> Actions;

        //Points
        private readonly PointsSkeleton Points;
        private readonly PointsSkeleton SkeletonSync;

        //Data
        private readonly DataMotors Data;
        private readonly MotorValues MotorValues;

        //Queue
        private readonly ConcurrentQueue<Skeleton> Queue;
        private readonly ManualResetEvent AllDone = new ManualResetEvent(false);
        private bool Running = false;
        
        //Task
        public MQTTService MqttService { get; set; }
        private Task KinectTask;

        public delegate void OnUpdateAnalyse(DetectorSync Status, DataMotors Data, MotorValues MotorValues);
        public OnUpdateAnalyse CallbackAnalyse;

        public KinectService()
        {
            Status = DetectorSync.WaitSync;
            Points = new PointsSkeleton();
            SkeletonSync = new PointsSkeleton();
            Queue = new ConcurrentQueue<Skeleton>();
            MotorValues = new MotorValues();
            Data = new DataMotors();
            Actions = new Dictionary<DetectorSync, Func<PointsSkeleton,MotorValues>>();
            this.InitActions();
        }

        private void InitActions()
        {
            Actions.Add(DetectorSync.Sync, StepSync);
            Actions.Add(DetectorSync.WaitSync, StepWaitSync);
        }

        public void ExecuteTask()
        {
            if (Running)
                return;

            KinectTask = Task.Run(Execute);
        }

        private async void Execute()
        {

            Running = true;
            while (Running)
            {
                AllDone.Reset();

                while(Queue.TryDequeue(out Skeleton skeleton))
                {
                    Points.Update(skeleton);

                    if (Actions.TryGetValue(Status, out Func<PointsSkeleton, MotorValues> action))
                    {
                        //get data motor in function of state
                        MotorValues value = action.Invoke(Points);

                        //calculate diff between new value and last
                        long diffLAbs = Math.Abs(MotorValues.MotorL - value.MotorL);
                        long diffRAbs = Math.Abs(MotorValues.MotorR - value.MotorR);

                        //if diff is more than 2
                        if (diffLAbs > 10 || diffRAbs > 10)
                        {
                            //actualize data
                            MotorValues.MotorL = value.MotorL;
                            MotorValues.MotorR = value.MotorR;

                            //and send to mqtt broker
                            if (MqttService != null)
                               await MqttService.SendDataMotors(MotorValues);
                        }
                        //callback after calcul
                        CallbackAnalyse(Status, Data, MotorValues);
                    }
                }
                AllDone.WaitOne();
            }
        }

        public void Stop()
        {
            Running = false;
            AllDone.Set();
            KinectTask?.Wait();
        }

        public async void NewFrame(Skeleton[] skeleton)
        {
            if (skeleton.Length == 0)
            {
                if(Status == DetectorSync.Sync)
                {
                    Status = DetectorSync.WaitSync;
                    MotorValues.MotorL = 0;
                    MotorValues.MotorR = 0;

                    if (MqttService != null)
                        await MqttService.SendDataMotors(MotorValues);
                }
                return;
            }
                

            Queue.Enqueue(skeleton[0]);

            AllDone.Set();
        }

        private MotorValues StepWaitSync(PointsSkeleton points)
        {
            var value = new MotorValues();

            if (!IsSync(GetDiffConditionWaitSync(points)))
                return value;
           
            SkeletonSync.Update(points);
            Data.PlageVariationLeft = (points.EL.Z - points.HL.Z ) * 100;
            Data.PlageVariationRigth = (points.ER.Z - points.HR.Z) * 100;

            Status = DetectorSync.Sync;
            return StepSync(points);
        }

        private MotorValues StepSync(PointsSkeleton points)
        {

            var value = new MotorValues();

            if (!IsSync(GetDiffConditioSync(points)))
            {
                Status = DetectorSync.WaitSync;
                return value;
            }

            Data.VariationLeft = (points.HL.Y - points.EL.Y) * 10000;
            Data.VariationRigth = (points.HR.Y - points.ER.Y) * 10000;

            long vl = Convert.ToInt64(Data.VariationLeft);
            long vr = Convert.ToInt64(Data.VariationRigth);

            vl = vl > MaxVariation ? MaxVariation : vl < MinVariation ? MinVariation : vl;
            vr = vr > MaxVariation ? MaxVariation : vr < MinVariation ? MinVariation : vr;

            var l  = MapValues(vl, MinVariation, MaxVariation, -100, 100);
            var r = MapValues(vr, MinVariation, MaxVariation, -100, 100);

            value.MotorL = l < DeadZone && l > DeadZoneInv ? 0 : l;
            value.MotorR = r < DeadZone && r > DeadZoneInv ? 0 : r;

            return value;

        }

        private long MapValues(long x, long in_min, long in_max, long out_min, long out_max)
        {
            long divisor = (in_max - in_min);
            if (divisor == 0)
            {
                return -1; 
            }
            return (x - in_min) * (out_max - out_min) / divisor + out_min;
        }

        private bool IsSync(List<float> list)
        {
            foreach (float diff in list)
            {
                if (diff > PrecisionMesure || diff < PrecisionMesureInv)
                    return false;
            }
            return true;
        }

        private List<float> GetDiffConditionWaitSync(PointsSkeleton points)
        {
            List<float> list = new List<float>
            {
                //left
                points.SL.Z - points.EL.Z,
                points.HL.X - points.EL.X,
                points.HL.Y - points.EL.Y,
                points.SL.X - points.EL.X,

                //rigth
                points.SR.Z - points.ER.Z,
                points.HR.X - points.ER.X,
                points.HR.Y - points.ER.Y,
                points.SR.X - points.ER.X,
            };

            return list;
        }

        private List<float> GetDiffConditioSync(PointsSkeleton points)
        {
            List<float> list = new List<float>
            {
                //left
                points.SL.Z - points.EL.Z,
                points.HL.X - points.EL.X,
                points.SL.X - points.EL.X,

                //rigth
                points.SR.Z - points.ER.Z,
                points.HR.X - points.ER.X,
                points.SR.X - points.ER.X,
            };

            return list;
        }
    }
}
