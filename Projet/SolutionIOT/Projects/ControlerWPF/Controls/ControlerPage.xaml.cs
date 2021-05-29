using ControlerWPF.Services;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ControlerWPF.Controls
{
    /// <summary>
    /// Logique d'interaction pour ControlerPage.xaml
    /// </summary>
    public partial class ControlerPage : UserControl
    {

        public MQTTService MqttService { get; set; }
        public ControlerService Service { get; set; }
        Thread thread;
        bool threadBool = true;

        public delegate MQTTService ObtainServiceMQTT();

        public ObtainServiceMQTT GetServiceMQTT;

        public ControlerPage()
        {

            this.Service = new ControlerService(Dispatcher);
            DataContext = this.Service.ControllerView;

            Loaded += PageLoaded;
            Unloaded += PageUnloaded;

            InitializeComponent();
        }

        private void PageUnloaded(object sender, RoutedEventArgs e)
        {
            threadBool = false;
        }

        private void TimerTick()
        {
            threadBool = true;
            while (threadBool)
            {
                this.Service.Havechange();
                Thread.Sleep(50);
            }
        }

        public void PageClosing(object sender, CancelEventArgs e)
        {
            threadBool = false;
        }

        void PageLoaded(object sender, RoutedEventArgs e)
        {
            this.Service.ControllerView.PropertyChangedMotor += OnPropertyControllerChanged;
            thread = new Thread(new ThreadStart(TimerTick));
            thread.Start();
        }

        private void OnPropertyControllerChanged(object sender, PropertyChangedEventArgs e)
        {
            Task.Run(() => MqttService.SendDataMotors(Service.ControllerView.MotorValues));
        }

        public void MQTTHaveChange(object sender, PropertyChangedEventArgs e)
        {
            var main = sender as MainWindow;
            if (main != null)
            {
                MqttService = main.MQTTService;
                Service.MQTTService = main.MQTTService;
            }
        }
        public void SetMQTT(MQTTService mQTTService)
        {
            MqttService = mQTTService;
            Service.MQTTService = mQTTService;
        }
    }
}
