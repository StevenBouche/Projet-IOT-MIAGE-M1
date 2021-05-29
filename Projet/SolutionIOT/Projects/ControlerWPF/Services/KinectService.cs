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

        class CouplePoint
        {
            public float P1 { get; set; }
            public float P2 { get; set; }

            public CouplePoint(float p1, float p2)
            {
                this.P1 = p1;
                this.P2 = p2;
            }
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

        private DetectorSync Status;
        private readonly double PrecisionMesure;
        private readonly int MinVariation = -20;
        private readonly int MaxVariation = 20;
        private readonly Dictionary<DetectorSync, Action<PointsSkeleton>> Actions;
        private readonly PointsSkeleton Points;
        private readonly PointsSkeleton SkeletonSync;
        private readonly DataMotors Data;
        private readonly ConcurrentQueue<Skeleton> Queue;
        private readonly ManualResetEvent AllDone = new ManualResetEvent(false);
        private bool Running = false;
        private readonly MotorValues MotorValues;
        public MQTTService MqttService { get; set; }
        private Task KinectTask;

        public delegate void OnUpdateAnalyse(DetectorSync Status, DataMotors Data, MotorValues MotorValues);
        public OnUpdateAnalyse CallbackAnalyse;

        public KinectService()
        {
            PrecisionMesure = 0.12;
            Status = DetectorSync.WaitSync;
            Points = new PointsSkeleton();
            SkeletonSync = new PointsSkeleton();
            Queue = new ConcurrentQueue<Skeleton>();
            MotorValues = new MotorValues();
            Data = new DataMotors();
            Actions = new Dictionary<DetectorSync, Action<PointsSkeleton>>();
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

                    if (Actions.TryGetValue(Status, out Action<PointsSkeleton> action))
                    {
                        action.Invoke(Points);

                        if (MqttService != null)
                                await MqttService.SendDataMotors(MotorValues);

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

        public void NewFrame(Skeleton[] skeleton)
        {
            if (skeleton.Length == 0)
                return;

            Queue.Enqueue(skeleton[0]);

            AllDone.Set();
        }

        private void StepWaitSync(PointsSkeleton points)
        {

            if (!IsSync(GetConditionWaitSync(points)))
                return;
           
            SkeletonSync.Update(points);
            Data.PlageVariationLeft = (points.EL.Z - points.HL.Z ) * 100;
            Data.PlageVariationRigth = (points.ER.Z - points.HR.Z) * 100;

            Status = DetectorSync.Sync;
            StepSync(points);
        }

        private void StepSync(PointsSkeleton points)
        {

            if (!IsSync(GetConditionSync(points)))
            {
                Status = DetectorSync.WaitSync;
                MotorValues.MotorL = 0;
                MotorValues.MotorR = 0;
                return;
            }

            Data.VariationLeft = (points.HL.Y - SkeletonSync.HL.Y) * 100;
            Data.VariationRigth = (points.HR.Y - SkeletonSync.HR.Y) * 100;

            long vl = Convert.ToInt64(Data.VariationLeft);
            long vr = Convert.ToInt64(Data.VariationRigth);
            vl = vl > MaxVariation ? MaxVariation : vl < MinVariation ? MinVariation : vl;
            vr = vr > MaxVariation ? MaxVariation : vr < MinVariation ? MinVariation : vr;
            var l  = MapValues(vl, MinVariation, MaxVariation, -100, 100);
            var r = MapValues(vr, MinVariation, MaxVariation, -100, 100);
            MotorValues.MotorL = l < 15 ? 0 : l;
            MotorValues.MotorR = r < 15 ? 0 : r;

        }

        private long  MapValues(long x, long in_min, long in_max, long out_min, long out_max)
        {
            long divisor = (in_max - in_min);
            if (divisor == 0)
            {
                return -1; //AVR returns -1, SAM returns 0
            }
            return (x - in_min) * (out_max - out_min) / divisor + out_min;
        }

        private bool IsSync(List<bool> list)
        {
            foreach (bool action in list)
            {
                if (!action)
                    return false;
            }

            return true;
        }

        private List<bool> GetConditionWaitSync(PointsSkeleton points)
        {
            List<bool> list = new List<bool>
            {
                //left
                Math.Abs(points.SL.Z - points.EL.Z) < PrecisionMesure,
                Math.Abs(points.SL.X - points.EL.X) < PrecisionMesure,
                Math.Abs(points.EL.Y - points.HL.Y) < PrecisionMesure,
                //rigth
                Math.Abs(points.SR.Z - points.ER.Z) < PrecisionMesure,
                Math.Abs(points.SR.X - points.ER.X) < PrecisionMesure,
                Math.Abs(points.ER.Y - points.HR.Y) < PrecisionMesure
            };

            return list;
        }

        private List<bool> GetConditionSync(PointsSkeleton points)
        {
            List<bool> list = new List<bool>
            {
                //left
                Math.Abs(points.SL.Z - points.EL.Z) < PrecisionMesure,
                Math.Abs(points.SL.X - points.EL.X) < PrecisionMesure,

                //rigth
                Math.Abs(points.SR.Z - points.ER.Z) < PrecisionMesure,
                Math.Abs(points.SR.X - points.ER.X) < PrecisionMesure
            };

            return list;
        }

        private List<Func<bool>> GetConditionIsSync(PointsSkeleton points)
        {
            List<Func<bool>> list = new List<Func<bool>>
            {
                //left
                () => points.SL.X - PrecisionMesure < points.EL.X,
                () => points.EL.X < points.SL.X + PrecisionMesure,
                () => points.SL.X - PrecisionMesure < points.HL.Z,
                () => points.HL.X < points.SL.X + PrecisionMesure,
                //rigth
                () => points.SR.X - PrecisionMesure < points.ER.X,
                () => points.ER.X < points.SR.X + PrecisionMesure,
                () => points.SR.X - PrecisionMesure < points.HR.Z,
                () => points.HR.X < points.SR.X + PrecisionMesure
            };

            return list;
        }

        private List<CouplePoint> WaitSyncResult(PointsSkeleton points)
        {

            List<CouplePoint> list = new List<CouplePoint>
            {
                new CouplePoint(points.SL.X, points.EL.X),
                new CouplePoint(points.SL.Z, points.EL.Z),
                new CouplePoint(points.HL.Y, points.EL.Y),
                new CouplePoint(points.HL.X, points.EL.X),
                new CouplePoint(points.SR.X, points.ER.X),
                new CouplePoint(points.SR.Z, points.ER.Z),
                new CouplePoint(points.HR.Y, points.ER.Y),
                new CouplePoint(points.HR.X, points.ER.X)
            };

            return list;
        }

        private bool CoordIsEquals(CouplePoint cp)
        {
            return cp.P1 - PrecisionMesure < cp.P2 && cp.P2 < cp.P1 + PrecisionMesure;
        }

    }
}
