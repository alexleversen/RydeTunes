﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZXing.Mobile;

namespace RydeTunes
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class RiderPage : ContentPage
	{
        private string _qrCodeText;

		public RiderPage ()
		{
			InitializeComponent ();
		}
	}
}