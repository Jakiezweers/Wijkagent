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
        Map Map { get; set; } = new Map(Basemap.CreateStreets());
        GraphicsOverlay overlay = new GraphicsOverlay();
        Graphic paint;
        SimpleMarkerSymbol marker = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Diamond, System.Drawing.Color.Red, 20);


        public HomePage(MainWindow MW)
        {
            mw = MW;
            InitializeComponent();
            LoadMap();
            mapview.GeoViewTapped += Click;
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

        private async void LoadMap()
        {

            //Map inladen
            var mapPoint = new MapPoint(6.100159, 52.512878, SpatialReferences.Wgs84);
            Viewpoint startingpoint = new Viewpoint(mapPoint, 50000);
            Map.InitialViewpoint = startingpoint;
            mapview.Map = Map;

            //Delicten op map laden
            cn.OpenConection();
            SqlDataReader sq = cn.DataReader("Select * from dbo.delict");
            while (sq.Read())
            {
                MapPoint point = new MapPoint((double)sq["long"], (double)sq["lat"], SpatialReferences.Wgs84);
                paint = new Graphic(point, marker);
                mapview.GraphicsOverlays.Remove(overlay);
                overlay.Graphics.Add(paint);
                mapview.GraphicsOverlays.Add(overlay);                
            }
            cn.CloseConnection();

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
    }

}
