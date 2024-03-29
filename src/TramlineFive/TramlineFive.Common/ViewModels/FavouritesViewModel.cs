﻿using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using Mapsui;
using Microsoft.Extensions.DependencyInjection;
using SkgtService;
using SkgtService.Models;
using SkgtService.Models.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TramlineFive.Common.Messages;
using TramlineFive.Common.Models;
using TramlineFive.Common.Services;
using TramlineFive.DataAccess.Domain;

namespace TramlineFive.Common.ViewModels
{
    public class FavouritesViewModel : BaseViewModel
    {
        public ObservableCollection<FavouriteDomain> Favourites { get; private set; }

        public ICommand RemoveCommand { get; private set; }

        private readonly LocationService locationService;
        private bool firstLocalization = true;

        private readonly PublicTransport publicTransport;

        public FavouritesViewModel(LocationService locationService, PublicTransport publicTransport)
        {
            MessengerInstance.Register<FavouriteAddedMessage>(this, (f) => OnFavouriteAdded(f.Added));
            RemoveCommand = new RelayCommand<FavouriteDomain>(async (f) => await RemoveFavouriteAsync(f));

            MessengerInstance.Register<StopSelectedMessage>(this, async (sc) => await OnStopSelected(sc.Selected));
            MessengerInstance.Register<UpdateLocationMessage>(this, async (message) =>
            {
                if (firstLocalization && ApplicationService.GetBoolSetting(Settings.ShowStopOnLaunch, true))
                {
                    firstLocalization = false;
                    await OnNearestFavouriteRequested(message.Position);
                }
            });

            this.locationService = locationService;
            this.publicTransport = publicTransport;
        }

        private async Task OnNearestFavouriteRequested(Position location)
        {
            double minDistance = double.MaxValue;
            FavouriteDomain minDistanceFavourite = null;

            while (Favourites == null)
                await Task.Delay(100);

            foreach (FavouriteDomain favourite in Favourites)
            { 
                StopInformation stop = MapService.Stops.FirstOrDefault(s => s.Code == favourite.StopCode);
                double distance = locationService.GetDistance(location.Latitude, location.Longitude, stop.Lat, stop.Lon);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    minDistanceFavourite = favourite;
                }
            }

            if (minDistanceFavourite != null)
                MessengerInstance.Send(new StopSelectedMessage(minDistanceFavourite.StopCode, true));
        }

        private async Task OnStopSelected(string stopCode)
        {
            FavouriteDomain favourite = Favourites.FirstOrDefault(f => f.StopCode == stopCode);
            if (favourite != null)
            {
                await FavouriteDomain.IncrementAsync(favourite.StopCode);
                ++favourite.TimesClicked;
            }
        }

        private void OnFavouriteAdded(FavouriteDomain favourite)
        {
            favourite.Lines = publicTransport.FindStop(favourite.StopCode).Lines;
            Favourites.Add(favourite);

            RaisePropertyChanged("HasFavourites");

            ApplicationService.VibrateShort();
            MessengerInstance.Send(new FavouritesChangedMessage(Favourites.ToList()));
        }

        private async Task RemoveFavouriteAsync(FavouriteDomain favourite)
        {
            if (await ApplicationService.DisplayAlertAsync("", $"Премахване на {favourite.Name}?", "Да", "Не"))
            {
                await FavouriteDomain.RemoveAsync(favourite.StopCode);
                Favourites.Remove(favourite);

                ApplicationService.DisplayToast($"{favourite.Name} е премахната");
                ApplicationService.VibrateShort();

                RaisePropertyChanged("HasFavourites");

                MessengerInstance.Send(new FavouritesChangedMessage(Favourites.ToList()));
            }
        }

        public async Task LoadFavouritesAsync()
        {
            IsLoading = true;

            Favourites = new ObservableCollection<FavouriteDomain>((await FavouriteDomain.TakeAsync()).OrderByDescending(f => f.TimesClicked));
            foreach (FavouriteDomain favourite in Favourites)
                favourite.Lines = publicTransport.FindStop(favourite.StopCode).Lines;

            IsLoading = false;

            RaisePropertyChanged("Favourites");
            RaisePropertyChanged("HasFavourites");

            MessengerInstance.Send(new FavouritesChangedMessage(Favourites.ToList()));
        }

        public bool HasFavourites => (Favourites == null || Favourites.Count == 0) && !isLoading;

        private FavouriteDomain selected;
        public FavouriteDomain Selected
        {
            get
            {
                return selected;
            }
            set
            {
                selected = value;
                RaisePropertyChanged();

                if (value != null)
                {
                    ServiceContainer.ServiceProvider.GetService<MainViewModel>().ChangeViewCommand.Execute("Map");
                    MessengerInstance.Send(new StopSelectedMessage(selected.StopCode, true));

                    selected = null;
                    RaisePropertyChanged();
                }
            }
        }

        private bool isLoading = true;
        public bool IsLoading
        {
            get
            {
                return isLoading;
            }
            set
            {
                isLoading = value;
                RaisePropertyChanged();
            }
        }
    }
}
