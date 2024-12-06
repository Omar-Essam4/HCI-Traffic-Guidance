namespace new_hci
{
   
    public partial class Form1 : Form
    {

        int f = 0;
        string bg;
        Image img;
        Image []map;
        public Form1()
        {
            //this.WindowState = FormWindowState.Maximized;
            this.ClientSize = new Size(1200, 1200);
           
            this.Paint += new PaintEventHandler(Form1_Paint);
            bg = "home";
            map=new Image[]
            {
                    Image.FromFile("cairo_map.png"),
                    Image.FromFile("cairo_map_12.png"),
                    Image.FromFile("cairo_map_13.png"),
                    Image.FromFile("cairo_map_14.png")
            };
        }

        void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            if (bg == "home")
            {
                img = Image.FromFile("home.jpg");
            }
            if (bg == "map")
            {
                img = Image.FromFile("cairo_map.png");
                f = 1;
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
        }
    }
}