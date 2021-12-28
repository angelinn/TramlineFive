﻿using Newtonsoft.Json;
using SkgtService.Models.Locations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SkgtService
{
    public static class StopsLoader
    {
        private const string URL = "https://routes.sofiatraffic.bg/resources/stops-bg.json";
        private static string PATH = String.Empty;

        public static event EventHandler OnStopsUpdated;

        public static void Initialize(string basePath)
        {
            PATH = Path.Combine(basePath, "stops.json");
        }

        public static async Task<List<StopLocation>> LoadStopsAsync()
        {
            if (!File.Exists(PATH))
            {
                await UpdateStopsAsync();
            }

            string json = File.ReadAllText(PATH);
            return JsonConvert.DeserializeObject<List<StopLocation>>(json);
        }

        public static List<StopLocation> LoadStops(Stream stream)
        {
            string json = String.Empty;
            using (StreamReader reader = new StreamReader(stream))
            {
                json = reader.ReadToEnd();
            }
            return JsonConvert.DeserializeObject<List<StopLocation>>(json);
        }

        public static async Task UpdateStopsAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                byte[] stops = await client.GetByteArrayAsync(URL);
                File.WriteAllBytes(PATH, stops);

                OnStopsUpdated?.Invoke(null, new EventArgs());
            }
        }
    }
}
