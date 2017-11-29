﻿using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TramlineFive.Common.Messages;
using TramlineFive.DataAccess.Domain;

namespace TramlineFive.Common.ViewModels
{
    public class FavouritesViewModel : BaseViewModel
    {
        public ObservableCollection<FavouriteDomain> Favourites { get; private set; }

        public ICommand RemoveCommand { get; private set; }

        public FavouritesViewModel()
        {
            MessengerInstance.Register<FavouriteAddedMessage>(this, (f) => Favourites.Insert(0, f.Added));
            RemoveCommand = new RelayCommand<FavouriteDomain>(async (f) => await RemoveFavouriteAsync(f));
        }

        private async Task RemoveFavouriteAsync(FavouriteDomain favourite)
        {
            if (await InteractionService.DisplayAlertAsync("", $"Премахване на {favourite.Name}?", "Да", "Не"))
            {
                await FavouriteDomain.RemoveAsync(favourite.StopCode);
                Favourites.Remove(favourite);

                InteractionService.DisplayToast($"{favourite.Name} е премахната");
            }
        }

        public async Task LoadFavouritesAsync()
        {
            Favourites = new ObservableCollection<FavouriteDomain>((await FavouriteDomain.TakeAsync()).Reverse());
            RaisePropertyChanged("Favourites");
        }

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
                    InteractionService.ChangeTab(0);
                    MessengerInstance.Send(new StopSelectedMessage(selected.StopCode));

                    selected = null;
                    RaisePropertyChanged();
                }
            }
        }
    }
}