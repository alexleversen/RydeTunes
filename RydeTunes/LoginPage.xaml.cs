using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace RydeTunes
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class LoginPage : ContentPage
	{
	    private LoginViewModel _viewModel;

		public LoginPage ()
		{
			InitializeComponent ();
		}

	    protected override void OnAppearing()
	    {
	        base.OnAppearing();

            _viewModel = (LoginViewModel)BindingContext;
            _viewModel.ReadyToNavigateToSuccess += ViewModel_ReadyToNavigateToSuccess;
	    }

        private void BackButton_OnTapped(object sender, EventArgs e)
	    {
	        // Check to see if there is anywhere to go back to
	        if (LoginWebView.CanGoBack)
	        {
	            LoginWebView.GoBack();
	        }
        }

	    private void ForwardButton_OnTapped(object sender, EventArgs e)
	    {
	        if (LoginWebView.CanGoForward)
	        {
	            LoginWebView.GoForward();
	        }
        }

	    private void LoginWebView_OnNavigated(object sender, WebNavigatedEventArgs e)
	    {
	        _viewModel.HandleUrlNavigation(e.Url);
	    }

	    private void ViewModel_ReadyToNavigateToSuccess(object sender, EventArgs e)
	    {
	        var page = new MainPage
	        {
	            BindingContext = new MainViewModel()
	        };
	        Application.Current.MainPage.Navigation.PushAsync(page);
            Application.Current.MainPage.Navigation.RemovePage(this);
	    }
    }
}