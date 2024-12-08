using Microsoft.VisualBasic.Logging;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace new_hci
{
   
    public partial class Form1 : Form
    {

        int f = 0;
        string bg;
        Image img;
        Image []map;
        int server_port = 3333;
        string server_host = "127.0.0.1";
        private NetworkStream stream;
        string curr_object = "none";
        string prev_object = "";
        bool loggedin = false;
        string user = "";
        public Form1()
        {
            //this.WindowState = FormWindowState.Maximized;
            this.ClientSize = new Size(1200, 1200);
            this.Load += Form1_Load;
            this.Paint += new PaintEventHandler(Form1_Paint);
            bg = "home";
            map=new Image[]
            {
                    Image.FromFile("cairo_map_12.png"),
                    Image.FromFile("cairo_map_13.png"),
                    Image.FromFile("cairo_map_14.png"),
                    Image.FromFile("cairo_map_15.png")
            };
        }

        private async void Form1_Load(object? sender, EventArgs e)
        {
            loggedin = await Login();
            if (loggedin)
            {
                await Task.Run(() => recieve_object());
                Console.WriteLine("Connected");
            }
            else
                Console.WriteLine("Failed to login");
        }

        private async Task<bool> Login()
        {
            try
            {
                TcpClient serverClient = new TcpClient("127.0.0.1", server_port);
                stream = serverClient.GetStream();

                Console.WriteLine("Logged in Successfully");
                return true;
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Error");
            }
            return false;
        }
        private async void recieve_object()
        {
            try
            {
                byte[] receiveBuffer = new byte[1024];
                while (true)
                {
                    int bytesReceived = stream.Read(receiveBuffer, 0, receiveBuffer.Length);
                    if (bytesReceived > 0)
                    {
                        curr_object = Encoding.UTF8.GetString(receiveBuffer, 0, bytesReceived).Trim();
                        Console.WriteLine("object recieved");
                        updatebg(curr_object);
                    }
                    Task.Delay(700).Wait();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
                bg = "none";
            }
        }
        private void updatebg(string curr_object)
        {
            if (curr_object != prev_object)
            {
                // Update the background based on the object
                this.Invoke((MethodInvoker)delegate
                {
                    prev_object = curr_object;
                    bg = curr_object;
                    this.Invalidate(); // Trigger repaint
                });
            }
        }
        void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            if (bg == "home" || bg == "none")
            {
                img = Image.FromFile("home.jpg");
            }
            if (bg == "map")
            {
                f = 0;
                img = map[f];
            }
            if(bg=="zoomin"&&f<3)
            {
                f++;
                img = map[f];
            }
            else if(bg=="zoomout"&&f>0)
            {
                    f--;
                    img = map[f];
            }
            if(bg=="traffic")
            {
                img = Image.FromFile("cairo_traffic_map.png");
            }
            if(bg=="bus")
            {
                img = Image.FromFile("map_cairo_with_bus_stations.png");
            }
            if (bg == "train")
            {
                img = Image.FromFile("map_cairo_with_train_stations.png");
            }

            g.DrawImage(img, 10, 10, this.ClientSize.Width, this.ClientSize.Height);
            if(loggedin && (bg == "omar" || bg == "abdelellah" || bg == "mostafa" || bg == "abdelrahman"))
                g.DrawString("Logged in Successfully", new Font("Arial", 17.0f), new SolidBrush(Color.White), new PointF(5, 5));
        }
    }
}