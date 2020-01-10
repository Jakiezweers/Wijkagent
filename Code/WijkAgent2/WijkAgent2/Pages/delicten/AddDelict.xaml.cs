﻿using Esri.ArcGISRuntime.Geometry;
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
    public partial class AddDelict : Page
    {
        private readonly MainWindow mw;//Required mainwindow

        readonly List<CategoryList> categoryList = new List<CategoryList>();//List containing all categories.

        public List<int> personBSN = new List<int>();//List containing the BSN number of a person
        List<string> personType = new List<string>();//List containing the type / categorie of a person (verdachte, slachtoffer etc...)
        List<int> personID = new List<int>();//List containing personID's connected to the delict.

        LocatorTask _geocoder;

        Uri _serviceUri = new Uri("https://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer");

        int state; //Refers to state of the Delict: active, inactive, mapmarked

        double longCoord = 0.0000; //Long needed to be generated by ARCgis to be added to delict.
        double latCoord = 0.0000; //Lat needed to be generated by ARCgis to be added to delict.

        double longt = 0.0000; //Long needed when creating a delict trough a click on the map.
        double latt = 0.0000; //Lat needed when creating a delict trough a click on the map.

        string errorMessage = "De volgende velden zijn niet correct ingevoerd: "; //Start of the errormessage from the fields verification when creating a delict. Additional messages get appended to show a correct error message.
        bool errorBool = false; //If this errorBool is false the data will be uploaded. If its true the user will be notified that there is an error and no data will be uploaded.

        private Connection cn = new Connection(); //Connection object for the database connection

        int i = 0; //Counter for indexes used when inserting persons into the database.
       
        //Constructor used when creating a new delict without using the map.
        public AddDelict(MainWindow MW)
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

        //Constructor used when creating a new delict trough the map. This way the Long and Lat exist already and the user doesnt have to fill in location based fields.
        public AddDelict(MainWindow MW, double lon, double lat)
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

        //Triggers the save delict statement on a seperate thread.
        private void SaveDelict_Click(object sender, RoutedEventArgs e)
        {
            Thread t = new Thread(Run_This);
            t.Start();
        }

        //Method that checks every field. Makes sure the data saved is the same for every delict even if user input differs. This fires the SearchCoordAsync method to check the filled in location fields. If everything is right the delict gets uploaded to the database.
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
            //location data is not inputted to delict if made via map
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
            if (errorBool) //Reports on the error and lists what went wrong
            {
                await Dispatcher.BeginInvoke((Action)(() =>
                {
                    string errorBoxText = errorMessage.Substring(0, errorMessage.Length - 2);
                    mw.ShowMessage(errorBoxText);
                    errorBool = false;
                    errorMessage = "De volgende velden zijn niet correct ingevoerd: ";
                }));
            }
            else //Fucntion will execute a SQL query to insert an delict
            {
                SendDelictToDatabase(date, mw.FirstCharToUpper(placeName), int.Parse(homeNumbernum.ToString()), homeNumberLet.ToString().ToUpper(), zipCode.ToUpper(), mw.FirstCharToUpper(street), description, longCoord, latCoord);
            }
        }

        //Check if the filled in location data correspends with eachother. If not the errorMessage is fired. If it is correct it sends a correct LAT and LONG based on the place,street and zipcode.
        private async Task SearchCoordAsync(string check, string zip)
        {
            try
            {
                _geocoder = await LocatorTask.CreateAsync(_serviceUri);
                IReadOnlyList<SuggestResult> suggestions = await _geocoder.SuggestAsync(check);
                SuggestResult firstSuggestion = suggestions.First();
                IReadOnlyList<GeocodeResult> coords = await _geocoder.GeocodeAsync(firstSuggestion.Label);
                if (coords.Count < 1) { return; } // No results
                string xCoord = coords.First().DisplayLocation.X.ToString();
                string yCoord = coords.First().DisplayLocation.Y.ToString();
                double parseX = Double.Parse(xCoord);
                double parseY = Double.Parse(yCoord);

                MapPoint newPoint = new MapPoint(coords.First().DisplayLocation.X, coords.First().DisplayLocation.Y);
                ReverseGeocodeParameters parameters = new ReverseGeocodeParameters();
                parameters.MaxResults = 1;
                IReadOnlyList<GeocodeResult> adres = await _geocoder.ReverseGeocodeAsync(newPoint, parameters);
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
        //Function to procces the input of the AddDelict page.
        private void SendDelictToDatabase(string date, string placeName, int homeNumberNumber, string homeNumberLetters, string zipCode, string street, string description, double longCoord, double latCoord)
        {
            int id = 0;
            int status = state;
            //Assigns a value if delict is made via the map
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

                    if (personBSN != null)
                    {
                        foreach (var item in personID)
                        {
                            command.Parameters.Clear();
                            command.CommandText = "insert into dbo.delict_person (delict_id, person_id, type) values (@delictIDPerson, @person_id, @type)";
                            command.Parameters.Add("@delictIDPerson", SqlDbType.NVarChar).Value = id;
                            command.Parameters.Add("@person_id", SqlDbType.NVarChar).Value = personID[i];
                            command.Parameters.Add("@type", SqlDbType.NVarChar).Value = personType[i];
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

        //Button to cancel creating the delict and gets navigated back to the mainscreen.
        private void CancelDelict_Click(object sender, RoutedEventArgs e)
        {
            mw.LoadHomeScreen();
        }

        //Opens a modal to add persons to a delict. 
        private void AddPerson_Click(object sender, RoutedEventArgs e, AddNewPerson addperson)
        {
            addperson.ShowDialog();
            personBSN = addperson.bsnlist;
            personType = addperson.typelist;
            personID = addperson.person_idList;
        }

        //---Dropdown categories---//
        private void BindCategroryDropDown()
        {
            categoryCB.ItemsSource = categoryList;
        }

        private void Category_TextChanged(object sender, TextChangedEventArgs e)
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
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            categoryCB.SelectedIndex = -1;
        }
    }
}