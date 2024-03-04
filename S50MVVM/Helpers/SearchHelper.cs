﻿using Newtonsoft.Json;
using RestSharp;
using SpotifyWPFSearch.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static SpotifyWPFSearch.Models.SpotifySearch;

namespace SpotifyWPFSearch.Helpers
{
    public static class SearchHelper
    {
        public static Token token { get; set; }

        public static async Task GetTokenAsync() 
        {
            #region SecretVault
            string clientID = "24e67cd5a9504357807fdfb8a3afeebd";

            string clientSecret = "493cc283c15346cebc84de681deec5f7";
            #endregion

            string auth = Convert.ToBase64String(Encoding.UTF8.GetBytes(clientID + ":" + clientSecret));

            List<KeyValuePair<string, string>> args = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            };

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Basic {auth}");
            HttpContent content = new FormUrlEncodedContent(args);

            HttpResponseMessage resp = await client.PostAsync("https://accounts.spotify.com/api/token", content);
            string msg = await resp.Content.ReadAsStringAsync();

            token = JsonConvert.DeserializeObject<Token>(msg);
        }

        public static SpotifyResult SearchArtistOrSong(string searchWord) 
        {
            var client = new RestClient("https://api.spotify.com/v1/search");
            client.AddDefaultHeader("Authorization", $"Bearer {token.access_token}");
            var request = new RestRequest($"?q={searchWord}&type=artist", Method.GET);
            var response = client.Execute(request);

            if (response.IsSuccessful)
            {
                var result = JsonConvert.DeserializeObject<SpotifyResult>(response.Content);
                return result;
            }
            else
            {
                return null;
            }
                
        }
    }
}
