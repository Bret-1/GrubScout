using FoodFinder.Models;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileSystemGlobbing;
using Newtonsoft.Json.Linq;
using System.Device.Location;
using System.IO;
using System.Numerics;
using System.Text;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FoodFinder.Controllers
{
    public class EstablishmentListController : Controller
    {
        public static string PlacesAPIKey;
        public IActionResult Index()
        {
            EstablishmentList est = new EstablishmentList();

            est.SortBySelectList = new List<SelectListItem>();
            est.SortBySelectList.Add(new SelectListItem { Text = "Rating"});
            est.SortBySelectList.Add(new SelectListItem { Text = "Price Level" });
            est.SortBySelectList.Add(new SelectListItem { Text = "Distance" });

            if(EstablishmentList.establishments == null || EstablishmentList.establishments.Count == 0)
                getLocalEstablishmentsSearchResult(est);
            return View(est);
        }
        [HttpPost]
        public IActionResult Index(EstablishmentList model)
        {
            var selected_sortby = model.SelectedSortBy;
            
            if (selected_sortby == null)
            {
                return RedirectToAction("Index");
            }
            if(selected_sortby == "Rating")
            {
                EstablishmentList.establishments = SortEstablishments(EstablishmentList.establishments, "rating", ascending: false);
            }
            else if(selected_sortby == "Price Level")
            {
                EstablishmentList.establishments = SortEstablishments(EstablishmentList.establishments, "pricelevel", ascending: true);
            } 
            else if (selected_sortby == "Distance")
            {
                // get distance somehow
                //model.establishments = SortEstablishments(model.establishments, "distance", ascending: true);
            }
            return RedirectToAction("Index");

        }

        public static List<Establishment> SortEstablishments(
        List<Establishment> establishments,
        string sortBy,
        bool ascending = true)
        {
            Func<Establishment, object> keySelector = sortBy.ToLower() switch
            {
                "rating" => e => e.Rating,
                "pricelevel" => e => e.PriceLevel,
                "name" => e => e.Name,
                //"distance" or "distancetouser" => e => e.Distance, //distance not implemented yet
                _ => e => e.Name // default sort if unknown key
            };

            return ascending
                ? establishments.OrderBy(keySelector).ToList()
                : establishments.OrderByDescending(keySelector).ToList();
        }

        private void getLocalEstablishmentsSearchResult(EstablishmentList est)
        {
            PlacesAPIKey = getPlacesAPIKey();
            GeoCoordinate cord = GetGeoCordinate();

            double current_latitude = cord.Latitude;
            double current_longitude = cord.Longitude;

            int radius = 5000; // 2 km
            string url = $"https://maps.googleapis.com/maps/api/place/nearbysearch/json" +
                     $"?location={current_latitude},{current_longitude}&radius={radius}&type=restaurant&key={PlacesAPIKey}";
            using HttpClient client = new HttpClient();
            string json = client.GetStringAsync(url).Result;

            JObject result = JObject.Parse(json);

            foreach (var place in result["results"])
            {
                string name = place["name"]?.ToString();
                string address = place["vicinity"]?.ToString();
                double rating = Double.Parse(place["rating"]?.ToString()); 
                bool open_now = place["opening_hours"]?["open_now"]?.ToString() == "True";
                string temp = place["price_level"]?.ToString();

                int price_level;
                if (temp != null)
                    price_level = Int32.Parse(temp);
                else
                    price_level = 4;
                JToken? photos = place["photos"];
                string photo_html = "";
                if(photos != null)
                {
                    photo_html = photos[0].ToString();
                }

                Console.WriteLine($"{name} — {address} — Rating: {rating}");
                Console.WriteLine($"Open?{open_now} - Price Level: {price_level} - {photo_html}");
                Establishment newEstablishment = new Establishment(name, address, rating, open_now, price_level, photo_html);
                
                newEstablishment.GetDistance(current_latitude, current_longitude);
                EstablishmentList.establishments.Add(newEstablishment);

            }


        }

        private GeoCoordinate GetGeoCordinate()
        {
            GeoCoordinateWatcher watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.Default);
            watcher.TryStart(false, TimeSpan.FromMilliseconds(3000));
            //Making sure that we can get the current location
            //TODO: add alternate message if location sharing is off
            int i = 0;
            while (watcher.Status != GeoPositionStatus.Ready)
            {
                if (watcher.Permission == GeoPositionPermission.Denied)
                {
                    Console.WriteLine("permission demied");
                }
                Thread.Sleep(200);
                i++;
                if (i > 10)
                    break;
            }
            return watcher.Position.Location;
        }

        public static string getPlacesAPIKey()
        {
            string filePath = @"../../API KEYS/places-api.txt";
            string readContents;
            using (StreamReader streamReader = new StreamReader(filePath, Encoding.UTF8))
            {
                readContents = streamReader.ReadToEnd();
            }
            Console.WriteLine(readContents);
            return readContents;

        }
    }
}
