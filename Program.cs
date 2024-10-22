using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json.Linq;

namespace MapboxExample
{
    class Program
    {
        private static readonly string mapboxUrl = "https://api.mapbox.com/styles/v1/mapbox/streets-v12/static";
        private static readonly string trafficUrl = "https://api.mapbox.com/styles/v1/mapbox/traffic-day-v2/static";
        private static readonly string accessToken = "sk.eyJ1Ijoib2FobWVkZCIsImEiOiJjbTJqY255bXMwM2U4MmxzYWE4MmtoOWdhIn0.6mkGuAUfoBgtXogIgrA18Q";  // Replace with your Mapbox token
        private static readonly string busStationUrl = "https://api.mapbox.com/geocoding/v5/mapbox.places/bus%20station.json"; // Example URL for bus station search
        private static readonly string trainStationUrl = "https://api.mapbox.com/geocoding/v5/mapbox.places/train%20station.json"; // URL for train station search

        static async Task Main(string[] args)
        {
            string location = "31.2399,30.0382";  // Long, Lat for Cairo
            int zoom = 13;  // Zoom level for the map

            await GetMap(location, zoom);

            await GetTrafficMap(location, zoom);

            // Call the method to find nearest bus stations
            var busStations = await FindNearestBusStationsAsync(location);

            // Call the method to retrieve and save the traffic map image with bus station markers
            await GetTrafficMapForBusStationsAsync(busStations, zoom, location);

            // Find nearest train stations
            var trainStations = await FindNearestTrainStationsAsync(location);

            // Generate a traffic map with train station markers and save it
            await GetTrafficMapForTrainStationsAsync(trainStations, zoom, location);
        }
        public static async Task GetMap(string location, int zoom)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string requestUrl = $"{mapboxUrl}/{location},{zoom}/1200x1200@2x?access_token={accessToken}";
                    HttpResponseMessage response = await client.GetAsync(requestUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        byte[] imageBytes = await response.Content.ReadAsByteArrayAsync();
                        string filePath = "cairo_map.png";
                        await File.WriteAllBytesAsync(filePath, imageBytes);
                        Console.WriteLine("Cairo map saved successfully at: " + filePath);
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode}, {response.ReasonPhrase}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                }
            }
        }
        public static async Task GetTrafficMap(string location, int zoom)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string requestUrl = $"{trafficUrl}/{location},{zoom}/1200x1200@2x?access_token={accessToken}";
                    //Console.WriteLine(requestUrl);

                    HttpResponseMessage response = await client.GetAsync(requestUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        byte[] imageBytes = await response.Content.ReadAsByteArrayAsync();
                        string filePath = "cairo_traffic_map.png";
                        await File.WriteAllBytesAsync(filePath, imageBytes);
                        Console.WriteLine("Cairo with traffic map saved successfully at: " + filePath);
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode}, {response.ReasonPhrase}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                }
            }
        }
        public static async Task GetTrafficMapForBusStationsAsync(JToken stations, int zoom, string location)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string markers = string.Join(",", stations.Select(station => $"pin-s+FF0000({station["geometry"]["coordinates"][0]},{station["geometry"]["coordinates"][1]})"));
                    string requestUrl = $"{mapboxUrl}/{markers}/{location},{zoom}/1200x1200@2x?access_token={accessToken}";
                    //Console.WriteLine(requestUrl);

                    HttpResponseMessage response = await client.GetAsync(requestUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        byte[] imageBytes = await response.Content.ReadAsByteArrayAsync();
                        string filePath = "map_cairo_with_bus_stations.png";
                        await File.WriteAllBytesAsync(filePath, imageBytes);

                        Console.WriteLine("Traffic map image with bus station markers saved successfully at: " + filePath);
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode}, {response.ReasonPhrase}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                }
            }
        }

        public static async Task<JToken> FindNearestBusStationsAsync(string location)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string requestUrl = $"{busStationUrl}?proximity={location}&access_token={accessToken}";

                    HttpResponseMessage response = await client.GetAsync(requestUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        JObject json = JObject.Parse(jsonResponse);
                        var stations = json["features"];

                        Console.WriteLine("Nearest Bus Stations:");
                        foreach (var station in stations)
                        {
                            string name = station["text"].ToString();
                            var coordinates = station["geometry"]["coordinates"];
                            double longitude = (double)coordinates[0];
                            double latitude = (double)coordinates[1];

                            Console.WriteLine($"Name: {name}, Longitude: {longitude}, Latitude: {latitude}");
                        }

                        return stations; // Return stations for use in the map
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode}, {response.ReasonPhrase}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                }
            }
            return null; // Return null if the request fails
        }

        public static async Task<JToken> FindNearestTrainStationsAsync(string location)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // Construct the request URL for train station search using proximity to a location
                    string requestUrl = $"{trainStationUrl}?proximity={location}&access_token={accessToken}";

                    // Send request to the Mapbox Geocoding API
                    HttpResponseMessage response = await client.GetAsync(requestUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        // Parse the JSON response and extract train stations
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        JObject json = JObject.Parse(jsonResponse);
                        var stations = json["features"];

                        // Log details of the nearest train stations
                        Console.WriteLine("Nearest Train Stations:");
                        foreach (var station in stations)
                        {
                            string name = station["text"].ToString();
                            var coordinates = station["geometry"]["coordinates"];
                            double longitude = (double)coordinates[0];
                            double latitude = (double)coordinates[1];

                            Console.WriteLine($"Name: {name}, Longitude: {longitude}, Latitude: {latitude}");
                        }

                        return stations; // Return stations for use in the map
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode}, {response.ReasonPhrase}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                }
            }

            return null; // Return null if the request fails
        }
        public static async Task GetTrafficMapForTrainStationsAsync(JToken stations, int zoom, string location)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // Create markers for each train station using the coordinates
                    string markers = string.Join(",", stations.Select(station =>
                        $"pin-s+0000FF({station["geometry"]["coordinates"][0]},{station["geometry"]["coordinates"][1]})")); // Blue pin for train stations

                    // Construct the request URL to fetch the map
                    string requestUrl = $"{mapboxUrl}/{markers}/{location},{zoom}/1200x1200@2x?access_token={accessToken}";

                    // Send request to the Mapbox API
                    HttpResponseMessage response = await client.GetAsync(requestUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        // Get the map image as byte array and save it as a PNG file
                        byte[] imageBytes = await response.Content.ReadAsByteArrayAsync();
                        string filePath = "map_cairo_with_train_stations.png";
                        await File.WriteAllBytesAsync(filePath, imageBytes);

                        Console.WriteLine("Traffic map image with train station markers saved successfully at: " + filePath);
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode}, {response.ReasonPhrase}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                }
            }
        }
    }
}