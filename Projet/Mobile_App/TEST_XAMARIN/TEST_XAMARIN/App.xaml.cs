using System;
using TEST_XAMARIN.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TEST_XAMARIN
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
