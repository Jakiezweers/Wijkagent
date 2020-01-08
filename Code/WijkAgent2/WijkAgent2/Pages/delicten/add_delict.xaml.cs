using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Tasks.Geocoding;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
using WijkAgent2.Classes;
using WijkAgent2.Database;
using WijkAgent2.Modals;

namespace WijkAgent2.Pages.delicten
{
    /// <summary>
    /// Interaction logic for add_delict.xaml
    /// </summary>
    public partial class add_delict : Page
    {
        List<CategoryList> categoryList = new List<CategoryList>();
        public List<int> personsbsn = new List<int>();
        List<string> personstype = new List<string>();
        List<int> person_id = new List<int>();
        LocatorTask _geocoder;
        double longCoord = 0.0000;
        double latCoord = 0.0000;
        int state;
        double longt = 0.0000;
        double latt = 0.0000;
        string errorMessage = "De volgende velden zijn niet correct ingevoerd: ";
        bool errorBool = false;
        Uri _serviceUri = new Uri("https://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer");

        private Connection cn = new Connection();
        int i = 0;
        private MainWindow mw;
        public add_delict(MainWindow MW)
        {
            this.mw = MW;
            InitializeComponent();
            categoryList = new List<CategoryList>();
            BindCategroryDropDown();
            DatumTB.SelectedDate = DateTime.Today;
            AddNewPerson addperson = new AddNewPerson(mw);
            AddPersonButton.Click += (sender, EventArgs) => { AddPerson_Click(sender, EventArgs, addperson); };

            cn.OpenConection();
            SqlDataReader sq = cn.DataReader("Select * from dbo.category");
            while (sq.Read())
            {
                CategoryList obj = new CategoryList((int)sq["category_id"], (string)(sq["name"]));
                categoryList.Add(obj);
            }
            cn.CloseConnection();
        }
        public add_delict(MainWindow MW, double lon, double lat)
        {
            this.mw = MW;
            InitializeComponent();
            categoryList = new List<CategoryList>();
            BindCategroryDropDown();
            DatumTB.SelectedDate = DateTime.Today;
            AddNewPerson addperson = new AddNewPerson(mw);
            AddPersonButton.Click += (sender, EventArgs) => { AddPerson_Click(sender, EventArgs, addperson); };

            cn.OpenConection();
            SqlDataReader sq = cn.DataReader("Select * from dbo.category");
            while (sq.Read())
            {
                CategoryList obj = new CategoryList((int)sq["category_id"], (string)(sq["name"]));
                categoryList.Add(obj);
            }
            cn.CloseConnection();
            longt = lon;
            latt = lat;

            state = 3;
            if (state == 3)
            {
                HuisnummerTB.Visibility = Visibility.Collapsed;
                StraatTB.Visibility = Visibility.Collapsed;
                PostcodeTB.Visibility = Visibility.Collapsed;
                HuisnummerLB.Visibility = Visibility.Collapsed;
                StraatLB.Visibility = Visibility.Collapsed;
                PostcodeLB.Visibility = Visibility.Collapsed;
                PlaatsLB.Visibility = Visibility.Collapsed;
                PlaatsTB.Visibility = Visibility.Collapsed;
            }
        }
        private void BindCategroryDropDown()
        {
            categoryCB.ItemsSource = categoryList;
        }

