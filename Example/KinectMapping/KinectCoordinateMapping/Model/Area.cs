using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectCoordinateMapping.Model
{
    class Area
    {
        private CameraSpacePoint HandRightSync;
        private CameraSpacePoint HandLeftSync;
        private CameraSpacePoint ShoulderLeftSync;
        private CameraSpacePoint ShoulderRightSync;

        private CameraSpacePoint HandRight;
        private CameraSpacePoint HandLeft;
        private CameraSpacePoint ShoulderLeft;
        private CameraSpacePoint ShoulderRight;

        private float variationLeft;
        private float variationRight;

        public Area(IReadOnlyDictionary<JointType, Joint> join)
        {
            HandRightSync = join[JointType.HandRight].Position;
            HandLeftSync = join[JointType.HandLeft].Position;
            ShoulderLeftSync = join[JointType.ShoulderLeft].Position;
            ShoulderRightSync = join[JointType.ShoulderRight].Position;
            variationLeft = 0;
            variationRight = 0;
        }

        public void Update(IReadOnlyDictionary<JointType, Joint> join)
        {
            HandRight = join[JointType.HandRight].Position;
            HandLeft = join[JointType.HandLeft].Position;
            ShoulderLeft = join[JointType.ShoulderLeft].Position;
            ShoulderRight = join[JointType.ShoulderRight].Position;
            variationLeft = ((ShoulderLeft.Z - HandLeft.Z) - (ShoulderLeftSync.Z - HandLeftSync.Z) ) * 100;
            variationRight = ((ShoulderRight.Z - HandRight.Z) - (ShoulderRightSync.Z - HandRightSync.Z) ) * 100;
        }

        public float GetVariationLeft()
        {
            return variationLeft;
        }

        public float GetVariationRight()
        {
            return variationRight;
        }
    }
}
