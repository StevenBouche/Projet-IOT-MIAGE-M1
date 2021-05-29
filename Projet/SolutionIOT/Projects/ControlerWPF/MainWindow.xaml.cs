using ControlerWPF.Configs;
using ControlerWPF.Controls;
using ControlerWPF.Services;
using MQTTLib.Config;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ControlerWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        private bool _connected = false;

        public bool Connected
        {
            get => _connected;
            set {
                if (_connected != value)
                {
                    SetProperty(ref _connected, value);
                    ConnectedColor = value ? "Green" : "Red";
                }
            }
        }

        private string _connectedColor = "Red";
        public string ConnectedColor
        {
            get => _connectedColor;
            set
            {
                if (_connectedColor != value)
                    SetProperty(ref _connectedColor, value);
            }
        }

        private MQTTService _mqttService;

        public MQTTService MQTTService { 
            get => _mqttService;
            set => SetPropertyMQTT(ref _mqttService, value);
        }

        public MQTTConfigClient ConfigClientMQTT { get; set; }
        public MQTTServiceConfig ConfigTopicMQTT { get; set; }


        public MainWindow()
        {
            //attach context of MainWindow to this class
            DataContext = this;

            //subscribe event load and close
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;

            InitializeComponent();
        }

        /// <summary>
        /// Execute when windows closing. Stop MQTT Service.
        /// </summary>
        async void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            await MQTTService.Disconnect();
        }

        /// <summary>
        /// Execute when windows has loaded. Subscribe to events MQTT connect and disconnect. And subcribe user control to event closing.
        /// After execute try connection MQTT. 
        /// </summary>
        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

            //create new mqtt service for send data
            if (MQTTService is null)
                MQTTService = new MQTTService(ConfigClientMQTT, ConfigTopicMQTT);

            MQTTService.ConnectedEvents = OnConnectMQTT;
            MQTTService.DisconnectedEvents = OnDisconnectMQTT;

            ControlerPage.SetMQTT(MQTTService);
            KinectPage.SetMQTT(MQTTService);

            Closing += ControlerPage.PageClosing;
            Closing += KinectPage.PageClosing;

            TryConnectionMQTT();

        }

        /// <summary>
        /// Try to connect MQTT broker while is not connected.
        /// </summary>
        private async void TryConnectionMQTT()
        {
            try
            {

                await MQTTService.Connect();

            } catch(Exception e)
            {
                OnDisconnectMQTT(null);
            }
            
        }

        /// <summary>
        /// On disconnect MQTT set variable Connected to false.
        /// </summary>
        private void OnDisconnectMQTT(MqttClientDisconnectedEventArgs args)
        {
            Connected = false;
            Task.Run(async () =>
            {
                await Task.Delay(2000);
                TryConnectionMQTT();
            });
        }

        /// <summary>
        /// On connect MQTT set variable Connected to true.
        /// </summary>
        private void OnConnectMQTT(MqttClientConnectedEventArgs args)
        {
            Connected = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public event PropertyChangedEventHandler PropertyMQTTChanged;

        protected bool SetPropertyMQTT<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(field, newValue))
            {

                field = newValue;

                if (Dispatcher.CheckAccess())
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                }
                else
                {
                    Dispatcher.BeginInvoke((Action)(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName))));
                }
                return true;
            }
            return false;
        }

        protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(field, newValue))
            {

                field = newValue;

                if (Dispatcher.CheckAccess())
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                }
                else
                {
                    Dispatcher.BeginInvoke((Action)(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName))));
                }
                return true;
            }
            return false;
        }
    }
}
