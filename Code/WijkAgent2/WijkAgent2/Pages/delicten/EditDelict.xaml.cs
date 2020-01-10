using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for EditDelict.xaml
    /// </summary>
    public partial class EditDelict : Page
    {
        readonly MainWindow mw; //Required mainwindow

        readonly int currDelictID; //Delict id of opened delict
        readonly List<CategoryList> categoryList = new List<CategoryList>(); //List containing all categories.
        List<int> personBSN = new List<int>(); //List containing the BSN number of a person
        List<string> personType = new List<string>(); //List containing the type / categorie of a person (verdachte, slachtoffer etc...)
        List<int> personID = new List<int>(); //List containing personID's connected to the delict.
        readonly int returnPage; //Return page number for pathing.

        private readonly Connection cn = new Connection();

        public EditDelict(MainWindow MW, int delictID, int previousPage)
        {
            InitializeComponent();
            mw = MW;
            currDelictID = delictID;
            LoadDelict(currDelictID);
            returnPage = previousPage;
        }

        //Loads all data connected to delicts (Delict itself, Categories connected to delict and persons connected to the delict.
        private void LoadDelict(int currDelictID)
        {
            cn.OpenConection();
            SqlDataReader sqDel = cn.DataReader("SELECT * FROM dbo.delict WHERE delict_id = " + currDelictID);
            while (sqDel.Read())
            {
                string status = "";
                if ((int)sqDel["status"] == 1)
                {
                    status = "Actief";
                }
                else
                {
                    status = "Inactief";
                }
                DelictPlaceLabel.Text = (string)sqDel["place"];
                DelictIDLabel.Content += ": " + currDelictID;
                DelictStreetLabel.Text = (string)sqDel["street"];
                DelictHouseNumberLabel.Text = "" + sqDel["housenumber"] + " " + sqDel["housenumberAddition"];
                DelictZipcodeLabel.Text = (string)sqDel["zipcode"];
                DelictStatusLabel.Content += ": " + status;
                DelictDescriptionTB.Text = (string)sqDel["description"];
                DelictDateLabel.Content += ": " + sqDel["added_date"];
            }
            cn.CloseConnection();
            cn.OpenConection();
            SqlDataReader sqCat = cn.DataReader("Select * from dbo.category");
            while (sqCat.Read())
            {
                CategoryList obj = new CategoryList((int)sqCat["category_id"], (string)(sqCat["name"]));
                categoryList.Add(obj);
            }
            cn.CloseConnection();
            cn.OpenConection();
            SqlDataReader sqCatActive = cn.DataReader("SELECT category.category_id FROM category_delict JOIN category ON category.category_id = category_delict.category_id WHERE delict_id =" + currDelictID);
            while (sqCatActive.Read())
            {
                foreach (var item in categoryList)
                {
                    if (item.Category_ID == (int)sqCatActive["category_id"])
                    {
                        item.Check_Status = true;
                    }
                }
            }
            cn.CloseConnection();
            cn.OpenConection();

            BindCategoryDropDown();
            BindListBOX();

            SqlDataReader sqPerson = cn.DataReader("SELECT dp.person_id, p.bsn, dp.type FROM delict_person dp JOIN person p on dp.person_id = p.person_id WHERE delict_id = " + currDelictID);
            while (sqPerson.Read())
            {
                personID.Add((int)sqPerson["person_id"]);
                personBSN.Add((int)sqPerson["bsn"]);
                personType.Add((string)sqPerson["type"]);
                RefreshPersonList();
            }
            cn.CloseConnection();
        }


        //Button click to go back.
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            mw.ShowDelict(currDelictID, returnPage);
        }

        //Method that fires on save, Checks all fields. If everything is allright it fires the UploadToDatabase method.
        private void SaveEditDelict_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult dialogResult = MessageBox.Show("Wil u dit delict wijzigen?", "Opslaan", MessageBoxButton.YesNo);
            if (dialogResult == MessageBoxResult.Yes)
            {

                string errorMessage = "De volgende velden zijn niet correct ingevoerd: ";
                bool errorBool = false;
                string placeName = DelictPlaceLabel.Text;
                string zipCode = Regex.Replace(DelictZipcodeLabel.Text, @" ", "");
                string homeNumber = DelictHouseNumberLabel.Text;
                string street = DelictStreetLabel.Text;
                string description = DelictDescriptionTB.Text;

                StringBuilder homeNumbernum =
                         new StringBuilder();
                StringBuilder homeNumberLet =
                         new StringBuilder();
                StringBuilder special =
                         new StringBuilder();

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

                if (errorBool) //Hieronder alles wat gedaan moet worden als er iets fout gaat.
                {
                    string errorBoxText = errorMessage.Substring(0, errorMessage.Length - 2);
                    string errorCaption = "Delict wijzigen mislukt.";
                    MessageBoxButton button = MessageBoxButton.OK;
                    MessageBox.Show(errorBoxText, errorCaption, button);
                }
                else //Hieronder alles wat uitgevoerd moet worden als alles goed is. 
                {
                    UploadToDatabase(mw.FirstCharToUpper(placeName), int.Parse(homeNumbernum.ToString()), homeNumberLet.ToString().ToUpper(), zipCode.ToUpper(), mw.FirstCharToUpper(street), description);
                    mw.ShowMessage("Delict gewijzigd");
                }
            }
            else if (dialogResult == MessageBoxResult.No)
            {
                mw.ShowDelict(currDelictID, returnPage);
                mw.ShowMessage("Delict niet aangepast.");
            }
        }

        //Method to update the rows in the database.
        private void UploadToDatabase(string placeName, int homeNumberNumber, string homeNumberLetters, string zipCode, string street, string description)
        {
            string provider = ConfigurationManager.AppSettings["provider"];
            string connectionstring = ConfigurationManager.AppSettings["connectionString"];

            using (SqlConnection cnn = new SqlConnection(connectionstring))
            {
                try
                {
                    cnn.Open();
                    string sqlEditDelict = "UPDATE delict SET place = @placePara, street = @streetPara, zipcode = @zipcodePara, housenumber = @housenumberPara,housenumberAddition = @housenumberAdditionPara, description = @descriptionPara WHERE delict_id = " + currDelictID;
                    using (SqlCommand cmd = new SqlCommand(sqlEditDelict, cnn))
                    {
                        cmd.Parameters.Add("@streetPara", SqlDbType.NVarChar).Value = street;
                        cmd.Parameters.Add("@placePara", SqlDbType.NVarChar).Value = placeName;
                        cmd.Parameters.Add("@zipcodePara", SqlDbType.NVarChar).Value = zipCode;
                        cmd.Parameters.Add("@housenumberPara", SqlDbType.Int).Value = homeNumberNumber;
                        cmd.Parameters.Add("@housenumberAdditionPara", SqlDbType.NVarChar).Value = homeNumberLetters;
                        cmd.Parameters.Add("@descriptionPara", SqlDbType.NVarChar).Value = description;

                        cmd.ExecuteNonQuery();
                    }

                    string sqlDeleteCategory = "DELETE FROM category_delict WHERE delict_id =" + currDelictID;
                    using (SqlCommand cmd = new SqlCommand(sqlDeleteCategory, cnn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    string sqlCategoryInsert = "insert into dbo.category_delict (delict_id, category_id) values (@delictID, @categoryID)";
                    foreach (var item in categoryList)
                    {
                        if (item.Check_Status == true)
                        {
                            using (SqlCommand cmd = new SqlCommand(sqlCategoryInsert, cnn))
                            {
                                cmd.Parameters.Add("@delictID", SqlDbType.NVarChar).Value = currDelictID;
                                cmd.Parameters.Add("@categoryID", SqlDbType.NVarChar).Value = item.Category_ID;
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }

                    string sqlDeletePerson = "DELETE FROM delict_person WHERE delict_id =" + currDelictID;
                    using (SqlCommand cmd = new SqlCommand(sqlDeletePerson, cnn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    string sqlPersonInsert = "insert into delict_person (delict_id, person_id, type) values (@delictID, @personID,@type)";
                    for (int i = 0; i < personID.Count; i++)
                    {
                        using (SqlCommand cmd = new SqlCommand(sqlPersonInsert, cnn))
                        {
                            cmd.Parameters.Add("@delictID", SqlDbType.Int).Value = currDelictID;
                            cmd.Parameters.Add("@personID", SqlDbType.Int).Value = personID[i];
                            cmd.Parameters.Add("@type", SqlDbType.NVarChar).Value = personType[i];

                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("ERROROR?:!" + ex.Message);
                }
                mw.ShowDelict(currDelictID, returnPage);
                mw.ShowMessage("Delict succesvol gewijzigd");
            }
        }

        //Opens the AddNewPerson modal to edit persons connected to the current delict.
        private void AddPerson_Click(object sender, RoutedEventArgs e)
        {
            AddNewPerson addperson = new AddNewPerson(mw, personType, personBSN, personID);
            addperson.RefreshData();
            addperson.ShowDialog();
            personBSN = addperson.bsnlist;
            personType = addperson.typelist;
            personID = addperson.person_idList;
            RefreshPersonList();
        }

        //Method to refresh the personlist.
        private void RefreshPersonList()
        {
            PersonenListbox.Items.Clear();
            for (int i = 0; i < personID.Count; i++)
            {
                string text = personType[i] + " - " + personBSN[i];
                PersonenListbox.Items.Add(text);
            }
        }

        //---Combobox----//

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
            CategoryListbox.Items.Clear();
            foreach (var category in categoryList)
            {
                if (category.Check_Status == true)
                {
                    CategoryListbox.Items.Add(category.Category_Name);
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
        private void BindCategoryDropDown()
        {
            categoryCB.ItemsSource = categoryList;
        }
    }
}