        private void category_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!categoryCB.IsDropDownOpen)
            {
                categoryCB.IsDropDownOpen = true;
            }
            var t = categoryCB.SelectedIndex;
            categoryCB.ItemsSource = categoryList.Where(x => x.Category_Name.ToLower().StartsWith(categoryCB.Text.Trim().ToLower()));
        }

        private void AllCheckbocx_CheckedAndUnchecked(object sender, RoutedEventArgs e)
        {
            BindListBOX();
        }

        private void BindListBOX()
        {
            testListbox.Items.Clear();
            foreach (var category in categoryList)
            {
                if (category.Check_Status == true)
                {
                    testListbox.Items.Add(category.Category_Name);
                    categoryCB.Text = "";
                }
            }
        }


        private async void Run_This()
        {

            string placeName = "";
            string zipCode = "";
            string homeNumber = "";
            string street = "";
            string description = "";
            string date = "";
            string checkCoord = "";
            int status = state;
            await Dispatcher.BeginInvoke((Action)(() =>
            {
                placeName = PlaatsTB.Text;
                zipCode = Regex.Replace(PostcodeTB.Text, @" ", "");
                homeNumber = HuisnummerTB.Text;
                street = StraatTB.Text;
                description = OmschijvingTB.Text;
                date = DatumTB.Text;
                checkCoord = street + ' ' + homeNumber + ' ' + placeName;

            }));

            StringBuilder homeNumbernum =
                  new StringBuilder();
            StringBuilder homeNumberLet =
                     new StringBuilder();

            if (status == 3)
            {

                placeName = "nvt";
                zipCode = "nvt";
                homeNumber = "0";
                street = "nvt";

            }

            for (int i = 0; i < homeNumber.Length; i++)
            {
                if (Char.IsDigit(homeNumber[i]))
                    homeNumbernum.Append(homeNumber[i]);
                else if ((homeNumber[i] >= 'A' &&
                         homeNumber[i] <= 'Z') ||
                         (homeNumber[i] >= 'a' &&
                          homeNumber[i] <= 'z'))
                    homeNumberLet.Append(homeNumber[i]);
            }

            StringBuilder zipCodeNum = new StringBuilder();
            StringBuilder zipCodeLet = new StringBuilder();

            for (int i = 0; i < zipCode.Length; i++)
            {
                if (Char.IsDigit(zipCode[i]))
                    zipCodeNum.Append(zipCode[i]);
                else if ((zipCode[i] >= 'A' &&
                         zipCode[i] <= 'Z') ||
                         (zipCode[i] >= 'a' &&
                          zipCode[i] <= 'z'))
                    zipCodeLet.Append(zipCode[i]);
            }
            zipCode = Convert.ToString(zipCodeNum) + Convert.ToString(zipCodeLet);

            if (!CheckCategorie())
            {
                errorMessage += "Categorie, ";
                errorBool = true;
            }
            if (description == "")
            {
                errorMessage += "Beschrijving, ";
                errorBool = true;
            }
            if (state != 3)
            {
                if (placeName == "")
                {
                    errorMessage += "Plaats, ";
                    errorBool = true;
                }
                if (zipCode == "" || zipCode.Length != 6 || zipCodeLet.Length != 2 || zipCodeNum.Length != 4)
                {
                    errorMessage += "Postcode, ";
                    errorBool = true;
                }
                if (homeNumber.Length == 0 || homeNumbernum.Length == 0 || homeNumberLet.Length > 1)
                {
                    errorMessage += "Huisnummer, ";
                    errorBool = true;
                }
                if (street == "")
                {
                    errorMessage += "Straat, ";
                    errorBool = true;
                }
            }
            if (status != 3)
            {
                await SearchCoordAsync(checkCoord, zipCode.ToUpper());
            }
            if (errorBool) //Hieronder alles wat gedaan moet worden als er iets fout gaat.
            {
                await Dispatcher.BeginInvoke((Action)(() =>
                {
                    string errorBoxText = errorMessage.Substring(0, errorMessage.Length - 2);
                    mw.ShowMessage(errorBoxText);
                    errorBool = false;
                    errorMessage = "De volgende velden zijn niet correct ingevoerd: ";
                }));
            }
            else //Hieronder alles wat uitgevoerd moet worden als alles goed is.
            {
                SendDelictToDatabase(date, mw.FirstCharToUpper(placeName), int.Parse(homeNumbernum.ToString()), homeNumberLet.ToString().ToUpper(), zipCode.ToUpper(), mw.FirstCharToUpper(street), description, longCoord, latCoord);
            }
        }

        private void SaveDelict_Click(object sender, RoutedEventArgs e)
        {
            Thread t = new Thread(Run_This);
            t.Start();
        }
        // functie data controleren
        private async Task SearchCoordAsync(string check, string zip)
        {
            try
            {
                _geocoder = await LocatorTask.CreateAsync(_serviceUri);
                IReadOnlyList<SuggestResult> suggestions = await _geocoder.SuggestAsync(check);
                SuggestResult firstsuggestion = suggestions.First();
                IReadOnlyList<GeocodeResult> coords = await _geocoder.GeocodeAsync(firstsuggestion.Label);
                if (coords.Count < 1) { Console.WriteLine("NOPE geen resultaten"); return; } // GEEN RESULTATEN GEVONDEN!
                string xcoord = coords.First().DisplayLocation.X.ToString();
                string ycoord = coords.First().DisplayLocation.Y.ToString();
                double parseX = Double.Parse(xcoord);
                double parseY = Double.Parse(ycoord);

                MapPoint nieuwepoint = new MapPoint(coords.First().DisplayLocation.X, coords.First().DisplayLocation.Y);
                ReverseGeocodeParameters parameters = new ReverseGeocodeParameters();
                parameters.MaxResults = 1;
                IReadOnlyList<GeocodeResult> adres = await _geocoder.ReverseGeocodeAsync(nieuwepoint, parameters);
                GeocodeResult eerste = adres.First();
                string ZIP = eerste.Attributes["Postal"].ToString();
                string trimmed = ZIP.Replace(" ", string.Empty);
                Console.WriteLine(trimmed);

                if (zip != trimmed)
                {
                    errorMessage += "Adres gegevens, ";
                    errorBool = true;
                    return;
                }
                else
                {
                    longCoord = parseX;
                    latCoord = parseY;
                }



            }
            catch (Exception eas) { Console.WriteLine(eas); }
            cn.CloseConnection();

        }


        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            categoryCB.SelectedIndex = -1;
        }

        private void SendDelictToDatabase(string date, string placeName, int homeNumberNumber, string homeNumberLetters, string zipCode, string street, string description, double longCoord, double latCoord)
        {
            string sqlDelictInsert = "insert into dbo.delict (date, place, housenumber,housenumberAddition, zipcode, street, description, long, lat, status, added_date) OUTPUT INSERTED.delict_id values(@first,@second,@third,@thirdAddition,@fourth,@fifth,@sixth,@seventh,@eight,@ninth,GETDATE())";

            int id = 0;
            int status = state;
            if (status == 3)
            {
                longCoord = longt;
                latCoord = latt;
            }
            else
            {
                status = 1;
            }
            using(SqlConnection connection = new SqlConnection(ConfigurationManager.AppSettings["connectionString"]))
            {
                connection.Open();
                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;

                transaction = connection.BeginTransaction("Delict Creation Transaction");

                command.Connection = connection;
                command.Transaction = transaction;

                try
                {
                    command.CommandText = "insert into dbo.delict (date, place, housenumber,housenumberAddition, zipcode, street, description, long, lat, status, added_date) OUTPUT INSERTED.delict_id values(@first,@second,@third,@thirdAddition,@fourth,@fifth,@sixth,@seventh,@eight,@ninth,GETDATE())";
                    command.Parameters.Add("@first", SqlDbType.DateTime).Value = date;
                    command.Parameters.Add("@second", SqlDbType.NVarChar).Value = placeName;
                    command.Parameters.Add("@third", SqlDbType.Int).Value = homeNumberNumber;
                    command.Parameters.Add("@thirdAddition", SqlDbType.NVarChar).Value = homeNumberLetters;
                    command.Parameters.Add("@fourth", SqlDbType.NVarChar).Value = zipCode;
                    command.Parameters.Add("@fifth", SqlDbType.NVarChar).Value = street;
                    command.Parameters.Add("@sixth", SqlDbType.NVarChar).Value = description;
                    command.Parameters.Add("@seventh", SqlDbType.Float).Value = longCoord;
                    command.Parameters.Add("@eight", SqlDbType.Float).Value = latCoord;
                    command.Parameters.Add("@ninth", SqlDbType.NVarChar).Value = status;

                    id = (int)command.ExecuteScalar();

                    if (personsbsn != null)
                    {
                        foreach (var item in person_id)
                        {
                            command.Parameters.Clear();
                            command.CommandText = "insert into dbo.delict_person (delict_id, person_id, type) values (@delictIDPerson, @person_id, @type)";
                            command.Parameters.Add("@delictIDPerson", SqlDbType.NVarChar).Value = id;
                            command.Parameters.Add("@person_id", SqlDbType.NVarChar).Value = person_id[i];
                            command.Parameters.Add("@type", SqlDbType.NVarChar).Value = personstype[i];
                            command.ExecuteNonQuery();
                            i++;
                        }
                    }

                    foreach (var item in categoryList)
                    {
                        if (item.Check_Status == true)
                        {
                            command.Parameters.Clear();
                            command.CommandText = "insert into dbo.category_delict (delict_id, category_id) values (@delictIDCat,@categoryID)";
                            command.Parameters.Add("@delictIDCat", SqlDbType.NVarChar).Value = id;
                            command.Parameters.Add("@categoryID", SqlDbType.NVarChar).Value = item.Category_ID;
                            command.ExecuteNonQuery();
                        }
                    }
                    // Attempt to commit the transaction.
                    transaction.Commit();
                    Console.WriteLine("Both records are written to database.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Commit Exception Type: {0}", ex.GetType());
                    Console.WriteLine("  Message: {0}", ex.Message);

                    // Attempt to roll back the transaction.
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception ex2)
                    {
                        // This catch block will handle any errors that may have occurred
                        // on the server that would cause the rollback to fail, such as
                        // a closed connection.
                        Console.WriteLine("Rollback Exception Type: {0}", ex2.GetType());
                        Console.WriteLine("  Message: {0}", ex2.Message);
                    }
                }
            }
            Dispatcher.BeginInvoke((Action)(() =>
            {
                mw.ShowMessage("Delict toegevoegd");
                mw.LoadHomeScreen();
            }));
            cn.CloseConnection();
        }

        private bool CheckCategorie()
        {
            foreach (var item in categoryList)
            {
                if (item.Check_Status)
                {
                    return true;
                }
            }
            return false;
        }

        private void CancelDelict_Click(object sender, RoutedEventArgs e)
        {
            mw.LoadHomeScreen();
        }
        private void AddPerson_Click(object sender, RoutedEventArgs e, AddNewPerson addperson)
        {
            addperson.ShowDialog();
            personsbsn = addperson.bsnlist;
            personstype = addperson.typelist;
            person_id = addperson.person_idList;
        }
    }
}