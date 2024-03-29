﻿using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Mapsui;
using Mapsui.UI.Maui;
using Mapsui.Widgets;
using Mapsui.Widgets.ScaleBar;
using Mapsui.Widgets.Zoom;
using Microsoft.Maui.Layouts;
using System;
using System.Threading.Tasks;
using TramlineFive.Common;
using TramlineFive.Common.Messages;
using TramlineFive.Common.Services;
using TramlineFive.Common.ViewModels;
using Map = Mapsui.Map;

namespace TramlineFive.Pages
{
    public partial class MapPage : ContentPage
    {
        private bool initialized;
        private bool isOpened;

        private MapService mapService;
        private Map nativeMap;
        //MapView map;
        //View slideMenu;

        public MapPage()
        {
            InitializeComponent();

            Messenger.Default.Register<ShowMapMessage>(this, async (m) => await ToggleMap(m));
            //Messenger.Default.Register<RefreshMapMessage>(this, m => map.Refresh());
            Messenger.Default.Register<UpdateLocationMessage>(this, m => map.MyLocationLayer.UpdateMyLocation(new Position(m.Position.Latitude, m.Position.Longitude)));
        }

        private async void OnMapTouchAction(object sender, SkiaSharp.Views.Maui.SKTouchEventArgs e)
        {
            if (e.ActionType == SkiaSharp.Views.Maui.SKTouchAction.Released)
            {
                await mapService.ShowNearbyStops(new MPoint(nativeMap.Navigator.Viewport.CenterX, nativeMap.Navigator.Viewport.CenterY), true);
                //map.Refresh();

                System.Diagnostics.Debug.WriteLine($"Show stops");
            }

            System.Diagnostics.Debug.WriteLine($"Touch: {e.ActionType} {nativeMap.Navigator.Viewport.CenterX} {nativeMap.Navigator.Viewport.CenterY}");
        }

        private async Task ShowVirtualTables(int linesCount)
        {
            int coef = linesCount > 2 ? 2 : linesCount;
            slideMenu.HeightRequest = Height * (coef + 1) * 0.20;


            //Animation animation = new Animation((h) => map.HeightRequest = h, map.HeightRequest, Height - slideMenu.HeightRequest + 30);

            await slideMenu.TranslateTo(0, 0, 400);
            //await map.LayoutTo(new Rect(0, 0, Width, Height - slideMenu.HeightRequest + 30));
            map.HeightRequest = Height - slideMenu.HeightRequest + 30;
            //AbsoluteLayout.SetLayoutFlags(map, AbsoluteLayoutFlags.PositionProportional | AbsoluteLayoutFlagks.WidthProportional);
            //AbsoluteLayout.SetLayoutBounds(map, new Rect(1, 1, 1, Height - slideMenu.HeightRequest + 30));
            //animation.Commit(map, "ShowMap", 256, 400);
        }

        private async Task HideVirtualTables()
        {
            //Animation animation = new Animation((h) => map.HeightRequest = h, map.HeightRequest, Height);

            map.HeightRequest = Height;

            //AbsoluteLayout.SetLayoutFlags(map, AbsoluteLayoutFlags.All);
            //AbsoluteLayout.SetLayoutBounds(map, new Rect(1, 1, 1, 1));
            //await map.LayoutTo(new Rect(0, 0, Width, Height));
            await slideMenu.TranslateTo(0, Height, 400);
            //animation.Commit(map, "Expand", 16, 400);
        }

        private async Task ToggleMap(ShowMapMessage message)
        {
            if (message.ElapsedMilliseconds > 0)
            {
                long difference = 400 - message.ElapsedMilliseconds;
                if (difference > 0)
                    await Task.Delay((int)difference);
            }

            
            Dispatcher.Dispatch(async () =>
            {
                isOpened = message.Show;

                if (!message.Show)
                    await HideVirtualTables();
                else if (message.Show)
                    await ShowVirtualTables(message.ArrivalsCount);
            });
        }

        protected override void OnAppearing()
        {
            if (initialized)
                return;

            initialized = true;
            Task task = (BindingContext as MapViewModel).LoadAsync();

            nativeMap = new Map
            {
                BackColor = Mapsui.Styles.Color.White,
                CRS = "EPSG:3857"
            };

            mapService = ServiceContainer.ServiceProvider.GetService<MapService>();
            Task _ = (BindingContext as MapViewModel).Initialize(nativeMap, nativeMap.Navigator);

            map.Map = nativeMap;
            map.TouchAction += OnMapTouchAction;

            if (VersionTracking.IsFirstLaunchEver)
                Messenger.Default.Send(new ChangePageMessage("Location"));
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            slideMenu.TranslationY = isOpened ? 0 : Height;
            map.HeightRequest = Height;
        }   
    }
}
