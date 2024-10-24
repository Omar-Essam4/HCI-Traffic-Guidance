//using System;
//using System.Net.Http;
//using System.Threading.Tasks;
//using System.IO;
//using Newtonsoft.Json.Linq;
//using System.Windows.Forms;
//using System.Linq;

//namespace TUIO
//{
//    public partial class Form1 : Form
//    {
//        private static readonly string mapboxUrl = "https://api.mapbox.com/styles/v1/mapbox/streets-v12/static";
//        private static readonly string trafficUrl = "https://api.mapbox.com/styles/v1/mapbox/traffic-day-v2/static";
//        private static readonly string accessToken = "sk.eyJ1Ijoib2FobWVkZCIsImEiOiJjbTJqY255bXMwM2U4MmxzYWE4MmtoOWdhIn0.6mkGuAUfoBgtXogIgrA18Q";  // Replace with your Mapbox token
//        private static readonly string busStationUrl = "https://api.mapbox.com/geocoding/v5/mapbox.places/bus%20station.json";
//        private static readonly string trainStationUrl = "https://api.mapbox.com/geocoding/v5/mapbox.places/train%20station.json";

//        public Form1()
//        {
//            InitializeComponent();
//            LoadData();
//        }

//        private async void LoadData()
//        {
//            string location = "31.2399,30.0382";  // Long, Lat for Cairo
//            int zoom = 13;  // Zoom level for the map

//            await GetMap(location, zoom);
//            await GetTrafficMap(location, zoom);
//            var busStations = await FindNearestBusStationsAsync(location);
//            await GetTrafficMapForBusStationsAsync(busStations, zoom, location);
//            var trainStations = await FindNearestTrainStationsAsync(location);
//            await GetTrafficMapForTrainStationsAsync(trainStations, zoom, location);
//        }
//        private void SaveFileManually(string filePath, byte[] bytes)
//        {
//            try
//            {
//                using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
//                {
//                    fs.Write(bytes, 0, bytes.Length);
//                }
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show($"Error writing file manually: {ex.Message}");
//            }
//        }
//        public async Task GetMap(string location, int zoom)
//        {
//            using (HttpClient client = new HttpClient())
//            {
//                try
//                {
//                    string requestUrl = $"{mapboxUrl}/{location},{zoom}/1200x1200@2x?access_token={accessToken}";
//                    HttpResponseMessage response = await client.GetAsync(requestUrl);
//                    if (response.IsSuccessStatusCode)
//                    {
//                        byte[] imageBytes = await response.Content.ReadAsByteArrayAsync();
//                        string filePath = "cairo_map.png";
//                        SaveFileManually(filePath, imageBytes);
//                        MessageBox.Show("Cairo map saved successfully at: " + filePath);
//                    }
//                    else
//                    {
//                        MessageBox.Show($"Error: {response.StatusCode}, {response.ReasonPhrase}");
//                    }
//                }
//                catch (Exception ex)
//                {
//                    MessageBox.Show($"Exception: {ex.Message}");
//                }
//            }
//        }

//        public async Task GetTrafficMap(string location, int zoom)
//        {
//            using (HttpClient client = new HttpClient())
//            {
//                try
//                {
//                    string requestUrl = $"{trafficUrl}/{location},{zoom}/1200x1200@2x?access_token={accessToken}";
//                    HttpResponseMessage response = await client.GetAsync(requestUrl);
//                    if (response.IsSuccessStatusCode)
//                    {
//                        byte[] imageBytes = await response.Content.ReadAsByteArrayAsync();
//                        string filePath = "cairo_traffic_map.png";
//                        SaveFileManually(filePath, imageBytes);
//                        MessageBox.Show("Cairo with traffic map saved successfully at: " + filePath);
//                    }
//                    else
//                    {
//                        MessageBox.Show($"Error: {response.StatusCode}, {response.ReasonPhrase}");
//                    }
//                }
//                catch (Exception ex)
//                {
//                    MessageBox.Show($"Exception: {ex.Message}");
//                }
//            }
//        }

