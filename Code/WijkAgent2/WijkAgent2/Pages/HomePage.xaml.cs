using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wijkagent2.Classes;
using WijkAgent2.Classes;
using WijkAgent2.Database;

namespace WijkAgent2.Pages
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        private MainWindow mw;
        private Connection cn = new Connection();
        private Connection cn1 = new Connection();
        private Connection cn2 = new Connection();
        bool disablefield = false;
        MapPoint mapPoint;
        Viewpoint startingpoint;
        Map Map { get; set; } = new Map(Basemap.CreateStreets());
        GraphicsOverlay overlay = new GraphicsOverlay();
        Graphic paint;
        SimpleMarkerSymbol marker = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Diamond, System.Drawing.Color.Red, 20);
        List<CategoryList> categoryList = new List<CategoryList>();
        List<Delict> delictenlist = new List<Delict>();
        List<Delict> delictenlistWithDate = new List<Delict>();
        List<Delict> delictenlistWithDateAndCategory = new List<Delict>();

        int i = 0;

        public HomePage(MainWindow MW)
        {
            mw = MW;
            InitializeComponent();
            LoadMap();
            mapview.GeoViewTapped += Click;

            cn.OpenConection();
            SqlDataReader sq = cn.DataReader("Select * from dbo.category");
            while (sq.Read())
            {
                categoryBox.Items.Add(sq["name"].ToString());
            }
            cn.CloseConnection();
        }

        //wanneer er geklikt wordt op een delict, focus op de juiste locatie
        private async void LoadMap(double x, double y)
        {
            mapPoint = new MapPoint(x, y, SpatialReferences.Wgs84);
            await mapview.SetViewpointCenterAsync(mapPoint);

        }

        private async void LoadMap()
        {

            //Map inladen
            mapPoint = new MapPoint(6.100159, 52.512878, SpatialReferences.Wgs84);
            startingpoint = new Viewpoint(mapPoint, 50000);
            Map.InitialViewpoint = startingpoint;
            mapview.Map = Map;

            //Delicten op map laden
            cn.OpenConection();
            SqlDataReader sq = cn.DataReader("SELECT long, lat, delict.delict_id as id, date as date from delict");
           
            while (sq.Read())
            {
                MapPoint point = new MapPoint((double)sq["long"], (double)sq["lat"], SpatialReferences.Wgs84);

                double longitude = double.Parse(sq["long"].ToString());
                double lat = double.Parse(sq["lat"].ToString());

                paint = new Graphic(point, marker);
                paint.Attributes.Add(sq["id"].ToString(), sq["id"].ToString());
                Delict d = new Delict();
                StringBuilder categories = new StringBuilder();
                StringBuilder persons = new StringBuilder();
                int i = 0;
                int delictid = Convert.ToInt32(sq["id"].ToString());
                DateTime delicttime = Convert.ToDateTime(sq["date"].ToString());

                //Categoriën bij delicten plaatsen
                    cn1.OpenConection();
                    SqlDataReader sq1 = cn1.DataReader("SELECT category.name as name from category_delict INNER JOIN category on category_delict.category_id = category.category_id WHERE category_delict.delict_id = " + delictid);
                    while (sq1.Read())
                    {
                        if (i < 3)
                        {
                        categories.Append(sq1["name"].ToString() + ", ");
                            i++;
                        } else
                        {
                            i++;
                        }
                    }
                cn1.CloseConnection();


                //Personen bij delicten plaatsen
                cn2.OpenConection();
                SqlDataReader sq2 = cn2.DataReader("SELECT person.firstname as firstname, person.lastname as lastname, delict_person.type as type from delict_person INNER JOIN person on person.person_id = delict_person.person_id WHERE delict_person.delict_id = " + delictid);
                while (sq2.Read())
                {
                    persons.Append(' ' + sq2["firstname"].ToString() + " " + sq2["lastname"].ToString() + " (" + sq2["type"].ToString()[0] + ")  |");
                    Console.WriteLine(sq2["firstname"]);
                }
                cn2.CloseConnection();


                //Check of er meer dan 3 categoriën zijn
                if (i > 3)
                    {
                    categories.Length = categories.Length - 2;
                        i = i - 3;
                    categories.Append(" (" + i.ToString() + ")");
                    }
                    else
                    {
                    categories.Length = categories.Length - 2;
                    }

                //Check of er personen in de lijst aanwezig zijn
                if (persons.Length != 0)
                {
                    persons.Length = persons.Length - 2;
                }

                //Elementen in lijst plaatsen
                    d.id = delictid;
                    d.category = categories.ToString();
                    d.person = persons.ToString();
                    d.datetime1 = delicttime.ToString("dd-MM-yyyy");
                d.longitude = longitude;
                d.lat = lat;
                delictenlist.Add(d);

                mapview.GraphicsOverlays.Remove(overlay);
                overlay.Graphics.Add(paint);
                mapview.GraphicsOverlays.Add(overlay);
            }
            cn.CloseConnection();

            foreach(var be in delictenlist)
            {
                delictList.Items.Add(delictenlist[i]);
                i++;

            }
            i = 0;
        }

        private async void Click(object sender, Esri.ArcGISRuntime.UI.Controls.GeoViewInputEventArgs e)
        {
            MapPoint point = new MapPoint(e.Location.X, e.Location.Y);
            paint = new Graphic(point, marker);
            IdentifyGraphicsOverlayResult identifyResults = await mapview.IdentifyGraphicsOverlayAsync(overlay, e.Position, 0, false, 1);
            if (identifyResults.Graphics.Count > 0)
            {
            }
            else
            {
                mapview.GraphicsOverlays.Remove(overlay);
                overlay.Graphics.Add(paint);
                mapview.GraphicsOverlays.Add(overlay);
            }
        }

        public void clickDelict(Object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DelictInzienBTN.IsEnabled = true;
            labelsVis.Visibility = Visibility.Visible;
            labelsVis1.Visibility = Visibility.Visible;
            labelsVis2.Visibility = Visibility.Visible;
            labelsVis3.Visibility = Visibility.Visible;
            labelsVis4.Visibility = Visibility.Visible;
            labelsVis5.Visibility = Visibility.Visible;
            labelsVis6.Visibility = Visibility.Visible;
            labelsVis7.Visibility = Visibility.Visible;
            delictName.Visibility = Visibility.Visible;
            delictCategory.Visibility = Visibility.Visible;
            delictDate.Visibility = Visibility.Visible;
            delictDescription.Visibility = Visibility.Visible;
            delictCoordinatesX.Visibility = Visibility.Visible;
            delictCoordinatesY.Visibility = Visibility.Visible;
            delictPerson.Visibility = Visibility.Visible;
            delictZip.Visibility = Visibility.Visible;

            StringBuilder categories = new StringBuilder();
            StringBuilder persons = new StringBuilder();
            
            if (delictList.SelectedCells.Count > 0)
            {

                TextBlock x = delictList.Columns[0].GetCellContent(delictList.Items[delictList.SelectedIndex]) as TextBlock;
                cn.OpenConection();
                Console.WriteLine("ID: " + x.Text);
                SqlDataReader sq = cn.DataReader("SELECT * FROM delict WHERE delict_id = " + x.Text);
                while (sq.Read())
                {
                    delictName.Content = sq["delict_id"].ToString();
                    delictDescription.Content = sq["description"].ToString();
                    DateTime delicttime = Convert.ToDateTime(sq["date"].ToString());
                    delictDate.Content = delicttime.ToString("dd-MM-yyyy");
                    delictZip.Content = sq["zipcode"].ToString();
                    delictCoordinatesX.Content = "X: " + sq["long"].ToString();
                    delictCoordinatesY.Content = "Y: " + sq["lat"].ToString();
                    double xcoor = double.Parse(sq["long"].ToString());
                    double ycoor = double.Parse(sq["lat"].ToString());
                    LoadMap(xcoor, ycoor);
                   
                }
                cn.CloseConnection();

                //Categoriën bij delicten plaatsen
                cn1.OpenConection();
                SqlDataReader sq1 = cn1.DataReader("SELECT category.name as name from category_delict INNER JOIN category on category_delict.category_id = category.category_id WHERE category_delict.delict_id = " + x.Text);
                while (sq1.Read())
                {
                   

                    categories.Append(sq1["name"].ToString() + ", ");
                   
                }
                cn1.CloseConnection();


                //Personen bij delicten plaatsen
                cn2.OpenConection();
                SqlDataReader sq2 = cn2.DataReader("SELECT person.firstname as firstname, person.lastname as lastname, delict_person.type as type from delict_person INNER JOIN person on person.person_id = delict_person.person_id WHERE delict_person.delict_id = " + x.Text);
                while (sq2.Read())
                {
                    persons.Append(' ' + sq2["firstname"].ToString() + " " + sq2["lastname"].ToString() + " (" + sq2["type"].ToString()[0] + ")  |");
                    Console.WriteLine(sq2["firstname"]);
                }
                cn2.CloseConnection();
                if (persons.Length != 0)
                {
                    persons.Length = persons.Length - 2;
                }

                delictCategory.Content = categories;
                delictPerson.Content = persons;



               
                setMarker(x);
            }
        }



        //Marker highlighten
        public void setMarker(TextBlock x)
        {
            foreach (var a in overlay.Graphics)
            {
                Console.WriteLine("X IN TEKST: " + x.Text);
                Console.WriteLine("ATTRIBUTES: " + a.Attributes);
                try
                {
                    SimpleMarkerSymbol marker1 = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Diamond, System.Drawing.Color.Red, 20);
                    a.Symbol = marker1;
                    Console.WriteLine(a.Attributes.ContainsKey(x.Text));

                    if (a.Attributes.ContainsKey(x.Text))
                    {
                        if (x.Text != "close")
                        {
                            SimpleMarkerSymbol marker = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Diamond, System.Drawing.Color.Blue, 50);
                            a.Symbol = marker;
                        }
                        else
                        {
                            SimpleMarkerSymbol marker = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Diamond, System.Drawing.Color.Red, 20);
                            a.Symbol = marker;
                        }
                                            }
                }
                catch (Exception ere) { }
            }
        }










        private void LogOut_Click(object sender, RoutedEventArgs e)
        {
            mw.Logout();
        }

        private void DelictList_Click(object sender, RoutedEventArgs e)
        {
            mw.ShowDelictenList();
        }

        private void DelictArchive_Click(object sender, RoutedEventArgs e)
        {
            mw.ShowDelictenArchive();
        }

        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            mw.AddUser();
        }

        private void Permission_button_Click(object sender, RoutedEventArgs e)
        {
            mw.LoadPermissionPage();
        }

        private void Close_window(object sender, RoutedEventArgs e)
        {
            labelsVis.Visibility = Visibility.Hidden;
            labelsVis1.Visibility = Visibility.Hidden;
            labelsVis2.Visibility = Visibility.Hidden;
            labelsVis3.Visibility = Visibility.Hidden;
            labelsVis4.Visibility = Visibility.Hidden;
            labelsVis5.Visibility = Visibility.Hidden;
            labelsVis6.Visibility = Visibility.Hidden;
            labelsVis7.Visibility = Visibility.Hidden;
            delictName.Visibility = Visibility.Hidden;
            delictCategory.Visibility = Visibility.Hidden;
            delictDate.Visibility = Visibility.Hidden;
            delictDescription.Visibility = Visibility.Hidden;
            delictCoordinatesX.Visibility = Visibility.Hidden;
            delictCoordinatesY.Visibility = Visibility.Hidden;
            delictPerson.Visibility = Visibility.Hidden;
            delictZip.Visibility = Visibility.Hidden;
            TextBlock x = new TextBlock();
            x.Text = "close";
            setMarker(x);
            DelictInzienBTN.IsEnabled = false;
        }

        private void filterMap(object sender, RoutedEventArgs e)
        {

            overlay.Graphics.Clear();
            delictenlistWithDate.Clear();
            delictenlistWithDateAndCategory.Clear();
            delictList.Items.Clear();
            listview.Items.Clear();
            if (startDate.SelectedDate != null && endDate.SelectedDate != null)
            {
                DateTime startdate = Convert.ToDateTime(startDate.Text);
                DateTime enddate = Convert.ToDateTime(endDate.Text);
                string correctEndDate = enddate.ToString("yyyy-MM-dd");
                string correctStartDate = startdate.ToString("yyyy-MM-dd");



                cn.OpenConection();
                if (enddate == null || disablefield == true)
                {
                    listview.Items.Add(startDate.Text);

                    SqlDataReader sq = cn.DataReader("SELECT delict_id as id FROM delict WHERE date = " + "'" + correctStartDate + "'");

                    while (sq.Read())
                    {
                        int id = Int32.Parse(sq["id"].ToString());

                        foreach (var b in delictenlist)
                        {
                            if (b.id == id)
                            {
                                delictenlistWithDate.Add(b);
                            }
                        }
                    }
                }
                else
                {
                    listview.Items.Add(startDate.Text + "\n" + "T/M " + "\n" + endDate.Text);

                    SqlDataReader sq = cn.DataReader("SELECT delict_id as id FROM delict WHERE date BETWEEN" + "'" + correctStartDate + "'" + "AND" + "'" + correctEndDate + "'");

                    while (sq.Read())
                    {
                        int id = Int32.Parse(sq["id"].ToString());

                        foreach (var b in delictenlist)
                        {
                            if (b.id == id)
                            {
                                delictenlistWithDate.Add(b);
                            }
                        }
                    }
                }

                cn.CloseConnection();

            }
            if (categoryBox.SelectedItem != null)
            {

                listview.Items.Add(categoryBox.SelectedItem);
                 cn.OpenConection();
                SqlDataReader sq1 = cn.DataReader("SELECT category.name as name, category_delict.delict_id as id from category_delict INNER JOIN category on category_delict.category_id = category.category_id WHERE category.name = " + "'" + categoryBox.SelectedItem + "'");


                while (sq1.Read())
                {
                    int id = Int32.Parse(sq1["id"].ToString());

                    if (delictenlistWithDate.Count > 0)
                    {
                        for (int k = 0; k < delictenlistWithDate.Count; k++)
                        {
                        Console.WriteLine("K: " + k);
                            if (delictenlistWithDate[k].id == id)
                            {
                                delictenlistWithDateAndCategory.Add(delictenlistWithDate[k]);
                            }
                        }


           }
                    else
                    {
                        Console.WriteLine("ELSE STATEMENT");
                        foreach (var b in delictenlist)
                        {
                            if (b.id == id)
                            {
                                Console.WriteLine(b.id);
                                delictenlistWithDateAndCategory.Add(b);
                            }
                        }
                    }
                }

                cn.CloseConnection();
                foreach (var he in delictenlistWithDateAndCategory)
                {
                    delictList.Items.Add(delictenlistWithDateAndCategory[i]);
                    MapPoint point = new MapPoint(he.longitude, he.lat, SpatialReferences.Wgs84);
                    paint = new Graphic(point, marker);
                    paint.Attributes.Add(he.id.ToString(), he.id.ToString());
                    mapview.GraphicsOverlays.Remove(overlay);
                    overlay.Graphics.Add(paint);
                    mapview.GraphicsOverlays.Add(overlay);
                    i++;
                }

            }
            else
            {
                foreach (var he in delictenlistWithDate)
                {
                    delictList.Items.Add(delictenlistWithDate[i]);
                    MapPoint point = new MapPoint(he.longitude, he.lat, SpatialReferences.Wgs84);
                    paint = new Graphic(point, marker);
                    paint.Attributes.Add(he.id.ToString(), he.id.ToString());
                    mapview.GraphicsOverlays.Remove(overlay);
                    overlay.Graphics.Add(paint);
                    mapview.GraphicsOverlays.Add(overlay);
                    i++;
                }

            }


      
            i = 0;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            listview.Items.Clear();
            delictenlist.Clear();
            delictenlistWithDate.Clear();
            delictenlistWithDateAndCategory.Clear();
            delictList.Items.Clear();
            LoadMap();
        }


        private void checkClick(object sender, RoutedEventArgs e)
        {
            if(disablefield == false)
            {
                endDate.IsEnabled = false;
                disablefield = true;
            }
            else
            {
                endDate.IsEnabled = true;
                disablefield = false;
            }
        }

        private void OpenDelict(object sender, RoutedEventArgs e)
        {
            mw.ShowDelict(Convert.ToInt32(delictName.Content),3);
        }
    }
}
