using System;
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
            var page = new RiderPage
            {
                BindingContext = new RiderPageViewModel()
            };
            Application.Current.MainPage.Navigation.PushAsync(new NavigationPage(page));
        }

        private void DriverOption_OnTapped(object sender, EventArgs e)
        {
            var page = new DriverLoginPage
            {
                BindingContext = new DriverLoginViewModel()
            };
            Application.Current.MainPage.Navigation.PushAsync(new NavigationPage(page));
        }
    }
}
