using System;
using System.Collections.ObjectModel;

namespace RydeTunes
{
    public class MainViewModel
    {
        public ObservableCollection<string> SongList { get; set; }

        public MainViewModel()
        {
            SongList = new ObservableCollection<string> { "Hello", "Hi", "How are you" };
        }
    }
}
