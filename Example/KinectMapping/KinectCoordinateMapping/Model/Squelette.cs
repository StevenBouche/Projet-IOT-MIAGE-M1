using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Kinect;

namespace KinectCoordinateMapping.Model
{
    public class Squelette
    {
        private KinectSensor _sensor;
        private CameraMode _mode;
        private Dictionary<string, Point> squelette2D;
        private Dictionary<string, CameraSpacePoint> squelette3D;
      
        private List<Ellipse> Ellipses { get; set; }
        private List<Line> Lines { get; set; }

        HandState handLeft;
        HandState handRight;

        private List<Tuple<JointType, JointType>> bones;


        HipCenter = 0,
        Spine = 1,
        ShoulderCenter = 2,
        Head = 3,
        ShoulderLeft = 4,
        ElbowLeft = 5,
        WristLeft = 6,
        HandLeft = 7,
        ShoulderRight = 8,
        ElbowRight = 9,
        WristRight = 10,
        HandRight = 11,
        HipLeft = 12,
        KneeLeft = 13,
        AnkleLeft = 14,
        FootLeft = 15,
        HipRight = 16,
        KneeRight = 17,
        AnkleRight = 18,
        FootRight = 19

        public Squelette(CameraMode mode, KinectSensor sensor)
        {
            _sensor = sensor;
            _mode = mode;
            this.squelette2D = new Dictionary<string, Point>();
            this.squelette3D = new Dictionary<string, CameraSpacePoint>();

            // a bone defined as a line between two joints
            this.bones = new List<Tuple<JointType, JointType>>();

            // Torse
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HipCenter, JointType.Spine));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.Spine, JointType.ShoulderCenter));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ShoulderCenter, JointType.Head));

            //left arm
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ShoulderCenter, JointType.ShoulderLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ShoulderLeft, JointType.ElbowLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ElbowLeft, JointType.WristLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristLeft, JointType.HandLeft));

            //right arm
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ShoulderCenter, JointType.ShoulderRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ShoulderRight, JointType.ElbowRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ElbowRight, JointType.WristRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristRight, JointType.HandRight));

            //left leg
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HipCenter, JointType.HipLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HipLeft, JointType.KneeLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.KneeLeft, JointType.AnkleLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.AnkleLeft, JointType.FootLeft));

            // right leg
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HipCenter, JointType.HipLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HipLeft, JointType.KneeLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.KneeLeft, JointType.AnkleLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.AnkleLeft, JointType.FootLeft));

        }

        public List<Ellipse> GetEllipses()
        {
            return Ellipses;
        }

        public Dictionary<string, CameraSpacePoint> GetSquelette3D()
        {
            return squelette3D;
        }

        public List<Line> GetLines()
        {
            return Lines;
        }

        public bool GetRightHandOpen()
        {
            return (handRight == HandState.Open) ? true : false;
        }

        public bool GetLeftHandOpen()
        {
            return (handLeft == HandState.Open) ? true : false;
        }


        public bool UpdateData(Body body)
        {
            if(body.HandLeftState == HandState.Closed || body.HandLeftState == HandState.Open) {
                handLeft = body.HandLeftState;
                handRight = body.HandRightState;
            }
            
            foreach (Joint joint in body.Joints.Values)
            {
                if (joint.TrackingState == TrackingState.Tracked)
                {
                    // 3D space point

                    CameraSpacePoint jointPosition = joint.Position;
                    if (squelette3D.ContainsKey(joint.JointType.ToString()))
                    {
                        squelette3D[joint.JointType.ToString()] = jointPosition;
                    }
                    else
                    {
                        squelette3D.Add(joint.JointType.ToString(), jointPosition);
                    }

                    Point point = CreatePoint(jointPosition);
                    if (squelette2D.ContainsKey(joint.JointType.ToString()))
                    {
                        squelette2D[joint.JointType.ToString()] = point;
                    }
                    else
                    {
                        squelette2D.Add(joint.JointType.ToString(), point);
                    }

                }
            }

            Ellipses = initPoint();
            Lines = drawLine(squelette2D, body.Joints);
            return true;
        }

        private Point CreatePoint(CameraSpacePoint jointPosition)
        {
            // 2D space point
            Point point = new Point();

            if (_mode == CameraMode.Deph)
            {
                ColorSpacePoint colorPoint = _sensor.CoordinateMapper.MapCameraPointToColorSpace(jointPosition);

                point.X = float.IsInfinity(colorPoint.X) ? 0 : colorPoint.X;
                point.Y = float.IsInfinity(colorPoint.Y) ? 0 : colorPoint.Y;
            }
            else if (_mode == CameraMode.Depth || _mode == CameraMode.Infrared) // Change the Image and Canvas dimensions to 512x424
            {
                DepthSpacePoint depthPoint = _sensor.CoordinateMapper.MapCameraPointToDepthSpace(jointPosition);

                point.X = float.IsInfinity(depthPoint.X) ? 0 : depthPoint.X;
                point.Y = float.IsInfinity(depthPoint.Y) ? 0 : depthPoint.Y;
            }

            return point;
        }

        private List<Ellipse> initPoint()
        {
            List<Ellipse> ellipses = new List<Ellipse>();

            foreach (Point p in squelette2D.Values)
            {
                // Draw
                Ellipse ellipse = new Ellipse
                {
                    Fill = Brushes.Yellow,
                    Width = 10,
                    Height = 10
                };
                Canvas.SetLeft(ellipse, p.X - ellipse.Width / 2);
                Canvas.SetTop(ellipse, p.Y - ellipse.Height / 2);
                ellipses.Add(ellipse);
            }
            if (squelette2D.ContainsKey("HandLeft"))
            {
                Ellipse ellipse = new Ellipse
                {
                    Fill = Brushes.Yellow,
                    Width = 50,
                    Height = 50
                };
                if (handLeft == HandState.Open)
                {
                    ellipse.Fill = Brushes.Green;
                    Canvas.SetLeft(ellipse, squelette2D["HandLeft"].X - ellipse.Width / 2);
                    Canvas.SetTop(ellipse, squelette2D["HandLeft"].Y - ellipse.Height / 2);
                    ellipses.Add(ellipse);
                }
                else if(handLeft == HandState.Closed)
                {
                    ellipse.Fill = Brushes.Red;
                    Canvas.SetLeft(ellipse, squelette2D["HandLeft"].X - ellipse.Width / 2);
                    Canvas.SetTop(ellipse, squelette2D["HandLeft"].Y - ellipse.Height / 2);
                    ellipses.Add(ellipse);
                }  
            }
            if (squelette2D.ContainsKey("HandRight"))
            {
                Ellipse ellipse = new Ellipse
                {
                    Fill = Brushes.Yellow,
                    Width = 50,
                    Height = 50
                };
                if (handRight == HandState.Open)
                {
                    ellipse.Fill = Brushes.Green;
                    Canvas.SetLeft(ellipse, squelette2D["HandRight"].X - ellipse.Width / 2);
                    Canvas.SetTop(ellipse, squelette2D["HandRight"].Y - ellipse.Height / 2);
                    ellipses.Add(ellipse);
                }
                else if (handRight == HandState.Closed)
                {
                    ellipse.Fill = Brushes.Red;
                    Canvas.SetLeft(ellipse, squelette2D["HandRight"].X - ellipse.Width / 2);
                    Canvas.SetTop(ellipse, squelette2D["HandRight"].Y - ellipse.Height / 2);
                    ellipses.Add(ellipse);
                }
            }
            return ellipses;
        }

        private List<Line> drawLine(Dictionary<string, Point> points, IReadOnlyDictionary<JointType, Joint> jointPoint)
        {
            List<Line> lines = new List<Line>();

            foreach (var bone in this.bones)
            {
                Joint joint0 = jointPoint[bone.Item1];
                Joint joint1 = jointPoint[bone.Item2];

                if(bone.Item2 == JointType.ElbowLeft && bone.Item1 == JointType.ShoulderLeft)
                {
                    if ((joint0.TrackingState == TrackingState.Tracked) && (joint1.TrackingState == TrackingState.Tracked))
                    {
                        Line l = CreateLine(points[bone.Item1.ToString()], points[bone.Item2.ToString()]);
                        lines.Add(l);
                    }
                }
                else
                {
                    if ((joint0.TrackingState == TrackingState.Tracked) && (joint1.TrackingState == TrackingState.Tracked))
                    {

                        lines.Add(CreateLine(points[bone.Item1.ToString()], points[bone.Item2.ToString()]));
                    }
                }
               
            }

            return lines;

        }

        private Line CreateLine(Point pA, Point pD)
        {
            Line myLine = new Line();
            SolidColorBrush mySolidColorBrush = new SolidColorBrush();
            mySolidColorBrush.Color = Color.FromRgb(59, 47, 207);
            myLine.Fill = Brushes.Blue;
            myLine.Stroke = Brushes.LightSteelBlue;
            myLine.X1 = pA.X;
            myLine.X2 = pD.X;
            myLine.Y1 = pA.Y;
            myLine.Y2 = pD.Y;
            myLine.HorizontalAlignment = HorizontalAlignment.Left;
            myLine.VerticalAlignment = VerticalAlignment.Center;
            myLine.StrokeThickness = 5;
            return myLine;
        }


    }

}
