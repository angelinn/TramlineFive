﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TramlineFive.DataAccess;
using TramlineFive.Services;
using Xamarin.Forms;

namespace TramlineFive
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new Pages.MainPage();
            
            IDatabasePathService dbPathService = DependencyService.Get<IDatabasePathService>();
            using (TramlineFiveContext context = new TramlineFiveContext(dbPathService.Path))
            {
                // context.Database.EnsureCreated();
            }
        }

        protected override void OnStart()
        {
            // Handle when your app starts
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
