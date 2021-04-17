using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectCoordinateMapping.Model
{

    enum DetectorSync
    {
        Sync,
        WaitSync,
        MinSync,
        MaxSync
    }

    public class DetectorControl
    {
        private Area AreaDetect;
        private DetectorSync Status;
        private float MaxVariation;
        private float MinVariation;
        private double precisionMesure; // Element who let an imprecision to sync more easier

        public DetectorControl()
        {
            Status = DetectorSync.WaitSync;
            precisionMesure = 0.55; //durant test 0.9
        }

        public String GetStateDetector()
        {
            return Status.ToString();
        }

        public float GetVariationLeft()
        {
            if(Status == DetectorSync.Sync)
            {
                return AreaDetect.GetVariationLeft();
            }
            return 0;
        }

        public float GetVariationRight()
        {
            if (Status == DetectorSync.Sync)
            {
                return AreaDetect.GetVariationRight();
            }
            return 0;
        }

        public float GetMaxVariation()
        {
            if (Status == DetectorSync.Sync)
            {
                return MaxVariation;
            }
            return 0;
        }

        public float GetMinVariation()
        {
            if (Status == DetectorSync.Sync)
            {
                return MinVariation;
            }
            return 0;
        }
        public void UpdateData(Body body)
        {
            if(Status == DetectorSync.WaitSync)
            {
                StepWaitSync(body);
            }
            else if (Status == DetectorSync.Sync)
            {
                StepSync(body);
            }
        }

        private void StepWaitSync(Body body)
        {
            

            List<List<float>> l = InitWaitSync(body);

           /* Boolean stateR = false;
            Boolean stateL = false;*/
            Boolean state = true;

            foreach(List<float> list in l)
            {
                if (state)
                {
                    state = CoordIsEquals(list[0],list[1]);
                }
            }

            if (state)
            {
                
                Console.WriteLine("DETECTOR SYNC  OK");
                Console.WriteLine("DETECTOR SYNC  OK DETECTOR SYNC  OK");
                Console.WriteLine("DETECTOR SYNC  OK DETECTOR SYNC  OK DETECTOR SYNC  OK");
                Console.WriteLine("DETECTOR SYNC  OK DETECTOR SYNC  OK DETECTOR SYNC  OK DETECTOR SYNC  OK");
                AreaDetect = new Area(body.Joints);
                setSyncValue(body);
                Status = DetectorSync.Sync;
           
            }
            /*
            if (  hL.Y < eL.Y - 0.05 && hL.Y < eL.Y + 0.05 && hL.X < eL.X - 0.05 && hL.X < eL.X + 0.05)
            {
                
                stateL = true;
            }
            if (sR.X - 0.07 < eR.X && eR.X < sR.X + 0.07 && sR.Z - 0.05 < eR.Z && eR.Z < sR.Z + 0.05 && hR.Y < eR.Y - 0.05 && hR.Y < eR.Y + 0.05 && hR.X < eR.X - 0.05 && hR.X < eR.X + 0.05)
            {
                Console.WriteLine("DETECTOR SYNC RIGHT OK");
                stateR = true;
            }
            if (stateL && stateR)
            {
                
                
            }*/
        }

        private void StepMinSync()
        {
            //TODO STEP MIN SYNC REMOVE ?
        }

        private void StepMaxSync()
        {
            //TODO STEP MAX SYNC REMOVE ?
        }

        private Boolean CoordIsEquals(float p1, float p2)
        {
            if (p1 - precisionMesure < p2 && p2 < p1 + precisionMesure)
            {
                return true;
            }
            else return false;
        }

        
        private void StepSync(Body body)
        {
            CameraSpacePoint eL = body.Joints[JointType.ElbowLeft].Position;
            CameraSpacePoint sL = body.Joints[JointType.ShoulderLeft].Position;
            CameraSpacePoint eR = body.Joints[JointType.ElbowRight].Position;
            CameraSpacePoint sR = body.Joints[JointType.ShoulderRight].Position;
            CameraSpacePoint hR = body.Joints[JointType.HandRight].Position;
            CameraSpacePoint hL = body.Joints[JointType.HandLeft].Position;

            //TODO ADAPTER AVEC COORD IS EQUALS
            if (sL.X - 0.09 < eL.X && eL.X < sL.X + 0.09 && sL.X - 0.09 < hL.Z && hL.X < sL.X + 0.09)
            {
                if (sR.X - 0.09 < eR.X && eR.X < sR.X + 0.09 && sR.X - 0.09 < hR.Z && hR.X < sR.X + 0.09)
                {
                    AreaDetect.Update(body.Joints);
                    Console.WriteLine("VARIATION RIGHT :" + AreaDetect.GetVariationRight());
                    Console.WriteLine("VARIATION LEFT : " + AreaDetect.GetVariationLeft());
                }
                else Status = DetectorSync.WaitSync;
            }
            else Status = DetectorSync.WaitSync;
        }
        private void setSyncValue(Body body)
        {
            CameraSpacePoint eL = body.Joints[JointType.ElbowLeft].Position;
            CameraSpacePoint sL = body.Joints[JointType.ShoulderLeft].Position;
            CameraSpacePoint eR = body.Joints[JointType.ElbowRight].Position;
            CameraSpacePoint sR = body.Joints[JointType.ShoulderRight].Position;
            CameraSpacePoint hR = body.Joints[JointType.HandRight].Position;
            CameraSpacePoint hL = body.Joints[JointType.HandLeft].Position;

            //TODO REVOIR CALCULE
            MinVariation = (((eL.Z - hL.Z) + (eR.Z - hR.Z)) / 2)*-100;
            MaxVariation = (((sL.Y - eL.Y) + (eL.Z - hL.Z)) / 2)*100;
        }

        private List<List<float>> InitWaitSync(Body body)
        {
            CameraSpacePoint eL = body.Joints[JointType.ElbowLeft].Position;
            CameraSpacePoint sL = body.Joints[JointType.ShoulderLeft].Position;
            CameraSpacePoint eR = body.Joints[JointType.ElbowRight].Position;
            CameraSpacePoint sR = body.Joints[JointType.ShoulderRight].Position;
            CameraSpacePoint hR = body.Joints[JointType.HandRight].Position;
            CameraSpacePoint hL = body.Joints[JointType.HandLeft].Position;


            List<List<float>> l = new List<List<float>>();
            float[] inputL = { sL.X, eL.X };
            float[] inputL2 = { sL.Z, eL.Z };
            float[] inputL3 = { hL.Y, eL.Y };
            float[] inputL4 = { hL.X, eL.X };
            float[] inputR = { sR.X, eR.X };
            float[] inputR2 = { sR.Z, eR.Z };
            float[] inputR3 = { hR.Y, eR.Y };
            float[] inputR4 = { hR.X, eR.X };
            l.Add(new List<float>(inputL));
            l.Add(new List<float>(inputL2));
            l.Add(new List<float>(inputL3));
            l.Add(new List<float>(inputL4));
            l.Add(new List<float>(inputR));
            l.Add(new List<float>(inputR2));
            l.Add(new List<float>(inputR3));
            l.Add(new List<float>(inputR4));
            return l;
        }


    }
}
