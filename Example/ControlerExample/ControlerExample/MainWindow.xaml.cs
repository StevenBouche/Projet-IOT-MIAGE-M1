using ControlerExample.Services;
using SharpDX.XInput;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Threading;

namespace ControlerExample
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