//        public async Task<JToken> FindNearestBusStationsAsync(string location)
//        {
//            using (HttpClient client = new HttpClient())
//            {
//                try
//                {
//                    string requestUrl = $"{busStationUrl}?proximity={location}&access_token={accessToken}";
//                    HttpResponseMessage response = await client.GetAsync(requestUrl);
//                    if (response.IsSuccessStatusCode)
//                    {
//                        string jsonResponse = await response.Content.ReadAsStringAsync();
//                        JObject json = JObject.Parse(jsonResponse);
//                        var stations = json["features"];
//                        return stations;
//                    }
//                    else
//                    {
//                        MessageBox.Show($"Error: {response.StatusCode}, {response.ReasonPhrase}");
//                    }
//                }
//                catch (Exception ex)
//                {
//                    MessageBox.Show($"Exception: {ex.Message}");
//                }
//            }
//            return null;
//        }

//        public async Task<JToken> FindNearestTrainStationsAsync(string location)
//        {
//            using (HttpClient client = new HttpClient())
//            {
//                try
//                {
//                    string requestUrl = $"{trainStationUrl}?proximity={location}&access_token={accessToken}";
//                    HttpResponseMessage response = await client.GetAsync(requestUrl);
//                    if (response.IsSuccessStatusCode)
//                    {
//                        string jsonResponse = await response.Content.ReadAsStringAsync();
//                        JObject json = JObject.Parse(jsonResponse);
//                        var stations = json["features"];
//                        return stations;
//                    }
//                    else
//                    {
//                        MessageBox.Show($"Error: {response.StatusCode}, {response.ReasonPhrase}");
//                    }
//                }
//                catch (Exception ex)
//                {
//                    MessageBox.Show($"Exception: {ex.Message}");
//                }
//            }
//            return null;
//        }

//        public async Task GetTrafficMapForBusStationsAsync(JToken stations, int zoom, string location)
//        {
//            if (stations == null) return;

//            using (HttpClient client = new HttpClient())
//            {
//                try
//                {
//                    string markers = string.Join(",", stations.Select(station => $"pin-s+FF0000({station["geometry"]["coordinates"][0]},{station["geometry"]["coordinates"][1]})"));
//                    string requestUrl = $"{mapboxUrl}/{markers}/{location},{zoom}/1200x1200@2x?access_token={accessToken}";
//                    HttpResponseMessage response = await client.GetAsync(requestUrl);
//                    if (response.IsSuccessStatusCode)
//                    {
//                        byte[] imageBytes = await response.Content.ReadAsByteArrayAsync();
//                        string filePath = "map_cairo_with_bus_stations.png";
//                        SaveFileManually(filePath, imageBytes);
//                        MessageBox.Show("Traffic map image with bus station markers saved successfully at: " + filePath);
//                    }
//                    else
//                    {
//                        MessageBox.Show($"Error: {response.StatusCode}, {response.ReasonPhrase}");
//                    }
//                }
//                catch (Exception ex)
//                {
//                    MessageBox.Show($"Exception: {ex.Message}");
//                }
//            }
//        }

//        public async Task GetTrafficMapForTrainStationsAsync(JToken stations, int zoom, string location)
//        {
//            if (stations == null) return;

