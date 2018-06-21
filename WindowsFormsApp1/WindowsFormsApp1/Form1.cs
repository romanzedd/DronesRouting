using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.BackColor = Color.FromArgb(255, 255, 255);
            this.Icon = new Icon(@"C:\Users\rzayk\source\repos\WindowsFormsApp1\WindowsFormsApp1\navigation.ico");
            searchButton.BackColor = Color.FromArgb(247, 247, 247);
            button1.BackColor = Color.FromArgb(247, 247, 247);
            undoButton.BackColor = Color.FromArgb(247, 247, 247);
            AstarButton.BackColor = Color.FromArgb(247, 247, 247);
            RunButton.BackColor = Color.FromArgb(247, 247, 247);
        }

        public Map FormMap { get; set; }
        public Graph[] nodes;
        public Graph[,,] map;
        public SquareGrid grid;
        Drones[] drones;
        public List<Location> ThePath;


        public void Form1_Load(object sender, EventArgs e)
        {
            drones = new Drones[3];
            drones[0] = new Drones("Vulcan Black Widow", 500, 100, 5);
            drones[1] = new Drones("Vulcan Payload Development", 5000, 150, 5);
            drones[2] = new Drones("Vulcan Airlift", 1000, 150, 10);
            string[] drone = new string[3];
            for (int i = 0; i < 3; i++)
            {
                drone[i] = drones[i].name;
            }

            LoadData loader = new LoadData();
            string filepath = @"e:\data source\addressbook.map"; //addressbook file path
            string addressBook = null, routes = null;
            int numLines = loader.fileDecrypt(filepath, ref addressBook);

            nodes = new Graph[(numLines-1)*4];

            object[] addresses = new string[numLines-1];
            loader.loadNodes(addressBook, numLines, ref nodes);
            filepath = @"e:\data source\routes.map"; //routes file path
            loader.fileDecrypt(filepath, ref routes);
            loader.loadRoutes(routes, ref nodes);//загрузка соседей в каждый нод

            for (int i = 0; i < numLines-1; i++)
            {
                addresses[i] = nodes[i].street + " " + nodes[i].house;
            }
            comboBox2.Items.AddRange(addresses);
            comboBox1.Items.AddRange(drone);
            mapBrowser.Navigate("https://www.google.ru/maps/@55.0457912,82.9236788,16.91z");

            routing route = new routing();
            map = new Graph[41, 41, 4];
            grid = new SquareGrid(41, 41, 4);
            route.MapMaker(ref map, nodes, 41, 41, 4, 55.041513, 82.916838);
        }

        private int pointsCount = 0;
        private string[] points = new string[3];
        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox2.Text == "")
                return;
            if (pointsCount < 3)
            {
                if (comboBox2.Text == points[0] || comboBox2.Text == points[1])
                    return;
                points[pointsCount] = comboBox2.Text;
                labelling();
                pointsCount++;
            }
            else
            {
                MessageBox.Show("Maximum number of points exceeded");
            }

        }
        private void undoButton_Click(object sender, EventArgs e)
        {
            if (pointsCount == 0)
            {
                MessageBox.Show("No points to delete");
                return;
            }
            pointsCount--;
            points[pointsCount] = null;
            switch (pointsCount)
            {
                case 0:
                    linkLabel1.Text = null;
                    break;
                case 1:
                    linkLabel2.Text = null;
                    break;
                case 2:
                    linkLabel3.Text = null;
                    break;
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            mapBrowser.Navigate("https://www.google.ru/maps/place/Ulitsa+" + suburl(0) + "Novosibirsk");
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            mapBrowser.Navigate("https://www.google.ru/maps/place/Ulitsa+" + suburl(1) + "Novosibirsk");
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            mapBrowser.Navigate("https://www.google.ru/maps/place/Ulitsa+" + suburl(2) + "Novosibirsk");
        }
        

        private void searchButton_Click(object sender, EventArgs e) //Add location from GMaps
        {
            string url = "";
            if (mapBrowser.Url != null)
            {
                url = mapBrowser.Url.AbsoluteUri;
            }
            string[] parseurl = url.Split(new string[] { "place/", "/@" }, StringSplitOptions.None);
            string[] parsestreet = parseurl[1].Split(new string[] { "+" }, StringSplitOptions.None);
            string address = "";
            for (int i = 0; i < parsestreet.Length; i++)
            {
                if (parsestreet[i] == "Ulitsa")
                    i++;
                if (parsestreet[i] == "Novosibirsk,")
                    break;
                address += parsestreet[i].Replace(",", "") + " ";
            }
            address = address.Remove(address.Length - 1);
            if (pointsCount < 3)
            {
                points[pointsCount] = address;
                labelling();
                pointsCount++;
            }
            else
            {
                MessageBox.Show("Maximum number of points exceeded");
            }


        }
        private string suburl()
        {
            string[] address = points[pointsCount].Split(null);
            string suburl = "";
            for (int i = 0; i < address.Length; i++)
                suburl += address[i] + ",+";
            return suburl;
        }
        private string suburl(int lblNum)
        {
            string[] address = points[lblNum].Split(null);
            string suburl = "";
            for (int i = 0; i < address.Length; i++)
                suburl += address[i] + ",+";
            return suburl;
        }
        private void labelling()
        {
            switch (pointsCount)
            {
                case 0:
                    linkLabel1.Text = points[pointsCount] + ", show on the map";
                    linkLabel1.Visible = true;

                    mapBrowser.Navigate("https://www.google.ru/maps/place/Ulitsa+" + suburl() + "Novosibirsk");
                    break;
                case 1:
                    if (points[pointsCount] + ", show on the map" == linkLabel1.Text)//so that user can't adde 2 simillar checkpoints
                        return;
                    linkLabel2.Text = points[pointsCount] + ", show on the map";
                    linkLabel2.Visible = true;

                    mapBrowser.Navigate("https://www.google.ru/maps/place/Ulitsa+" + suburl() + "Novosibirsk");
                    break;
                case 2:
                    if (points[pointsCount] + ", show on the map" == linkLabel1.Text || points[pointsCount] + ", show on the map" == linkLabel2.Text)
                        return;
                    linkLabel3.Text = points[pointsCount] + ", show on the map";
                    linkLabel3.Visible = true;

                    mapBrowser.Navigate("https://www.google.ru/maps/place/Ulitsa+" + suburl() + "Novosibirsk");
                    break;
            }
        }

        private void AstarButton_Click(object sender, EventArgs e)
        {
            //routing path = new routing();
            Graph search = new Graph();
            Location[] route = new Location[pointsCount];
            Graph[] links = new Graph[pointsCount];

            if (comboBox1.Text == "")
            {
                MessageBox.Show("Please select drone");
                return;
            }

            if (comboBox1.Text == drones[2].name)
            {
                MessageBox.Show("Sorry.\nCalculated route is out of range of selected Drone\nPlease selsect different Drone and try again");
                return;
            }

            switch (pointsCount)
            {
                case 0:
                    MessageBox.Show("Not enough checkpoints. Please add more points to the route");
                    return;
                case 1:
                    route[0] = search.findNode(linkLabel1.Text, map, 41, 41, 4);
                    break;
                case 2:
                    route[0] = search.findNode(linkLabel1.Text, map, 41, 41, 4);
                    route[1] = search.findNode(linkLabel2.Text, map, 41, 41, 4);
                    break;
                case 3:
                    route[0] = search.findNode(linkLabel1.Text, map, 41, 41, 4);
                    route[1] = search.findNode(linkLabel2.Text, map, 41, 41, 4);
                    route[2] = search.findNode(linkLabel3.Text, map, 41, 41, 4);
                    break;
            }

            int selected;
            string unserviced = "Sorry, chosen addresses:\n";
            bool unsrvcd = false;
            for (int i = 0; i < pointsCount; i++)
            {
                if (route[i].x < 0)
                {
                    unserviced += points[i] + "\n";
                    unsrvcd = true;
                }
            }
            if (unsrvcd)
            {
                unserviced += "are yet to be added to our coverage";
                MessageBox.Show(unserviced);
                return;
            }
            for (selected = 0; selected < 3; selected++)
            {
                if (comboBox1.Text == drones[selected].name)
                    break;
            }

            NewGrid(41, 41, 4, route);
            AStarSearch astar = new AStarSearch(grid, route[0], route[1]);
            ThePath = astar.ReconstructPath(route[0], route[1], astar.cameFrom);

            KML makefile = new KML();
            makefile.NewKML(ThePath, map);

        }
        public void NewGrid(int dimX, int dimY, int dimZ, Location[] pt)
        {
            //var gridd = new SquareGrid(41,41,4);
            for (int x = 0; x < dimX; x++)
            {
                for (int y = 0; y < dimY; y++)
                {
                    for (int z = 0; z < dimZ; z++)
                    {
                        if (map[x, y, z].passable == false)
                        {
                            for (int i = 0; i < pointsCount; i++)
                            {
                                if (x == pt[i].x && y == pt[i].y && z == pt[i].z)
                                {
                                    z++;
                                }
                            }
                            grid.walls.Add(new Location(x, y, z));
                        }
                    }
                }
            }
        }

        private void RunButton_Click(object sender, EventArgs e)
        {
            Visualizer visualizer = new Visualizer(map, ThePath);
            visualizer.Run();

        }

        private void BFS_Click(object sender, EventArgs e)
        {
            routing path = new routing();
            Graph search = new Graph();
            Location[] route = new Location[pointsCount];
            Graph[] links = new Graph[pointsCount];

            if (comboBox1.Text == "")
            {
                MessageBox.Show("Please select drone");
                return;
            }

            switch (pointsCount)
            {
                case 0:
                    MessageBox.Show("Not enough checkpoints. Please add more points to the route");
                    return;
                case 1:
                    route[0] = search.findNode(linkLabel1.Text, map, 41, 41, 4);
                    break;
                case 2:
                    route[0] = search.findNode(linkLabel1.Text, map, 41, 41, 4);
                    route[1] = search.findNode(linkLabel2.Text, map, 41, 41, 4);
                    break;
                case 3:
                    route[0] = search.findNode(linkLabel1.Text, map, 41, 41, 4);
                    route[1] = search.findNode(linkLabel2.Text, map, 41, 41, 4);
                    route[2] = search.findNode(linkLabel3.Text, map, 41, 41, 4);
                    break;
            }

            int selected;
            string unserviced = "Sorry, chosen addresses:\n";
            bool unsrvcd = false;
            for (int i = 0; i < pointsCount; i++)
            {
                if (route[i].x < 0)
                {
                    unserviced += points[i] + "\n";
                    unsrvcd = true;
                }
            }
            if (unsrvcd)
            {
                unserviced += "are yet to be added to our coverage";
                MessageBox.Show(unserviced);
                return;
            }
            for (selected = 0; selected < 3; selected++)
            {
                if (comboBox1.Text == drones[selected].name)
                    break;
            }

            NewGrid(41, 41, 4, route);
            //AStarSearch astar = new AStarSearch(grid, route[0], route[1]);
            //ThePath = astar.ReconstructPath(route[0], route[1], astar.cameFrom);

            BFSSearch bfs = new BFSSearch(grid, route[0], route[1]);
            ThePath = bfs.ReconstructPath(route[0], route[1], bfs.cameFrom);

            KML makefile = new KML();
            makefile.NewKML(ThePath, map);

        }
    }
}
