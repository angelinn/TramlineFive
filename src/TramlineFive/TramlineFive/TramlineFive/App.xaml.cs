﻿using GalaSoft.MvvmLight.Ioc;
using SkgtService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TramlineFive.Common.Services;
using TramlineFive.Common.ViewModels;
using TramlineFive.DataAccess;
using TramlineFive.Services;
using TramlineFive.Services.Main;
using Xamarin.Forms;

namespace TramlineFive
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            if (!SimpleIoc.Default.ContainsCreated<IApplicationService>())
            {
                SimpleIoc.Default.Register<IApplicationService>(() => new ApplicationService());
                SimpleIoc.Default.Register<INavigationService>(() => new NavigationService());
                SimpleIoc.Default.Register<MapService>();
            }

            Plugin.Iconize.Iconize.With(new Plugin.Iconize.Fonts.FontAwesomeModule());

            IPathService dbPathService = DependencyService.Get<IPathService>();
            StopsLoader.Initialize(dbPathService.BasePath);

            IPermissionService permissionService = DependencyService.Get<IPermissionService>();

            if (!permissionService.HasLocationPermissions())
                MainPage = new Pages.LocationPromptPage();
            else
                MainPage = new NavigationPage(new MasterPage());

            DependencyService.Get<IVersionCheckingService>().CreateTask();
        }

        protected override async void OnStart()
        {
            IPathService dbPathService = DependencyService.Get<IPathService>();
            TramlineFiveContext.DatabasePath = dbPathService.Path;
            await TramlineFiveContext.EnsureCreatedAsync();

            await SimpleIoc.Default.GetInstance<HistoryViewModel>().LoadHistoryAsync();
            await SimpleIoc.Default.GetInstance<FavouritesViewModel>().LoadFavouritesAsync();

            StopsLoader.OnStopsUpdated += OnStopsUpdated;
        }

        private void OnStopsUpdated(object sender, EventArgs e)
        {
            Application.Current.Properties["StopsUpdated"] = DateTime.Now;
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
