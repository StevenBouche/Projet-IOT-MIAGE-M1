using MobileApp.ViewModels;
using System.ComponentModel;
using Xamarin.Forms;

namespace MobileApp.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}