using System;
using RydeTunes.Network;
using Xamarin.Forms;

namespace RydeTunes
{
    public partial class MainPage : ContentPage
    {
        private MainViewModel _viewModel;

        public MainPage()
        {
            InitializeComponent();
            Title = "RydeTunes";
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if(_viewModel != null){
                _viewModel.ReadyToNavigate -= NavigateToRiderPage;
            }
            _viewModel = (MainViewModel)BindingContext;
            _viewModel.ReadyToNavigate += NavigateToRiderPage;
        }

        private void NavigateToRiderPage(object sender, EventArgs e)
        {
            var page = new RiderPage
            {
                BindingContext = new RiderPageViewModel()
            };
            NavigateAndSetAsRoot(page);
        }

        private void DriverOption_OnTapped(object sender, EventArgs e)
        {
            var page = new DriverDashboard
            {
                BindingContext = new DriverDashboardViewModel()
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
            Application.Current.MainPage.Navigation.PushAsync(new ApiTestPage{BindingContext = new ApiTestPageViewModel()});
        }
    }
}
