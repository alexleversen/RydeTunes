using System;
using RydeTunes.Network.DTO;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace RydeTunes
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class RiderPage : ContentPage
	{
		public RiderPage ()
		{
			InitializeComponent ();
		}

	    private void ListView_OnItemSelected(object sender, SelectedItemChangedEventArgs e)
	    {
	        ((RiderPageViewModel)BindingContext).AddSong((Song)e.SelectedItem);
	    }
	}
}