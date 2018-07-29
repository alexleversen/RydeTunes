using System.Timers;
using RydeTunes.Network;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace RydeTunes
{
    public partial class App : Application
    {
        private bool _rideStarted;

        public App()
        {
            InitializeComponent();

            SpotifyApi.Instance = new SpotifyApi();

            MainPage = new NavigationPage(new LoginPage {BindingContext = new LoginViewModel()});
        }

        protected override void OnStart()
        {
            // Handle when your app starts
            PingSpotifyLoop();
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
            var timer = new Timer(60000)
            {
                AutoReset = true
            };
            //timer.Elapsed += (_,__) => PingSpotify(timer);
            timer.Start();

            //TODO: Loop every minute(?) and ping the playlist to check if it's empty
            //When playlist is empty and we care (a song has been added this session), raise SessionInvalidated (SessionInvalidated?.Invoke(this, EventArgs.Empty);
        }

        //private void PingSpotify(Timer timer)
        //{
        //    if(SpotifyApi.Instance.PlaylistIsEmpty("") && _rideStarted){
        //        timer.Stop();
                //SpotifyApi.Instance.DisconnectFromPlaylist();
        //        return;
        //    }
        //    else if (!_rideStarted) {
        //        _rideStarted = true;
        //    }
        //}
    }
}
