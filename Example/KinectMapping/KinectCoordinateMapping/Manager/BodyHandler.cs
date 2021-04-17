using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;

namespace KinectCoordinateMapping.Model
{
    class BodyHandler
    {
        private ulong _currentBody;
        private IList<Body> _bodies;
        private Dictionary<ulong, Squelette> _skeleton;
        private Dictionary<ulong, DetectorControl> _detector;
        private MainWindow _mainWindow;

        public BodyHandler(MainWindow mainWindow)
        {
            _currentBody = 0;
            _skeleton = new Dictionary<ulong, Squelette>();
            _detector = new Dictionary<ulong, DetectorControl>();
            _mainWindow = mainWindow;
        }

        public float GetVariationLeft()
        {
            if (_currentBody != 0)
            {
                return _detector[_currentBody].GetVariationLeft();
            }
            else return 0;
        }

        public float GetVariationRight()
        {
            if (_currentBody != 0)
            {
                return _detector[_currentBody].GetVariationRight();
            }
            else return 0;
        }

        public bool GetRightHandOpen()
        {
            if (_currentBody != 0)
            {
                return _skeleton[_currentBody].GetRightHandOpen();
            }
            else return false;
        }

        public bool GetLeftHandOpen()
        {
            if (_currentBody != 0)
            {
                return _skeleton[_currentBody].GetLeftHandOpen();
            }
            return false;
        }

        public void Reader_BodySourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            var reference = e.FrameReference.AcquireFrame();

            // Body
            using (var frame = reference.BodyFrameReference.AcquireFrame())
            {
                if (frame != null)
                {

                    _bodies = new Body[frame.BodyFrameSource.BodyCount];
                    frame.GetAndRefreshBodyData(_bodies);
                    
                  

                    foreach (var body in _bodies)
                    {

                        if (body.IsTracked)
                        {
                            
                            if (!_skeleton.ContainsKey(body.TrackingId))
                            {
                                _mainWindow.GetModelContext().AddBody(body);
                                _skeleton.Add(body.TrackingId, new Squelette(_mainWindow.getMode(), _mainWindow.getSensor()));
                                _detector.Add(body.TrackingId, new DetectorControl());    
                            }

                            _detector[body.TrackingId].UpdateData(body);



                            if (_skeleton[body.TrackingId].UpdateData(body))
                            {


                                //anvas.Children.Clear();

                                _mainWindow.UpdateBodyView(_skeleton, _detector, _currentBody, _bodies);
                            }
                        }
                        else
                        {
                            if (_skeleton.ContainsKey(body.TrackingId))
                            {
                                _skeleton.Remove(body.TrackingId);
                                _detector.Remove(body.TrackingId);
                                _mainWindow.GetModelContext().RemoveBody(body);
                            }
                            
                            //TODO REMOVE BODY OF SKELETON
                        }
                    }
                }
            }
        }

        public float GetVariationMin()
        {
           
            if (_currentBody != 0)
            {
                return _detector[_currentBody].GetMinVariation();
            }
            else return 0;

        }

        public float GetVariationMax()
        {
           
            if (_currentBody != 0)
            {
                return _detector[_currentBody].GetMaxVariation();
            }
            else return 0;
        }

        internal void SetCurrentBodyId(ulong id)
        {
            _currentBody = id;
        }
    }
}
