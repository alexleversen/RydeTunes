using System;
using RydeTunes.Network;
using Xamarin.Forms;

namespace RydeTunes
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            Title = "RydeTunes";
        }

        private void RiderOption_OnTapped(object sender, EventArgs e)
        {
            var page = new RiderPage
            {
                BindingContext = new RiderPageViewModel()
            };
            NavigateAndSetAsRoot(page);
        }

        private void DriverOption_OnTapped(object sender, EventArgs e)
        {
            var page = new DriverLoginPage
            {
                BindingContext = new DriverLoginViewModel()
            };
            NavigateAndSetAsRoot(page);
        }

        private void NavigateAndSetAsRoot(Page page)
        {
            Application.Current.MainPage.Navigation.PushAsync(page);
            Application.Current.MainPage.Navigation.RemovePage(this);
        }

        private void ApiOption_OnTapped(object sender, EventArgs e)
        {
            Application.Current.MainPage.Navigation.PushAsync(new ApiTestPage());
        }
    }
}
