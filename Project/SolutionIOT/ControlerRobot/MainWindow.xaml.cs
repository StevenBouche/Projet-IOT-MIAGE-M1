using ControlerRobot.Services;
using System.ComponentModel;
using System.Threading;
using System.Windows;

namespace ControlerRobot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ControllerService Service { get; set; }
        Thread thread;
        bool threadBool = true;

        public MainWindow()
        {
            this.Service = new ControllerService(Dispatcher);
            DataContext = this.Service.ControllerView;

            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;

            InitializeComponent();
        }

        private void TimerTick()
        {
            while (threadBool)
            {
                this.Service.Havechange();
                Thread.Sleep(10);
            }
        }

        void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            threadBool = false;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

            thread = new Thread(new ThreadStart(TimerTick));
            thread.Start();
        }
    }
}
