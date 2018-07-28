using RydeTunes.Network;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace RydeTunes
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            SpotifyApi.Instance = new SpotifyApi();

            MainPage = new NavigationPage(new MainPage {BindingContext = new MainViewModel()});
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }

        private void PingSpotifyLoop()
        {


            //TODO: Loop every minute(?) and ping the playlist to check if it's empty
            //When playlist is empty and we care (a song has been added this session), raise SessionInvalidated (SessionInvalidated?.Invoke(this, EventArgs.Empty);
        }
    }
}