//            using (HttpClient client = new HttpClient())
//            {
//                try
//                {
//                    string markers = string.Join(",", stations.Select(station => $"pin-s+0000FF({station["geometry"]["coordinates"][0]},{station["geometry"]["coordinates"][1]})"));
//                    string requestUrl = $"{mapboxUrl}/{markers}/{location},{zoom}/1200x1200@2x?access_token={accessToken}";
//                    HttpResponseMessage response = await client.GetAsync(requestUrl);
//                    if (response.IsSuccessStatusCode)
//                    {
//                        byte[] imageBytes = await response.Content.ReadAsByteArrayAsync();
//                        string filePath = "map_cairo_with_train_stations.png";
//                        SaveFileManually(filePath, imageBytes);
//                        MessageBox.Show("Traffic map image with train station markers saved successfully at: " + filePath);
//                    }
//                    else
//                    {
//                        MessageBox.Show($"Error: {response.StatusCode}, {response.ReasonPhrase}");
//                    }
//                }
//                catch (Exception ex)
//                {
//                    MessageBox.Show($"Exception: {ex.Message}");
//                }
//            }
//        }
//    }
//}

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
        private static readonly string mapboxUrl = "https://api.mapbox.com/styles/v1/mapbox/streets-v12/static";
        private static readonly string trafficUrl = "https://api.mapbox.com/styles/v1/mapbox/traffic-day-v2/static";
        private static readonly string accessToken = "pk.eyJ1Ijoib2FobWVkZCIsImEiOiJjbTJqYno0cHgwM2NiMmtzZjFkeDJzajc2In0.SP7i6awoXHq4G5_re_JNNg";  // Replace with your Mapbox token
        private static readonly string busStationUrl = "https://api.mapbox.com/geocoding/v5/mapbox.places/bus%20station.json";
        private static readonly string trainStationUrl = "https://api.mapbox.com/geocoding/v5/mapbox.places/train%20station.json";
        public async Task RunAsync(string location, int zoom)
        {
            await GetMap(location, zoom);
            await GetTrafficMap(location, zoom);
            var busStations = await FindNearestBusStationsAsync(location);
            await GetTrafficMapForBusStationsAsync(busStations, zoom, location);
            var trainStations = await FindNearestTrainStationsAsync(location);
            await GetTrafficMapForTrainStationsAsync(trainStations, zoom, location);
        }

        private void SaveFileManually(string filePath, byte[] bytes)
        {
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(bytes, 0, bytes.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing file manually: {ex.Message}");
            }
        }

        public async Task GetMap(string location, int zoom)
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
                        SaveFileManually(filePath, imageBytes);
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

        public async Task GetTrafficMap(string location, int zoom)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string requestUrl = $"{trafficUrl}/{location},{zoom}/1200x1200@2x?access_token={accessToken}";
                    HttpResponseMessage response = await client.GetAsync(requestUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        byte[] imageBytes = await response.Content.ReadAsByteArrayAsync();
                        string filePath = "cairo_traffic_map.png";
                        SaveFileManually(filePath, imageBytes);
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

        public async Task<JToken> FindNearestBusStationsAsync(string location)
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
                        return json["features"];
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
            return null;
        }

        public async Task<JToken> FindNearestTrainStationsAsync(string location)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string requestUrl = $"{trainStationUrl}?proximity={location}&access_token={accessToken}";
                    HttpResponseMessage response = await client.GetAsync(requestUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        JObject json = JObject.Parse(jsonResponse);
                        return json["features"];
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
            return null;
        }

        public async Task GetTrafficMapForBusStationsAsync(JToken stations, int zoom, string location)
        {
            if (stations == null) return;

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string markers = string.Join(",", stations.Select(station => $"pin-s+FF0000({station["geometry"]["coordinates"][0]},{station["geometry"]["coordinates"][1]})"));
                    string requestUrl = $"{mapboxUrl}/{markers}/{location},{zoom}/1200x1200@2x?access_token={accessToken}";
                    HttpResponseMessage response = await client.GetAsync(requestUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        byte[] imageBytes = await response.Content.ReadAsByteArrayAsync();
                        string filePath = "map_cairo_with_bus_stations.png";
                        SaveFileManually(filePath, imageBytes);
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

        public async Task GetTrafficMapForTrainStationsAsync(JToken stations, int zoom, string location)
        {
            if (stations == null) return;

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string markers = string.Join(",", stations.Select(station => $"pin-s+0000FF({station["geometry"]["coordinates"][0]},{station["geometry"]["coordinates"][1]})"));
                    string requestUrl = $"{mapboxUrl}/{markers}/{location},{zoom}/1200x1200@2x?access_token={accessToken}";
                    HttpResponseMessage response = await client.GetAsync(requestUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        byte[] imageBytes = await response.Content.ReadAsByteArrayAsync();
                        string filePath = "map_cairo_with_train_stations.png";
                        SaveFileManually(filePath, imageBytes);
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