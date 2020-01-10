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
using System.Timers;
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
        public bool mapMarking = false;
        bool disableField = false;
        MapPoint mapPoint;
        Viewpoint startingPoint;
        Map Map { get; set; } = new Map(Basemap.CreateStreets());
        GraphicsOverlay overlay = new GraphicsOverlay();
        Graphic paint;
        SimpleMarkerSymbol marker = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Diamond, System.Drawing.Color.Red, 20);
        List<Delict> delictenList = new List<Delict>();
        List<Delict> delictenList1 = new List<Delict>();

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

       
        //When clicked on a delict, focus on the correct location
        private async void LoadMap(double x, double y)
        {
            mapPoint = new MapPoint(x, y, SpatialReferences.Wgs84);
            await mapview.SetViewpointCenterAsync(mapPoint);

        }

        private async void LoadMap()
        {
            //Show device location on map
            mapview.LocationDisplay.IsEnabled = true;
            mapview.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Off;


            //Load map
            mapPoint = new MapPoint(6.100159, 52.512878, SpatialReferences.Wgs84);


            startingPoint = new Viewpoint(mapPoint, 50000);
            Map.InitialViewpoint = startingPoint;
            mapview.Map = Map;

            //prevent doubleclick
            mapview.PreviewMouseDoubleClick += (s, e) => e.Handled = true;

            //Delicts loading on map
            cn.OpenConection();
            SqlDataReader sq = cn.DataReader("SELECT long, lat, delict.delict_id as id, date as date, status as status from delict where status = 1 or status = 3");

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
                int delictId = Convert.ToInt32(sq["id"].ToString());
                int status = Convert.ToInt32(sq["status"].ToString());

                DateTime delictTime = Convert.ToDateTime(sq["date"].ToString());

                //Place categories with delicts
                cn1.OpenConection();
                SqlDataReader sq1 = cn1.DataReader("SELECT category.name as name from category_delict INNER JOIN category on category_delict.category_id = category.category_id WHERE category_delict.delict_id = " + delictId);
                while (sq1.Read())
                {
                    if (i < 3)
                    {
                        categories.Append(sq1["name"].ToString() + ", ");
                        i++;
                    }
                    else
                    {
                        i++;
                    }
                }
                cn1.CloseConnection();

                //Place persons with delicts
                cn1.OpenConection();
                SqlDataReader sq2 = cn1.DataReader("SELECT person.firstname as firstname, person.lastname as lastname, delict_person.type as type from delict_person INNER JOIN person on person.person_id = delict_person.person_id WHERE delict_person.delict_id = " + delictId);
                while (sq2.Read())
                {
                    persons.Append(' ' + sq2["firstname"].ToString() + " " + sq2["lastname"].ToString() + " (" + sq2["type"].ToString()[0] + ")  |");
                    Console.WriteLine(sq2["firstname"]);
                }
                cn1.CloseConnection();


   
                //Check if there are more than 3 categories
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

                //check if there are more persons in the list present
                if (persons.Length != 0)
                {
                    persons.Length = persons.Length - 2;
                }

                //put elements in list
                d.id = delictId;
                d.status = status;
                d.category = categories.ToString();
                d.person = persons.ToString();
                d.datetime1 = delictTime.ToString("dd-MM-yyyy");
                d.longitude = longitude;
                d.lat = lat;
                if (d.status == 1 || d.status == 3)
                {
                    delictenList.Add(d);
                }

                mapview.GraphicsOverlays.Remove(overlay);

                //add delict to map
                overlay.Graphics.Add(paint);
                mapview.GraphicsOverlays.Add(overlay);
            }
            cn.CloseConnection();

            foreach (var be in delictenList)
            {
                delictList.Items.Add(delictenList[i]);
                i++;

            }
            i = 0;
        }


        //When someone clicks on the map, this function will be executed
        private async void Click(object sender, Esri.ArcGISRuntime.UI.Controls.GeoViewInputEventArgs e)
        {
            //Check the clicked location
            MapPoint point = e.Location;
            MapPoint pointLatLong = GeometryEngine.Project(point, SpatialReferences.Wgs84) as MapPoint;
            paint = new Graphic(point, marker);
            IdentifyGraphicsOverlayResult identifyResults = await mapview.IdentifyGraphicsOverlayAsync(overlay, e.Position, 0, false, 1);
            double lon = pointLatLong.X;
            double lat = pointLatLong.Y;


            //if someone pressed on a delict
            if (identifyResults.Graphics.Count > 0 && mapMarking == false)
            {
                foreach (var i in identifyResults.Graphics)
                {
                    TextBlock x = new TextBlock();
                    x.Text = i.Attributes.First().Key;
                    SetMarker(x);
                    MakeVisible();
                    StringBuilder categories = new StringBuilder();
                    StringBuilder persons = new StringBuilder();
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

                    //Place categories with delicts
                    cn.OpenConection();
                    SqlDataReader sq1 = cn.DataReader("SELECT category.name as name from category_delict INNER JOIN category on category_delict.category_id = category.category_id WHERE category_delict.delict_id = " + x.Text);
                    while (sq1.Read())
                    {
                        categories.Append(sq1["name"].ToString() + ", ");
                        
                    }
                    categories.Length = categories.Length - 2;
                    cn.CloseConnection();


                    //Place persons with delicts
                    cn.OpenConection();
                    SqlDataReader sq2 = cn.DataReader("SELECT person.firstname as firstname, person.lastname as lastname, delict_person.type as type from delict_person INNER JOIN person on person.person_id = delict_person.person_id WHERE delict_person.delict_id = " + x.Text);
                    while (sq2.Read())
                    {
                        persons.Append(' ' + sq2["firstname"].ToString() + " " + sq2["lastname"].ToString() + " (" + sq2["type"].ToString()[0] + ")  |");
                        Console.WriteLine(sq2["firstname"]);
                    }
                    cn.CloseConnection();
                    if (persons.Length != 0)
                    {
                        persons.Length = persons.Length - 2;
                    }

                    delictCategory.Content = categories;
                    delictPerson.Content = persons;

                } 
            }else if (mapMarking == true)
            {
                mapview.GraphicsOverlays.Remove(overlay);
                overlay.Graphics.Add(paint);
                mapview.GraphicsOverlays.Add(overlay);
                MessageBoxResult dialogResult = MessageBox.Show("Wilt u een delict aanmaken?", "Aanmaken map delict", MessageBoxButton.YesNo);
                if (dialogResult == MessageBoxResult.Yes)
                {
                    mw.AddDelict(lon, lat);

                }
                else if (dialogResult == MessageBoxResult.No)
                {
                    mapview.GraphicsOverlays.Remove(overlay);
                    overlay.Graphics.Remove(paint);
                    mapview.GraphicsOverlays.Add(overlay);
                }
            }
        }


        // make box appear on screen
        public void MakeVisible()
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
        }


        //if delict gets clicked from delictlist
        public void ClickDelict_Click(Object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MakeVisible();
            StringBuilder categories = new StringBuilder();
            StringBuilder persons = new StringBuilder();

            if (delictList.SelectedCells.Count > 0)
            {

                TextBlock x = delictList.Columns[0].GetCellContent(delictList.Items[delictList.SelectedIndex]) as TextBlock;
                cn.OpenConection();
                SqlDataReader sq = cn.DataReader("SELECT * FROM delict WHERE delict_id = " + x.Text);
                while (sq.Read())
                {
                    delictName.Content = sq["delict_id"].ToString();
                    delictDescription.Content = sq["description"].ToString();
                    DateTime delictTime = Convert.ToDateTime(sq["date"].ToString());
                    delictDate.Content = delictTime.ToString("dd-MM-yyyy");
                    delictZip.Content = sq["zipcode"].ToString();
                    delictCoordinatesX.Content = "X: " + sq["long"].ToString();
                    delictCoordinatesY.Content = "Y: " + sq["lat"].ToString();
                    double xCoor = double.Parse(sq["long"].ToString());
                    double yCoor = double.Parse(sq["lat"].ToString());
                    LoadMap(xCoor, yCoor);

                }
                cn.CloseConnection();

                //Place categories with delicts
                cn.OpenConection();
                SqlDataReader sq1 = cn.DataReader("SELECT category.name as name from category_delict INNER JOIN category on category_delict.category_id = category.category_id WHERE category_delict.delict_id = " + x.Text);
                while (sq1.Read())
                {
                    categories.Append(sq1["name"].ToString() + ", ");
                }
                categories.Length = categories.Length - 2;

                cn.CloseConnection();


                //Place persons with delicts
                cn.OpenConection();
                SqlDataReader sq2 = cn.DataReader("SELECT person.firstname as firstname, person.lastname as lastname, delict_person.type as type from delict_person INNER JOIN person on person.person_id = delict_person.person_id WHERE delict_person.delict_id = " + x.Text);
                while (sq2.Read())
                {
                    persons.Append(' ' + sq2["firstname"].ToString() + " " + sq2["lastname"].ToString() + " (" + sq2["type"].ToString()[0] + ")  |");
                    Console.WriteLine(sq2["firstname"]);
                }
                cn.CloseConnection();
                if (persons.Length != 0)
                {
                    persons.Length = persons.Length - 2;
                }

                delictCategory.Content = categories;
                delictPerson.Content = persons;

                SetMarker(x);
            }
        }



        //Highlight the marker
        public void SetMarker(TextBlock x)
        {
            foreach (var a in overlay.Graphics)
            {
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
            mw.ShowDelictenList(true);
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


        //make the box disappear from screen
        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            DelictInzienBTN.IsEnabled = false;
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
            SetMarker(x);

        }


        //Check if in marker mode or not
        private void AddMarkerDelict_Click(object sender, RoutedEventArgs e)
        {
           
            if (mapMarking == false)
            {
                AddMarkerDelictBTN.Background = Brushes.Green;
                mapMarking = true;
            }
            else
            {
                AddMarkerDelictBTN.Background = Brushes.Red;
                mapMarking = false;
            }
        }

        
        private void FilterMap_Click(object sender, RoutedEventArgs e)
        {

            //check if zipcode is correctly filled in, make uppercase
            for (int i = 0; i < ZIPfield.Text.Length; i++)
            {
                if (i < 4)
                {
                    char nummers = ZIPfield.Text[i];
                    bool result = Char.IsDigit(nummers);

                    if (result == false)
                    {
                        mw.ShowDialog("Postcode verkeerd ingevoerd");
                        return;
                    }
                }
                else
                {
                    char nummers = ZIPfield.Text[i];
                    Char.ToUpper(nummers);
                }
            }


            //check if startdate isn't later than enddate
            if (startDate.SelectedDate != null && endDate.SelectedDate != null)
            {
                DateTime startDateText = Convert.ToDateTime(startDate.Text);
                DateTime endDateText = Convert.ToDateTime(endDate.Text);
                if (startDateText > endDateText)
                {
                    mw.ShowDialog("De startdatum is later dan de einddatum");
                    return;
                }
            }

            delictList.Items.Clear();
            listview.Items.Clear();
            overlay.Graphics.Clear();
            delictenList1.Clear();
            string space = "                          ";
            if (!string.IsNullOrEmpty(ZIPfield.Text) && categoryBox.SelectedItem == null && startDate.SelectedDate == null)
            {
                listview.Items.Add(ZIPfield.Text + space);
                cn.OpenConection();
                SqlDataReader sq = cn.DataReader("SELECT delict_id as id FROM delict WHERE zipcode = " + "'" + ZIPfield.Text + "'");
                while (sq.Read())
                {
                    int id = Int32.Parse(sq["id"].ToString());
                    foreach (var b in delictenList)
                    {
                        if (b.id == id)
                        {
                            delictenList1.Add(b);
                        }
                    }
                }
                cn.CloseConnection();
            }


            if (!string.IsNullOrEmpty(ZIPfield.Text) && categoryBox.SelectedItem != null && startDate.SelectedDate == null)
            {
                listview.Items.Add(ZIPfield.Text + "\n" + categoryBox.SelectedItem + space);
                Console.WriteLine();
                cn.OpenConection();
                SqlDataReader sq = cn.DataReader("SELECT delict.delict_id as id FROM delict INNER JOIN category_delict ON category_delict.delict_id = delict.delict_id INNER JOIN category on category_delict.category_id = category.category_id WHERE zipcode = " + "'" + ZIPfield.Text + "'" + "AND category.name = " + "'" + categoryBox.SelectedItem + "'");
                while (sq.Read())
                {
                    int id = Int32.Parse(sq["id"].ToString());
                    foreach (var b in delictenList)
                    {
                        if (b.id == id)
                        {
                            delictenList1.Add(b);
                        }
                    }
                }
                cn.CloseConnection();

            }




            if (string.IsNullOrEmpty(ZIPfield.Text) && categoryBox.SelectedItem != null && startDate.SelectedDate == null)
            {
                listview.Items.Add(categoryBox.SelectedItem + space);
                cn.OpenConection();
                SqlDataReader sq = cn.DataReader("SELECT delict.delict_id as id FROM delict INNER JOIN category_delict ON category_delict.delict_id = delict.delict_id INNER JOIN category on category_delict.category_id = category.category_id WHERE category.name = " + "'" + categoryBox.SelectedItem + "'");
                while (sq.Read())
                {
                    int id = Int32.Parse(sq["id"].ToString());
                    foreach (var b in delictenList)
                    {
                        if (b.id == id)
                        {
                            delictenList1.Add(b);
                        }
                    }
                }
                cn.CloseConnection();

            }



            if (!string.IsNullOrEmpty(ZIPfield.Text) && categoryBox.SelectedItem != null && startDate.SelectedDate != null)
            {
                if (endDate.SelectedDate == null || disableField == true)
                {
                    cn.OpenConection();
                    DateTime startDateText = Convert.ToDateTime(startDate.Text);
                    listview.Items.Add(ZIPfield.Text + "\n" + categoryBox.SelectedItem + "\n" + startDate.Text + space);

                    string correctStartDate = startDateText.ToString("yyyy-MM-dd");
                    SqlDataReader sq = cn.DataReader("SELECT delict.delict_id as id FROM delict INNER JOIN category_delict ON category_delict.delict_id = delict.delict_id INNER JOIN category on category_delict.category_id = category.category_id WHERE zipcode = " + "'" + ZIPfield.Text + "'" + "AND category.name = " + "'" + categoryBox.SelectedItem + "'" + "AND date = " + "'" + correctStartDate + "'");
                    while (sq.Read())
                    {
                        int id = Int32.Parse(sq["id"].ToString());
                        foreach (var b in delictenList)
                        {
                            if (b.id == id)
                            {
                                delictenList1.Add(b);
                            }
                        }
                    }
                    cn.CloseConnection();
                }
                else
                {
                    listview.Items.Add(ZIPfield.Text + "\n" + categoryBox.SelectedItem + "\n" + startDate.Text + "\n" + "T/M" + "\n" + endDate.Text + space);
                    DateTime startDateText = Convert.ToDateTime(startDate.Text);
                    string correctStartDate = startDateText.ToString("yyyy-MM-dd");
                    DateTime endDateText = Convert.ToDateTime(endDate.Text);
                    string correctEndDate = endDateText.ToString("yyyy-MM-dd");

                    cn.OpenConection();
                    SqlDataReader sq = cn.DataReader("SELECT delict.delict_id as id FROM delict INNER JOIN category_delict ON category_delict.delict_id = delict.delict_id INNER JOIN category on category_delict.category_id = category.category_id WHERE zipcode = " + "'" + ZIPfield.Text + "'" + "AND category.name = " + "'" + categoryBox.SelectedItem + "'" + "AND date BETWEEN " + "'" + correctStartDate + "'" + " AND " + "'" + correctEndDate + "'");
                    while (sq.Read())
                    {
                        int id = Int32.Parse(sq["id"].ToString());
                        foreach (var b in delictenList)
                        {
                            if (b.id == id)
                            {
                                delictenList1.Add(b);
                            }
                        }
                    }
                    cn.CloseConnection();

                }
            }




            if (string.IsNullOrEmpty(ZIPfield.Text) && categoryBox.SelectedItem != null && startDate.SelectedDate != null)
            {
                if (endDate.SelectedDate == null || disableField == true)
                {
                    cn.OpenConection();
                    DateTime startDateText = Convert.ToDateTime(startDate.Text);
                    listview.Items.Add(categoryBox.SelectedItem + "\n" + startDate.Text + space);

                    string correctStartDate = startDateText.ToString("yyyy-MM-dd");
                    SqlDataReader sq = cn.DataReader("SELECT delict.delict_id as id FROM delict INNER JOIN category_delict ON category_delict.delict_id = delict.delict_id INNER JOIN category on category_delict.category_id = category.category_id WHERE category.name = " + "'" + categoryBox.SelectedItem + "'" + "AND date = " + "'" + correctStartDate + "'");
                    while (sq.Read())
                    {
                        int id = Int32.Parse(sq["id"].ToString());
                        foreach (var b in delictenList)
                        {
                            if (b.id == id)
                            {
                                delictenList1.Add(b);
                            }
                        }
                    }
                    cn.CloseConnection();
                }
                else
                {
                    listview.Items.Add(categoryBox.SelectedItem + "\n" + startDate.Text + "\n" + "T/M" + "\n" + endDate.Text + space);

                    DateTime startDateText = Convert.ToDateTime(startDate.Text);
                    string correctStartDate = startDateText.ToString("yyyy-MM-dd");
                    DateTime endDateText = Convert.ToDateTime(endDate.Text);
                    string correctEndDate = endDateText.ToString("yyyy-MM-dd");

                    cn.OpenConection();
                    SqlDataReader sq = cn.DataReader("SELECT delict.delict_id as id FROM delict INNER JOIN category_delict ON category_delict.delict_id = delict.delict_id INNER JOIN category on category_delict.category_id = category.category_id WHERE category.name = " + "'" + categoryBox.SelectedItem + "'" + "AND date BETWEEN " + "'" + correctStartDate + "'" + " AND " + "'" + correctEndDate + "'");
                    while (sq.Read())
                    {
                        int id = Int32.Parse(sq["id"].ToString());
                        foreach (var b in delictenList)
                        {
                            if (b.id == id)
                            {
                                delictenList1.Add(b);
                            }
                        }
                    }
                    cn.CloseConnection();

                }
            }


            if (!string.IsNullOrEmpty(ZIPfield.Text) && categoryBox.SelectedItem == null && startDate.SelectedDate != null)
            {
                if (endDate.SelectedDate == null || disableField == true)
                {
                    cn.OpenConection();
                    DateTime startDateText = Convert.ToDateTime(startDate.Text);
                    listview.Items.Add(ZIPfield.Text + "\n" + startDate.Text + space);

                    string correctStartDate = startDateText.ToString("yyyy-MM-dd");
                    SqlDataReader sq = cn.DataReader("SELECT delict.delict_id as id FROM delict INNER JOIN category_delict ON category_delict.delict_id = delict.delict_id INNER JOIN category on category_delict.category_id = category.category_id WHERE zipcode = " + "'" + ZIPfield.Text + "'" + " AND date = " + "'" + correctStartDate + "'");
                    while (sq.Read())
                    {
                        int id = Int32.Parse(sq["id"].ToString());
                        foreach (var b in delictenList)
                        {
                            if (b.id == id)
                            {
                                delictenList1.Add(b);
                            }
                        }
                    }
                    cn.CloseConnection();
                }
                else
                {
                    listview.Items.Add(ZIPfield.Text + "\n" + categoryBox.SelectedItem + "\n" + startDate.Text + "\n" + "T/M" + "\n" + endDate.Text + space);

                    DateTime startDateText = Convert.ToDateTime(startDate.Text);
                    string correctStartDate = startDateText.ToString("yyyy-MM-dd");
                    DateTime endDateText = Convert.ToDateTime(endDate.Text);
                    string correctEndDate = endDateText.ToString("yyyy-MM-dd");

                    cn.OpenConection();
                    SqlDataReader sq = cn.DataReader("SELECT delict.delict_id as id FROM delict INNER JOIN category_delict ON category_delict.delict_id = delict.delict_id INNER JOIN category on category_delict.category_id = category.category_id WHERE zipcode = " + "'" + ZIPfield.Text + "'" + " AND date BETWEEN " + "'" + correctStartDate + "'" + " AND " + "'" + correctEndDate + "'");
                    while (sq.Read())
                    {
                        int id = Int32.Parse(sq["id"].ToString());
                        foreach (var b in delictenList)
                        {
                            if (b.id == id)
                            {
                                delictenList1.Add(b);
                            }
                        }
                    }
                    cn.CloseConnection();

                }
            }
            if (string.IsNullOrEmpty(ZIPfield.Text) && categoryBox.SelectedItem == null && startDate.SelectedDate != null)
            {
                if (endDate.SelectedDate == null || disableField == true)
                {
                    cn.OpenConection();
                    DateTime startDateText = Convert.ToDateTime(startDate.Text);
                    listview.Items.Add(startDate.Text + space);

                    string correctStartDate = startDateText.ToString("yyyy-MM-dd");
                    SqlDataReader sq = cn.DataReader("SELECT delict.delict_id as id FROM delict WHERE date = " + "'" + correctStartDate + "'");
                    while (sq.Read())
                    {
                        int id = Int32.Parse(sq["id"].ToString());
                        foreach (var b in delictenList)
                        {
                            if (b.id == id)
                            {
                                delictenList1.Add(b);
                            }
                        }
                    }
                    cn.CloseConnection();
                }
                else
                {
                    listview.Items.Add(startDate.Text + "\n" + "T/M" + "\n" + endDate.Text + space);

                    DateTime startDateText = Convert.ToDateTime(startDate.Text);
                    string correctStartDate = startDateText.ToString("yyyy-MM-dd");
                    DateTime endDateText = Convert.ToDateTime(endDate.Text);
                    string correctEndDate = endDateText.ToString("yyyy-MM-dd");

                    cn.OpenConection();
                    SqlDataReader sq = cn.DataReader("SELECT delict.delict_id as id FROM delict WHERE date BETWEEN " + "'" + correctStartDate + "'" + " AND " + "'" + correctEndDate + "'");
                    while (sq.Read())
                    {
                        int id = Int32.Parse(sq["id"].ToString());
                        foreach (var b in delictenList)
                        {
                            if (b.id == id)
                            {
                                delictenList1.Add(b);
                            }
                        }
                    }
                    cn.CloseConnection();

                }
            }




            //make the filtered items appear on map and list
            foreach (var he in delictenList1)
            {
                Console.WriteLine("POSTCODE: " + he.id);
                delictList.Items.Add(delictenList1[i]);
                MapPoint point = new MapPoint(he.longitude, he.lat, SpatialReferences.Wgs84);
                paint = new Graphic(point, marker);
                paint.Attributes.Add(he.id.ToString(), he.id.ToString());
                mapview.GraphicsOverlays.Remove(overlay);
                overlay.Graphics.Add(paint);
                mapview.GraphicsOverlays.Add(overlay);
                i++;
            }
            i = 0;

        }

        private void ClearFilter_Click(object sender, RoutedEventArgs e)
        {
            listview.Items.Clear();
            delictenList.Clear();
            delictList.Items.Clear();
            LoadMap();
            categoryBox.SelectedItem = null;
        }


        private void CheckClick_Click(object sender, RoutedEventArgs e)
        {
            if (disableField == false)
            {
                endDate.IsEnabled = false;
                disableField = true;
            }
            else
            {
                endDate.IsEnabled = true;
                disableField = false;
            }
        }

        private void OpenDelict_Click(object sender, RoutedEventArgs e)
        {
            mw.ShowDelict(Convert.ToInt32(delictName.Content), 3);
        }
    }

}
