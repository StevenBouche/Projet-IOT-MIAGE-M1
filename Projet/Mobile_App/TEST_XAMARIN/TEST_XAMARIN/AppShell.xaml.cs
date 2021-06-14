using System;
using System.Collections.Generic;
using TEST_XAMARIN.ViewModels;
using TEST_XAMARIN.Views;
using Xamarin.Forms;

namespace TEST_XAMARIN
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(AboutPage), typeof(AboutPage));
        }

        private async void OnMenuItemClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}
