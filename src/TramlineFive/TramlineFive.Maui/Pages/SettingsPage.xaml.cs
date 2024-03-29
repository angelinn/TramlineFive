﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TramlineFive.Common.ViewModels;



namespace TramlineFive.Pages
{
	public partial class SettingsPage : ContentPage
	{
		public SettingsPage ()
		{
			InitializeComponent (); 
			(BindingContext as SettingsViewModel).Initialize(DisplayActionSheet);
		}

        protected override bool OnBackButtonPressed()
        {
			Shell.Current.GoToAsync("//Main");
			return true;
        }
    }
} 
