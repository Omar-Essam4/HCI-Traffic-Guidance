/*
	TUIO C# Demo - part of the reacTIVision project
	Copyright (c) 2005-2016 Martin Kaltenbrunner <martin@tuio.org>

	This program is free software; you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation; either version 2 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with this program; if not, write to the Free Software
	Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*/

using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections;
using System.Threading;
using TUIO;
using System.IO;
using System.Drawing.Drawing2D;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Linq;
public class TuioDemo : Form , TuioListener
{
    private TuioClient client;
    private Dictionary<long,TuioObject> objectList;
	private Dictionary<long,TuioCursor> cursorList;
	private Dictionary<long,TuioBlob> blobList;

	public static int width, height;
	private int window_width =  640;
	private int window_height = 480;
	private int window_left = 0;
	private int window_top = 0;
	private int screen_width = Screen.PrimaryScreen.Bounds.Width;
	private int screen_height = Screen.PrimaryScreen.Bounds.Height;
	private string objectImagePath;
	private string backgroundImagePath;
    public string location = "31.2399,30.0382";  // Long, Lat for Cairo
    public int zoom = 13;  // Zoom level for the map
    private static readonly string mapboxUrl = "https://api.mapbox.com/styles/v1/mapbox/streets-v12/static";
    private static readonly string trafficUrl = "https://api.mapbox.com/styles/v1/mapbox/traffic-day-v2/static";
    private static readonly string accessToken = "sk.eyJ1Ijoib2FobWVkZCIsImEiOiJjbTJqY255bXMwM2U4MmxzYWE4MmtoOWdhIn0.6mkGuAUfoBgtXogIgrA18Q";  // Replace with your Mapbox token
    private static readonly string busStationUrl = "https://api.mapbox.com/geocoding/v5/mapbox.places/bus%20station.json"; // Example URL for bus station search
    private static readonly string trainStationUrl = "https://api.mapbox.com/geocoding/v5/mapbox.places/train%20station.json"; // URL for train station search

    private bool fullscreen;
	private Image img;

    private bool verbose;

	

	Font font = new Font("Arial", 10.0f);
	SolidBrush fntBrush = new SolidBrush(Color.White);
	SolidBrush bgrBrush = new SolidBrush(Color.FromArgb(0,0,64));
	SolidBrush curBrush = new SolidBrush(Color.FromArgb(192, 0, 192));
	SolidBrush objBrush = new SolidBrush(Color.FromArgb(64, 0, 0));
	SolidBrush blbBrush = new SolidBrush(Color.FromArgb(64, 64, 64));
	Pen curPen = new Pen(new SolidBrush(Color.Blue), 1);
	

		public TuioDemo(int port) {
		
			verbose = false;
			fullscreen = false;
			width = window_width;
			height = window_height;

			this.ClientSize = new System.Drawing.Size(width, height);
			this.Name = "TuioDemo";
			this.Text = "TuioDemo";
			
			this.Closing+=new CancelEventHandler(Form_Closing);
			this.KeyDown+=new KeyEventHandler(Form_KeyDown);

			this.SetStyle( ControlStyles.AllPaintingInWmPaint |
							ControlStyles.UserPaint |
							ControlStyles.DoubleBuffer, true);

			objectList = new Dictionary<long,TuioObject>(128);
			cursorList = new Dictionary<long,TuioCursor>(128);
			blobList   = new Dictionary<long,TuioBlob>(128);
			try
			{
			    img = Image.FromFile("cairo_map.png");
			    this.BackgroundImage = img;
			}
			catch
			{
				
			    //Task task = GetMap(location, zoom);
			    //Image img = new Bitmap(@"cairo_map.png");
			    //this.BackgroundImage = img;
			    MessageBox.Show("dskdgiuwd");
			}

        client = new TuioClient(port);
			client.addTuioListener(this);

			client.connect();
		}

