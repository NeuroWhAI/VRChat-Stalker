﻿using System;
using System.Net.Http;
using System.Text;
using VRChatApi.Endpoints;

namespace VRChatApi
{
    public class VRChatApi
    {
        public RemoteConfig RemoteConfig { get; set; }
        public UserApi UserApi { get; set; }
        public FriendsApi FriendsApi { get; set; }
        public WorldApi WorldApi { get; set; }

        public VRChatApi(string username, string password)
        {
            // initialize endpoint classes
            RemoteConfig = new RemoteConfig();
            UserApi = new UserApi();
            FriendsApi = new FriendsApi();
            WorldApi = new WorldApi();

            // initialize http client
            // TODO: use the auth cookie
            if (Global.HttpClient == null)
            {
                Global.HttpClient = new HttpClient();
                Global.HttpClient.BaseAddress = new Uri("https://api.vrchat.cloud/api/1/");
                Global.HttpClient.DefaultRequestHeaders.Add("User-Agent", "VRChat-Stalker ");
            }

            string authEncoded = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));

            var header = Global.HttpClient.DefaultRequestHeaders;
            if (header.Contains("Authorization"))
            {
                header.Remove("Authorization");
            }
            header.Add("Authorization", $"Basic {authEncoded}");
        }
    }
}
