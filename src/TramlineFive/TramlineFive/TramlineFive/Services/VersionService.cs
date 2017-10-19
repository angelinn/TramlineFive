﻿using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TramlineFive.Services
{
    public class VersionService
    {
        public static async Task<string> CheckForUpdates()
        {
            GitHubClient client = new GitHubClient(new ProductHeaderValue("TramlineFive.Xamarin"));
            IReadOnlyList<Release> res = await client.Repository.Release.GetAll("betrakiss", "TramlineFive.Xamarin");
            Release lastRelease = res.First();

            string version = Version.Plugin.CrossVersion.Current.Version.Substring(0, 5);
            if (String.Compare(lastRelease.TagName, version) < 0)
                return lastRelease.Url;

            return null;
        }
    }
}
