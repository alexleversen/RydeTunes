using System.Collections.ObjectModel;

namespace RydeTunes
{
    class RiderPageViewModel
    {
        public ObservableCollection<string> SongList { get; set; }

        public RiderPageViewModel()
        {
            SongList = new ObservableCollection<string> { "Hello", "Hi", "How are you" };
        }
    }
}
