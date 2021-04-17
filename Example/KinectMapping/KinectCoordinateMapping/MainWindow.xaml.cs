using KinectCoordinateMapping.Model;
using KinectCoordinateMapping.ViewModels;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;



namespace KinectCoordinateMapping
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>

    public enum CameraMode
    {
        Deph,
        Depth,
        Infrared
    }


    public partial class MainWindow : Window
    {

        private KinectSensor _sensor;
        private MultiSourceFrameReader _reader;
        CameraMode _mode = CameraMode.Infrared;
        private List<MainViewModel> _chartListModel;
        private BodyHandler _bodyHandler;
        private Manager.NetworkController _networkController;
        private MainWindowViewModel _modelWindow { get; }
        List<Task> tasks = new List<Task>();

        public MainWindow()
        {
            InitializeComponent();

            //Init data context
            _modelWindow = new MainWindowViewModel(this);
            this.DataContext = _modelWindow;

            _bodyHandler = new BodyHandler(this);

            _networkController = new Manager.NetworkController(10000, _bodyHandler);
            _networkController.StartUDP();
            UpdatePanelNetwork();
            InitChart();
        }

        private void InitChart()
        {
            _chartListModel = new List<MainViewModel>();
            _chartListModel.Add(new MainViewModel("HandLeft"));
            _chartListModel.Add(new MainViewModel("HandRight"));
            _chartListModel.Add(new MainViewModel("ElbowRight"));
            _chartListModel.Add(new MainViewModel("ElbowLeft"));
            plotHandLeft.DataContext = _chartListModel[0];
            plotHandRight.DataContext = _chartListModel[1];
            plotElowRight.DataContext = _chartListModel[2];
            plotElowLeft.DataContext = _chartListModel[3];
        }

        public KinectSensor getSensor()
        {
            return _sensor;
        }

        public CameraMode getMode()
        {
            return _mode;
        }

        public MainWindowViewModel GetModelContext()
        {
            return _modelWindow;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _sensor = KinectSensor.GetDefault();

            if (_sensor != null)
            {
                
                _sensor.Open();

                _reader = _sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.Infrared | FrameSourceTypes.Body);
                //_reader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;
                _reader.MultiSourceFrameArrived += _bodyHandler.Reader_BodySourceFrameArrived;

            }
           
            
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (_reader != null)
            {
                _reader.Dispose();
            }

            if (_sensor != null)
            {
                _sensor.Close();
            }

            _networkController.StopUDP();
            Environment.Exit(0);
        }

        

        void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            var reference = e.FrameReference.AcquireFrame();

            // Color
            using (var frame = reference.ColorFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    if (_mode == CameraMode.Deph)
                    {
                        camera.Source = frame.ToBitmap();
                    }
                }
            }

            // Depth
            using (var frame = reference.DepthFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    if (_mode == CameraMode.Depth)
                    {
                        camera.Source = frame.ToBitmap();
                    }
                }
            }

            // Infrared
            using (var frame = reference.InfraredFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    if (_mode == CameraMode.Infrared)
                    {
                        camera.Source = frame.ToBitmap();
                    }
                }
            }

            
        }

        public void UpdateBodyView(Dictionary<ulong, Squelette> _skeleton, Dictionary<ulong, DetectorControl> _detector, ulong TrackingId, IList<Body> _bodies)
        {
            if(TrackingId != 0)
            {
             
                this.Dispatcher.Invoke((Action)(() =>
                {
                    while (canvas.Children.Count > 0)
                    {
                        canvas.Children.RemoveAt(0); 
                    }
                    updateCanvasEllipses(_skeleton[TrackingId]);
                    updateCanvasLines(_skeleton[TrackingId]);
                }));
               
                UpdateChart(_skeleton[TrackingId]);

                State.Content = _detector[TrackingId].GetStateDetector();
                if (_detector[TrackingId].GetStateDetector() == DetectorSync.Sync.ToString())
                {
                    VLeft.Content = "Variation Left : " + _detector[TrackingId].GetVariationLeft() + " cm";
                    VRight.Content = "Variation Right : " + _detector[TrackingId].GetVariationRight() + " cm";
                    VMin.Content = "Variation Min : " + _detector[TrackingId].GetMinVariation() + " cm";
                    VMax.Content = "Variation Max : " + _detector[TrackingId].GetMaxVariation() + " cm";
                }
                else
                {
                    VLeft.Content = "Variation Left : ";
                    VRight.Content = "Variation Right :";
                    VMin.Content = "Variation Min : : " + _detector[TrackingId].GetMinVariation() + " cm";
                    VMax.Content = "Variation Max : " + _detector[TrackingId].GetMaxVariation() + " cm";
                }

                UpdatePanelNetwork();

            }
        }

        private void UpdatePanelNetwork()
        {
            if (_networkController.GetRunning())
            {
                Running.Content = "Service UDP Running";
                StateNetwork.Content = "State : " + _networkController.GetStateNetwork();
            }
            else
            {
                Running.Content = "Service UDP Down";
                StateNetwork.Content = "State : Down";
            }
        }

        private void updateCanvasEllipses(Squelette squelette)
        {
            
            List<Ellipse> ellipses = squelette.GetEllipses();
                foreach (Ellipse ellipse in ellipses)
                {
                    canvas.Children.Add(ellipse);

                }
        }

        private void updateCanvasLines(Squelette squelette)
        {
            List<Line> lines = squelette.GetLines();
            foreach (Line line in lines)
            {
                canvas.Children.Add(line);
            }
        }

        private void UpdateChart(Squelette squelette)
        {
           
            foreach (MainViewModel m in _chartListModel)
            {
                Thread thdUDPServer = new Thread(new ThreadStart(() => {
                    m.UpdatedData(squelette.GetSquelette3D());
                }));
                thdUDPServer.Start();
            }
            
        }


        internal void CurrentBodyChanged(ulong id)
        {
            _bodyHandler.SetCurrentBodyId(id);
            InitChart();
        }



    }
}
