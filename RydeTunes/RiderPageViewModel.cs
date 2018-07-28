using System.Collections.ObjectModel;
using System.ComponentModel;

namespace RydeTunes
{
    public class RiderPageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<string> SongList { get; private set; }

        public RiderPageViewModel()
        {
            SongList = new ObservableCollection<string> { "Hello", "Hi", "How are you" };
        }
    }
}
