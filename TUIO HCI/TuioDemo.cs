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
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics.Eventing.Reader;
using System.Runtime.CompilerServices;


public class TuioDemo : Form , TuioListener
	{
		static string name = "";
		private TuioClient client;
		private Dictionary<long,TuioObject> objectList;
		private Dictionary<long,TuioCursor> cursorList;
		private Dictionary<long,TuioBlob> blobList;

		public static int width, height;
		private int window_width =  1200;
		private int window_height = 1200;
		private int window_left = 0;
		private int window_top = 0;
		private int screen_width = Screen.PrimaryScreen.Bounds.Width;
		private int screen_height = Screen.PrimaryScreen.Bounds.Height;

		private bool fullscreen;
		private bool verbose;
		private static string objectImagePath;
		private static string backgroundImagePath;
		private static List<string> main_maps = new List<string>();
		private static int map_index = 0;
		private int serverPort = 3333;
		private string serverHost = "127.0.0.1";
    /// <summary>
    /// /////
    /// 
    private Bitmap gazeImage = null;
	 static int k = 0;
    /// </summary>

    Font font = new Font("Arial", 10.0f);
		Font font2 = new Font("Arial", 15.0f);
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
			//this.WindowState = FormWindowState.Maximized;
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
			
			client = new TuioClient(port);
			client.addTuioListener(this);

			client.connect();
		}
		private static string login = "Please login";

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
		public static async void Client_side(string serverHost, int serverPort)
		{
			try
			{
			    TcpClient serverClient = new TcpClient(serverHost, serverPort);
			    NetworkStream serverStream = serverClient.GetStream();
			    byte[] buffer = new byte[1024];
			    int bytesRead;
			    StringBuilder completeMessage = new StringBuilder();
			
			    while ((bytesRead = serverStream.Read(buffer, 0, buffer.Length)) != 0)  // Using Read instead of ReadAsync
			    {

			        completeMessage.Append(Encoding.ASCII.GetString(buffer, 0, bytesRead));
				    
					login = "Logged in Successfully Welcome " + completeMessage.ToString();
					Console.WriteLine("Logged in Successfully");
					MessageBox.Show("Logged in Successfully welcome " + completeMessage.ToString());
					TuioDemo app = Application.OpenForms["TuioDemo"] as TuioDemo;
					 name = completeMessage.ToString();
					 app?.Invalidate();
				}				
			}
			catch(Exception ex) 
			{
				//MessageBox.Show("Error");
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

		protected override void OnPaintBackground(PaintEventArgs pevent)
		{
			// Getting the graphics object
			Graphics g = pevent.Graphics;
			g.FillRectangle(bgrBrush, new Rectangle(0,0,width,height));
		try
		{
			Bitmap img;

            if (name == "MARAWAN")
			{
				 img = new Bitmap("home.jpeg");
			}
			else if (name == "kholy")
			{
				img = new Bitmap("elk.jpg");
			}
			else
			{
                img = new Bitmap("no.jpg");
            }



			g.DrawImage(img, 0, 0, this.ClientSize.Width, this.ClientSize.Height);

            if (gazeImage != null)
            {
                g.DrawImage(gazeImage, 0, 0, this.ClientSize.Width, this.ClientSize.Height); // Example position and size
            }

            g.DrawString(login, font2, fntBrush, new PointF(0, 0));
        }
		catch
		{
			Console.WriteLine("Background image can't be loaded");
		}			
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
		if (login != "Please login")
		{
			// draw the objects
			if (objectList.Count > 0)
			{
				lock (objectList)
				{
					foreach (TuioObject tobj in objectList.Values)
					{
						int ox = tobj.getScreenX(width);
						int oy = tobj.getScreenY(height);
						int size = height / 12;
						if (name == "marawan")
						{
							switch (tobj.SymbolID)
							{

								case 0:
									objectImagePath = Path.Combine(Environment.CurrentDirectory, "map.jpeg");
									backgroundImagePath = Path.Combine(Environment.CurrentDirectory, main_maps[map_index]);
									break;
								case 1:
									objectImagePath = Path.Combine(Environment.CurrentDirectory, "car2.jpeg");
									backgroundImagePath = Path.Combine(Environment.CurrentDirectory, "cairo_traffic_map.png");
									break;
								case 2:
									objectImagePath = Path.Combine(Environment.CurrentDirectory, "bus.jpeg");
									backgroundImagePath = Path.Combine(Environment.CurrentDirectory, "map_cairo_with_bus_stations.jpeg");
									break;
								case 3:
									objectImagePath = Path.Combine(Environment.CurrentDirectory, "train.jpeg");
									backgroundImagePath = Path.Combine(Environment.CurrentDirectory, "map_cairo_with_train_stations.jpeg");
									break;
								default:
									// Use default rectangle for other IDs
									g.FillRectangle(objBrush, new Rectangle(ox - size / 2, oy - size / 2, size, size));
									g.DrawString(tobj.SymbolID + "", font, fntBrush, new PointF(ox - 10, oy - 10));
									continue;
							}
						}
						else
						{
                            switch (tobj.SymbolID)
                            {

                                case 0:
                                    objectImagePath = Path.Combine(Environment.CurrentDirectory, "map.jpeg");
                                    backgroundImagePath = Path.Combine(Environment.CurrentDirectory, main_maps[map_index]);
                                    break;
                                case 1:
                                    objectImagePath = Path.Combine(Environment.CurrentDirectory, "car2.jpeg");
                                    backgroundImagePath = Path.Combine(Environment.CurrentDirectory, "cairo_traffic_map.png");
                                    break;
                                case 2:
                                    MessageBox.Show("you have no access");
                                    break;
                                case 3:
                                    MessageBox.Show("you have no access");
                                    break;
                                default:
                                    // Use default rectangle for other IDs
                                    g.FillRectangle(objBrush, new Rectangle(ox - size / 2, oy - size / 2, size, size));
                                    g.DrawString(tobj.SymbolID + "", font, fntBrush, new PointF(ox - 10, oy - 10));
                                    continue;
                            }
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
										//Console.WriteLine(tobj.Angle / Math.PI * 180.0f);
										double tui_angle = tobj.Angle / Math.PI * 180.0f;
										if (true)
										{
											if (tui_angle > 45 && tui_angle <= 90 && map_index < 1)
												map_index++;
											if (tui_angle > 90 && tui_angle <= 135 && map_index < 2)
												map_index++;
											if (tui_angle > 135 && map_index < 3)
												map_index++;

											if (map_index == 3 && tui_angle >= 90 && tui_angle < 135)
												map_index--;
											if (map_index == 2 && tui_angle >= 45 && tui_angle < 90)
												map_index--;
											if (map_index == 3 && tui_angle < 45)
												map_index = 0;
											if (map_index == 1 && tui_angle >= 0 && tui_angle < 45)
												map_index--;
										}
										// Draw the rotated object
										g.DrawImage(objectImage, new Rectangle(ox - size / 2, oy - size / 2, size, size));

										// Restore the graphics state
										g.Restore(state);
									}
								}
								else
								{
									Console.WriteLine(tobj.Angle);
									Console.WriteLine($"Object image not found: {objectImagePath}");
									// Fall back to drawing a rectangle
									g.FillRectangle(objBrush, new Rectangle(ox - size / 2, oy - size / 2, size, size));
								}
							}
							catch
							{
								MessageBox.Show("Error");
							}

						g.TranslateTransform(ox, oy);
						g.RotateTransform((float)(tobj.Angle / Math.PI * 180.0f));
						g.TranslateTransform(-ox, -oy);

						//g.FillRectangle(objBrush, new Rectangle(ox - size / 2, oy - size / 2, size, size));

						g.TranslateTransform(ox, oy);
						g.RotateTransform(-1 * (float)(tobj.Angle / Math.PI * 180.0f));
						g.TranslateTransform(-ox, -oy);

						g.DrawString(tobj.SymbolID + "", font, fntBrush, new PointF(ox - 10, oy - 10));
					}
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
						Image cairo_bg = Image.FromFile("car2.png");
						Brush bg_brush = new TextureBrush(cairo_bg);

						g.TranslateTransform(bx, by);
						g.TranslateTransform(this.Width / 2, this.Height / 2);
						g.RotateTransform((float)(tblb.Angle / Math.PI * 180.0f));
						g.TranslateTransform(-bx, -by);

						g.FillEllipse(blbBrush, bx - bw / 2, by - bh / 2, bw, bh);

						g.TranslateTransform(bx, by);
						g.TranslateTransform(-this.Width / 2, -this.Height / 2);
						g.FillRectangle(bg_brush, 0, 0, this.Width, this.Height);
						
						g.RotateTransform(-1 * (float)(tblb.Angle / Math.PI * 180.0f));
						g.TranslateTransform(-bx, -by);
                    g.DrawString(tblb.BlobID + "", font, fntBrush, new PointF(bx, by));
					}
				}
			}
		}

    private void InitializeComponent()
    {
            this.SuspendLayout();
            // 
            // TuioDemo
            // 
            this.ClientSize = new System.Drawing.Size(282, 253);
            this.Name = "TuioDemo";
            this.Load += new System.EventHandler(this.TuioDemo_Load);
            this.ResumeLayout(false);
    }

    private void TuioDemo_Load(object sender, EventArgs e)
    {

    }
	private static bool CheckMaps()
	{
		main_maps.Add("cairo_map_12.png");
        main_maps.Add("cairo_map_13.png");
        main_maps.Add("cairo_map_14.png");
        main_maps.Add("cairo_map_15.png");
        string[] api_images = { "cairo_map_12.png", "cairo_map_13.png", "cairo_map_14.png","cairo_map_15.png" ,"cairo_traffic_map.png", "map_cairo_with_bus_stations.jpeg", "map_cairo_with_train_stations.jpeg"};
		for(int i = 0; i < api_images.Length; i++)
		{
			if (!File.Exists(api_images[i]))
				return false;
		}
		return true;
    }
    public static async void StartGazeReceiver()
    {
        try
        {
            TcpListener listener = new TcpListener(System.Net.IPAddress.Parse("127.0.0.1"), 5555);
            listener.Start();
            Console.WriteLine("Gaze server started on port 5555");

            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                _ = Task.Run(() => HandleGazeClient(client));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error in Gaze Receiver: " + ex.Message);
        }
    }

    private static void HandleGazeClient(TcpClient client)
    {
		if (login != "Please login")
		{
			//Graphics g;
			NetworkStream stream = client.GetStream();
			byte[] buffer = new byte[1024];
			int bytesRead;
			Bitmap img;
			while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
			{
				string direction = Encoding.ASCII.GetString(buffer, 0, bytesRead);
				Console.WriteLine("Gaze Direction: " + direction);

				TuioDemo app = Application.OpenForms["TuioDemo"] as TuioDemo;
				if (app != null)
				{

					app.Invoke((MethodInvoker)(() =>
					{
						switch (direction.Trim().ToLower())
						{
							case "left":
								MessageBox.Show("Gaze left Detected � trigger feature A");
								app.gazeImage = new Bitmap(main_maps[k]);//"map.jpeg");
								if (k > 0)
								{
									k--;
								}
								break;
							case "right":
								MessageBox.Show("Gaze right Detected � trigger feature A");
								app.gazeImage = new Bitmap(main_maps[k]);//"map.jpeg");
								if (k < 3)
								{
									k++;
								}
								// app.gazeImage = new Bitmap("right_image.jpg"); // optional
								break;
							case "up":
								MessageBox.Show("Gaze Up Detected � trigger feature A");
								break;
							case "down":
								MessageBox.Show("Gaze Down Detected � trigger feature B");
								break;
							case "traffic light":
								MessageBox.Show("traffic light");
								app.gazeImage = new Bitmap("cairo_traffic_map.png");//"map.jpeg");

								break;
							case "bus":
								MessageBox.Show("bus");
								app.gazeImage = new Bitmap("map_cairo_with_bus_stations.jpeg");//"map.jpeg");
								break;
							case "train":
								MessageBox.Show("train");
								app.gazeImage = new Bitmap("map_cairo_with_train_stations.jpeg");//"map.jpeg");
								break;
							case "car":
								MessageBox.Show("car");
								app.gazeImage = new Bitmap("cairo_map_12.png");//"map.jpeg");
								break;
                            case "traffic":
                                MessageBox.Show("traffic");
                                app.gazeImage = new Bitmap("cairo_traffic_map.png");//"map.jpeg");
                                break;


                        }
						app.Invalidate();
					}));
				}
			}
			client.Close();
		}
    }

    public static async Task Main(String[] argv) {
	 		int port = 3333;
			Client_side("127.0.0.1", port);
			StartGazeReceiver();
			if (!CheckMaps())
			{
				MapBoxAPI map_api = new MapBoxAPI();
				await map_api.RunAsync("31.2399,30.0382", 12);
			}			
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
