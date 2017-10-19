﻿using Rg.Plugins.Popup.Services;
using SkgtService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TramlineFive.Pages.Popup;
using TramlineFive.Services;
using TramlineFive.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TramlineFive.Pages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class VirtualTablesPage : ContentPage
	{
        private bool appeared;
        public VirtualTablesViewModel VirtualTablesViewModel { get; private set; } = new VirtualTablesViewModel();

        public VirtualTablesPage ()
		{
			InitializeComponent ();
            BindingContext = VirtualTablesViewModel;
		}

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (!appeared)
            {
                appeared = true;
                await VirtualTablesViewModel.CheckForUpdatesAsync();
            }
        }

        private void OnVersionTapped(object sender, EventArgs e)
        {
            Device.OpenUri(new Uri(VirtualTablesViewModel.Version.ReleaseUrl));
        }

        private async void OnCheckClicked(object sender, EventArgs e)
        {
            try
            {
                IEnumerable<Line> lines = await VirtualTablesViewModel.LoadLinesAsync();

                ChooseLinePopup linesPickPopup = new ChooseLinePopup(lines);
                await PopupNavigation.PushAsync(linesPickPopup);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Exception", ex.Message, "OK");
            }
        }
    }
}