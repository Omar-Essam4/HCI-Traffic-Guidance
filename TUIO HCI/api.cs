using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace TUIO
{
    public class MapBoxAPI
    {
        private static readonly string AccessToken = "YOUR_ACCESS_TOKEN"; // Replace with your Mapbox token
        private static readonly string BaseMapUrl = "https://api.mapbox.com/styles/v1/mapbox";
        private static readonly string GeocodingUrl = "https://api.mapbox.com/geocoding/v5/mapbox.places";

        public async Task RunAsync(string location, int zoom)
        {
            await GetMap($"{BaseMapUrl}/streets-v12/static", "map.png", location, zoom);
            await GetMap($"{BaseMapUrl}/traffic-day-v2/static", "traffic_map.png", location, zoom);

            string[] busStations = await FindNearestStationsAsync(location, "bus station", "FF0000");
            await GetStationMap(busStations, "map_with_bus_stations.png", location, zoom);

            string[] trainStations = await FindNearestStationsAsync(location, "train station", "0000FF");
            await GetStationMap(trainStations, "map_with_train_stations.png", location, zoom);
        }

        private async Task GetMap(string baseUrl, string filePath, string location, int zoom)
        {
            using (HttpClient client = new HttpClient())
            {
                string requestUrl = $"{baseUrl}/{location},{zoom}/1200x1200@2x?access_token={AccessToken}";
                try
                {
                    var response = await client.GetAsync(requestUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        var imageBytes = await response.Content.ReadAsByteArrayAsync();
                        File.WriteAllBytes(filePath, imageBytes);
                        Console.WriteLine($"Map saved as {filePath}");
                    }
                    else Console.WriteLine($"Error: {response.StatusCode}, {response.ReasonPhrase}");
                }
                catch (Exception ex) { Console.WriteLine($"Exception: {ex.Message}"); }
            }
        }

        private async Task<string[]> FindNearestStationsAsync(string location, string stationType, string color)
        {
            using (HttpClient client = new HttpClient())
            {
                string requestUrl = $"{GeocodingUrl}/{stationType}.json?proximity={location}&access_token={AccessToken}";
                try
                {
                    var response = await client.GetAsync(requestUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        var json = JObject.Parse(await response.Content.ReadAsStringAsync());
                        var stations = json["features"]
                            .Select(station => $"pin-s+{color}({station["geometry"]["coordinates"][0]},{station["geometry"]["coordinates"][1]})")
                            .ToArray();
                        return stations;
                    }
                    else Console.WriteLine($"Error: {response.StatusCode}, {response.ReasonPhrase}");
                }
                catch (Exception ex) { Console.WriteLine($"Exception: {ex.Message}"); }
                return Array.Empty<string>(); // Return an empty array if there was an error
            }
        }

        private async Task GetStationMap(string[] markers, string filePath, string location, int zoom)
        {
            if (markers == null || markers.Length == 0) return;
            using (HttpClient client = new HttpClient())
            {
                string markerString = string.Join(",", markers);
                string requestUrl = $"{BaseMapUrl}/streets-v12/static/{markerString}/{location},{zoom}/1200x1200@2x?access_token={AccessToken}";
                try
                {
                    var response = await client.GetAsync(requestUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        var imageBytes = await response.Content.ReadAsByteArrayAsync();
                        File.WriteAllBytes(filePath, imageBytes);
                        Console.WriteLine($"Station map saved as {filePath}");
                    }
                    else Console.WriteLine($"Error: {response.StatusCode}, {response.ReasonPhrase}");
                }
                catch (Exception ex) { Console.WriteLine($"Exception: {ex.Message}"); }
            }
        }
    }
}