		private void Form_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e) {

 			if ( e.KeyData == Keys.F1) {
	 			if (fullscreen == false) {

					width = screen_width;
					height = screen_height;

					window_left = this.Left;
					window_top = this.Top;

					this.FormBorderStyle = FormBorderStyle.None;
		 			this.Left = 0;
		 			this.Top = 0;
		 			this.Width = screen_width;
		 			this.Height = screen_height;

		 			fullscreen = true;
	 			} else {

					width = window_width;
					height = window_height;

		 			this.FormBorderStyle = FormBorderStyle.Sizable;
		 			this.Left = window_left;
		 			this.Top = window_top;
		 			this.Width = window_width;
		 			this.Height = window_height;

		 			fullscreen = false;
	 			}
 			} else if ( e.KeyData == Keys.Escape) {
				this.Close();

 			} else if ( e.KeyData == Keys.V ) {
 				verbose=!verbose;
 			}

 		}

		private void Form_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			client.removeTuioListener(this);

			client.disconnect();
			System.Environment.Exit(0);
		}

		public void addTuioObject(TuioObject o) {
			lock(objectList) {
				objectList.Add(o.SessionID,o);
			} if (verbose) Console.WriteLine("add obj "+o.SymbolID+" ("+o.SessionID+") "+o.X+" "+o.Y+" "+o.Angle);
		}

		public void updateTuioObject(TuioObject o) {

			if (verbose) Console.WriteLine("set obj "+o.SymbolID+" "+o.SessionID+" "+o.X+" "+o.Y+" "+o.Angle+" "+o.MotionSpeed+" "+o.RotationSpeed+" "+o.MotionAccel+" "+o.RotationAccel);
		}

		public void removeTuioObject(TuioObject o) {
			lock(objectList) {
				objectList.Remove(o.SessionID);
			}
			if (verbose) Console.WriteLine("del obj "+o.SymbolID+" ("+o.SessionID+")");
		}

		public void addTuioCursor(TuioCursor c) {
			lock(cursorList) {
				cursorList.Add(c.SessionID,c);
			}
			if (verbose) Console.WriteLine("add cur "+c.CursorID + " ("+c.SessionID+") "+c.X+" "+c.Y);
		}

		public void updateTuioCursor(TuioCursor c) {
			if (verbose) Console.WriteLine("set cur "+c.CursorID + " ("+c.SessionID+") "+c.X+" "+c.Y+" "+c.MotionSpeed+" "+c.MotionAccel);
		}

		public void removeTuioCursor(TuioCursor c) {
			lock(cursorList) {
				cursorList.Remove(c.SessionID);
			}
			if (verbose) Console.WriteLine("del cur "+c.CursorID + " ("+c.SessionID+")");
 		}

		public void addTuioBlob(TuioBlob b) {
			lock(blobList) {
				blobList.Add(b.SessionID,b);
			}
			if (verbose) Console.WriteLine("add blb "+b.BlobID + " ("+b.SessionID+") "+b.X+" "+b.Y+" "+b.Angle+" "+b.Width+" "+b.Height+" "+b.Area);
		}

		public void updateTuioBlob(TuioBlob b) {
		
			if (verbose) Console.WriteLine("set blb "+b.BlobID + " ("+b.SessionID+") "+b.X+" "+b.Y+" "+b.Angle+" "+b.Width+" "+b.Height+" "+b.Area+" "+b.MotionSpeed+" "+b.RotationSpeed+" "+b.MotionAccel+" "+b.RotationAccel);
		}

		public void removeTuioBlob(TuioBlob b) {
			lock(blobList) {
				blobList.Remove(b.SessionID);
			}
			if (verbose) Console.WriteLine("del blb "+b.BlobID + " ("+b.SessionID+")");
		}

		public void refresh(TuioTime frameTime) {
			Invalidate();
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
                    File.WriteAllBytes(filePath, imageBytes);

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
    public static async Task GetTrafficMapForBusStationsAsync(JToken stations, int zoom, string location)
    {
        using (HttpClient client = new HttpClient())
        {
            try
            {
                string markers = string.Join(",", stations.Select(station => $"pin-s+FF0000({station["geometry"]["coordinates"][0]},{station["geometry"]["coordinates"][1]})"));
                string requestUrl = $"{trafficUrl}/{markers}/{location},{zoom}/1200x1200@2x?access_token={accessToken}";
                //Console.WriteLine(requestUrl);

                HttpResponseMessage response = await client.GetAsync(requestUrl);
                if (response.IsSuccessStatusCode)
                {
                    byte[] imageBytes = await response.Content.ReadAsByteArrayAsync();
                    string filePath = "map_cairo_with_bus_stations.png";
                    File.WriteAllBytes(filePath, imageBytes);

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

                        //Console.WriteLine($"Name: {name}, Longitude: {longitude}, Latitude: {latitude}");
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
                    File.WriteAllBytes(filePath, imageBytes);
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
		                File.WriteAllBytes(filePath, imageBytes);
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
		
    protected override void OnPaintBackground(PaintEventArgs pevent)
		{
			// Getting the graphics object
			Graphics g = pevent.Graphics;
			g.Clear(Color.White);
			if(img != null)
			{
				g.DrawImage(img, new Rectangle(0, 0, width, height));
			}
			g.FillRectangle(bgrBrush, new Rectangle(0,0,width,height));
			// draw the cursor path
			if (cursorList.Count > 0) {
 			lock(cursorList) {
			foreach (TuioCursor tcur in cursorList.Values) {
					List<TuioPoint> path = tcur.Path;
					TuioPoint current_point = path[0];

					for (int i = 0; i < path.Count; i++) {
						TuioPoint next_point = path[i];
						g.DrawLine(curPen, current_point.getScreenX(width), current_point.getScreenY(height), next_point.getScreenX(width), next_point.getScreenY(height));
						current_point = next_point;
					}
					g.FillEllipse(curBrush, current_point.getScreenX(width) - height / 100, current_point.getScreenY(height) - height / 100, height / 50, height / 50);
					g.DrawString(tcur.CursorID + "", font, fntBrush, new PointF(tcur.getScreenX(width) - 10, tcur.getScreenY(height) - 10));
				}
			}
		}
			// draw the objects
			if (objectList.Count > 0) {
 				lock(objectList) {
					foreach (TuioObject tobj in objectList.Values) {
						int ox = tobj.getScreenX(width);
						int oy = tobj.getScreenY(height);
						int size = height / 10;

                    switch (tobj.SymbolID)
                    {
                        case 0:                           
                            objectImagePath = Path.Combine(Environment.CurrentDirectory, "cairo_traffic_map.png");
                            backgroundImagePath = Path.Combine(Environment.CurrentDirectory, "cairo_traffic_map.png");                            
                            break;
                        case 1:
                            objectImagePath = Path.Combine(Environment.CurrentDirectory, "map_cairo_with_bus_stations.png");
                            backgroundImagePath = Path.Combine(Environment.CurrentDirectory, "map_cairo_with_bus_stations.png");
                            break;
                        case 2:
                            objectImagePath = Path.Combine(Environment.CurrentDirectory, "map_cairo_with_train_stations.png");
                            backgroundImagePath = Path.Combine(Environment.CurrentDirectory, "map_cairo_with_train_stations.png");
                            break;
                        default:
                            // Use default rectangle for other IDs
                            g.FillRectangle(objBrush, new Rectangle(ox - size / 2, oy - size / 2, size, size));
                            g.DrawString(tobj.SymbolID + "", font, fntBrush, new PointF(ox - 10, oy - 10));
                            continue;
                    }
                    try
                    {
                        // Draw background image without rotation
                        if (File.Exists(backgroundImagePath))
                        {
                            using (Image bgImage = Image.FromFile(backgroundImagePath))
                            {
                                g.DrawImage(bgImage, new Rectangle(new Point(0, 0), new Size(width, height)));
                            }
                        }
                        else
                        {

                            Console.WriteLine($"Background image not found: {backgroundImagePath}");
                        }

                        // Draw object image with rotation
                        if (File.Exists(objectImagePath))
                        {
                            using (Image objectImage = Image.FromFile(objectImagePath))
                            {
                                // Save the current state of the graphics object
                                GraphicsState state = g.Save();

                                // Apply transformations for rotation
                                g.TranslateTransform(ox, oy);
                                g.RotateTransform((float)(tobj.Angle / Math.PI * 180.0f));
                                g.TranslateTransform(-ox, -oy);

                                // Draw the rotated object
                                g.DrawImage(objectImage, new Rectangle(ox - size / 2, oy - size / 2, size, size));

                                // Restore the graphics state
                                g.Restore(state);
                            }
                        }
                        else
                        {
                            if (backgroundImagePath == Path.Combine(Environment.CurrentDirectory, "cairo_traffic_map.png") || objectImagePath == Path.Combine(Environment.CurrentDirectory, "cairo_traffic_map.png"))
                            {
                                Task task = GetTrafficMap(location, zoom);
                                objectImagePath = Path.Combine(Environment.CurrentDirectory, "pic1.jpg");
                                backgroundImagePath = Path.Combine(Environment.CurrentDirectory, "cairo_traffic_map.png");
                            }
                            if (backgroundImagePath == Path.Combine(Environment.CurrentDirectory, "map_cairo_with_bus_stations.png") || objectImagePath == Path.Combine(Environment.CurrentDirectory, "map_cairo_with_bus_stations.png"))
                            {
                                Task task2 = FindNearestBusStationsAsync(location);
                                GetTrafficMapForBusStationsAsync(task2, zoom, location);
                                objectImagePath = Path.Combine(Environment.CurrentDirectory, "map_cairo_with_bus_stations.png");
                                backgroundImagePath = Path.Combine(Environment.CurrentDirectory, "map_cairo_with_bus_stations.png");
                            }
                            if (backgroundImagePath == Path.Combine(Environment.CurrentDirectory, "map_cairo_with_train_stations.png") || objectImagePath == Path.Combine(Environment.CurrentDirectory, "map_cairo_with_train_stations.png"))
                            {
                                var trainStations = FindNearestTrainStationsAsync(location).Result;  // Blocks until the task completes
                                GetTrafficMapForTrainStationsAsync(trainStations, zoom, location).Wait();  // Blocks until the task completes
                                objectImagePath = Path.Combine(Environment.CurrentDirectory, "map_cairo_with_bus_stations.png");
                                backgroundImagePath = Path.Combine(Environment.CurrentDirectory, "map_cairo_with_bus_stations.png");
                            }
                            Console.WriteLine($"Object image not found: {objectImagePath}");
                            // Fall back to drawing a rectangle
                            g.FillRectangle(objBrush, new Rectangle(ox - size / 2, oy - size / 2, size, size));
                        }
                    }
					catch
					{
						MessageBox.Show("Failed");
					}

                        g.TranslateTransform(ox, oy);
						g.RotateTransform((float)(tobj.Angle / Math.PI * 180.0f));
						g.TranslateTransform(-ox, -oy);

						g.FillRectangle(objBrush, new Rectangle(ox - size / 2, oy - size / 2, size, size));

						g.TranslateTransform(ox, oy);
						g.RotateTransform(-1 * (float)(tobj.Angle / Math.PI * 180.0f));
						g.TranslateTransform(-ox, -oy);

						g.DrawString(tobj.SymbolID + "", font, fntBrush, new PointF(ox - 10, oy - 10));
					}
				}
			}

			// draw the blobs
			if (blobList.Count > 0) {
				lock(blobList) {
					foreach (TuioBlob tblb in blobList.Values) {
						int bx = tblb.getScreenX(width);
						int by = tblb.getScreenY(height);
						float bw = tblb.Width*width;
						float bh = tblb.Height*height;

						g.TranslateTransform(bx, by);
						g.RotateTransform((float)(tblb.Angle / Math.PI * 180.0f));
						g.TranslateTransform(-bx, -by);

						g.FillEllipse(blbBrush, bx - bw / 2, by - bh / 2, bw, bh);

						g.TranslateTransform(bx, by);
						g.RotateTransform(-1 * (float)(tblb.Angle / Math.PI * 180.0f));
						g.TranslateTransform(-bx, -by);
						
						g.DrawString(tblb.BlobID + "", font, fntBrush, new PointF(bx, by));
					}
				}
			}
		}

    private void GetTrafficMapForBusStationsAsync(Task task2, int zoom, string location)
    {
        throw new NotImplementedException();
    }

    public static void Main(String[] argv) {
			int port = 0;
			string location = "31.2399,30.0382";  // Long, Lat for Cairo
			int zoom = 13;  // Zoom level for the map
			Task task = GetMap(location, zoom);
			switch (argv.Length) {
				case 1:
					port = int.Parse(argv[0],null);
					if(port==0) goto default;
					break;
				case 0:
					port = 3333;
					break;
				default:
					Console.WriteLine("usage: mono TuioDemo [port]");
					System.Environment.Exit(0);
					break;
			}
			
			TuioDemo app = new TuioDemo(port);
			Application.Run(app);
		}
	}
