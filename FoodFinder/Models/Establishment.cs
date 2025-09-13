using FoodFinder.Controllers;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium.BiDi.Script;
using System;
using System.Net.Http;

namespace FoodFinder.Models
{
    public class Establishment
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public string Address { get; set; }
        public double Rating { get; set; } // out of 5 stars
        public bool OpenNow { get; set; }
        public int PriceLevel { get; set; } // out of 3
        public string PhotoHtml { get; set; }
        public string WalkingDistance { get; set; } //distance from users current location
        public string WalkingTime { get; set; }
        public string DrivingDistance { get; set; }
        public string DrivingTime { get; set; }

        public Establishment(string name, string address, double rating, bool open_now, int price_level, string photo_html) 
        {
            Name = name;
            Address = address;
            Rating = rating;
            OpenNow = open_now;
            PriceLevel = price_level;
            PhotoHtml = photo_html;
        }
        public Establishment() { }

        public void GetDistance(double origin_latitude, double origin_longitude)
        {
            string encodedAddress = Uri.EscapeDataString(Address);
            string address_url = $"https://maps.googleapis.com/maps/api/geocode/json?address={encodedAddress}&key={EstablishmentListController.PlacesAPIKey}";
            var http_client = new HttpClient();
            var response = http_client.GetStringAsync(address_url);
            var json = JObject.Parse(response.Result);

            var location = json["results"]?[0]?["geometry"]?["location"];
            if (location == null)
            {
                WalkingDistance = "";
                WalkingTime = "";
                DrivingDistance = "";
                DrivingTime = "";
                return;
            }
            double destination_latitude = (double)location["lat"];
            double destination_longitude = (double)location["lng"];


            string walking_url = $"https://maps.googleapis.com/maps/api/distancematrix/json" +
                 $"?origins={origin_latitude},{origin_longitude}" +
                 $"&destinations={destination_latitude},{destination_longitude}" +
                 $"&mode=walking" +
                 $"&units=imperial" +
                 $"&key={EstablishmentListController.PlacesAPIKey}";

            string driving_url = $"https://maps.googleapis.com/maps/api/distancematrix/json" +
                 $"?origins={origin_latitude},{origin_longitude}" +
                 $"&destinations={destination_latitude},{destination_longitude}" +
                 $"&mode=driving" +
                 $"&units=imperial" +
                 $"&key={EstablishmentListController.PlacesAPIKey}";

            var walking_directions_response = http_client.GetStringAsync(walking_url);
            var walking_directions_json = JObject.Parse(walking_directions_response.Result);

            if (walking_directions_json == null)
            {
                WalkingDistance = "";
                WalkingTime = "";
            }
            else
            {
                var element = walking_directions_json["rows"]?[0]?["elements"]?[0];
                if (element?["status"]?.ToString() == "OK")
                {
                    WalkingDistance = (string)element["distance"]?["text"];
                    WalkingTime = (string)element["duration"]?["text"];
                }
            }

            var driving_directions_response = http_client.GetStringAsync(driving_url);
            var driving_directions_json = JObject.Parse(driving_directions_response.Result);

            if (driving_directions_json == null)
            {
                DrivingDistance = "";
                DrivingTime = "";
            }
            else
            {
                var element = driving_directions_json["rows"]?[0]?["elements"]?[0];
                if (element?["status"]?.ToString() == "OK")
                {
                    DrivingDistance = (string)element["distance"]?["text"];
                    DrivingTime = (string)element["duration"]?["text"];
                }
            }
        }
    }
}
