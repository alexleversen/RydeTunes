using System;
using Android.Util;
using Xamarin.Forms;

namespace RydeTunes
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void RiderOption_OnTapped(object sender, EventArgs e)
        {
            Log.Debug("d", "Calling ViewModel Constructor");
            var viewModel = new RiderPageViewModel();
            Log.Debug("d", "ViewModel Constructor called");
            var page = new RiderPage
            {
                BindingContext = viewModel
            };
            Application.Current.MainPage.Navigation.PushAsync(page);
        }

        private void DriverOption_OnTapped(object sender, EventArgs e)
        {
            var page = new DriverLoginPage
            {
                BindingContext = new DriverLoginViewModel()
            };
            Application.Current.MainPage.Navigation.PushAsync(page);
        }
    }
}
