using Google.Maps;
using Google.Maps.Geocoding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Wijkagent
{
    /// <summary>
    /// Interaction logic for AddDelictWindow.xaml
    /// </summary>
    public partial class AddDelictWindow : Window
    {
        List<DDL_Country> objCountryList;

        public AddDelictWindow()
        {
            InitializeComponent();
            objCountryList = new List<DDL_Country>();
            AddElementsInList();
            BindCountryDropDown();
            GoogleSigned.AssignAllServices(new GoogleSigned("-"));
        }


        private void BindCountryDropDown()
        {
            ddlCountry.ItemsSource = objCountryList;
        }
        private void ddlCountry_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ddlCountry_TextChanged(object sender, TextChangedEventArgs e)
        {
            ddlCountry.ItemsSource = objCountryList.Where(x => x.Country_Name.StartsWith(ddlCountry.Text.Trim()));
        }

        private void AllCheckbocx_CheckedAndUnchecked(object sender, RoutedEventArgs e)
        {
            BindListBOX();
        }

        private void BindListBOX()
        {
            testListbox.Items.Clear();
            foreach (var country in objCountryList)
            {
                if (country.Check_Status == true)
                {
                    testListbox.Items.Add(country.Country_Name);
                    ddlCountry.Text = "";
                }
            }
        }

        private void AddElementsInList()
        {
            // 1 element  
            DDL_Country obj = new DDL_Country();
            obj.Country_ID = 10;
            obj.Country_Name = "Moord";
            objCountryList.Add(obj);
            obj = new DDL_Country();
            obj.Country_ID = 11;
            obj.Country_Name = "Diefstal";
            objCountryList.Add(obj);
            obj = new DDL_Country();
            obj.Country_ID = 12;
            obj.Country_Name = "Roofmoord";
            objCountryList.Add(obj);
            obj = new DDL_Country();
            obj.Country_ID = 13;
            obj.Country_Name = "Ongeval";
            objCountryList.Add(obj);
            obj = new DDL_Country();
            obj.Country_ID = 14;
            obj.Country_Name = "Yeet";
            objCountryList.Add(obj);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string category = "";
            string placeName = PlaatsTB.Text;
            string zipCode = PostcodeTB.Text;
            string homeNumber = HuisnummerTB.Text;
            string street = StraatTB.Text;
            string description = OmschijvingTB.Text;
            string date = DatumTB.Text;

            foreach (var item in testListbox.Items)
            {
                category += item + " ,";
            }
            
            category = category.Substring(0, category.Length - 2);

            var request = new GeocodingRequest();
            request.Address = "Morigerweg 14, blijham";
            var response = new GeocodingService().GetResponse(request);

            if (response.Status == ServiceResponseStatus.Ok && response.Results.Count() > 0)
            {
                var result = response.Results.First();
                LabelTest.Text = result.Geometry.Location.Latitude.ToString();
            }
            else
            {
                string error = $"Unable to geocode.  Status={0} and ErrorMessage={1}" + response.Status + response.ErrorMessage;
                LabelTest.Text = error;
            }
        }

        private void GetLongLat()
        {
            var request = new GeocodingRequest();
            request.Address = "1600 Pennsylvania Ave NW, Washington, DC 20500";
            var response = new GeocodingService().GetResponse(request);

            var result = response.Results.First();

            LabelTest.Text = result.Geometry.Location.Latitude.ToString();
        }
    }

    public class DDL_Country
    {
        public int Country_ID
        {
            get;
            set;
        }
        public string Country_Name
        {
            get;
            set;
        }
        public Boolean Check_Status
        {
            get;
            set;
        }
    }
}
