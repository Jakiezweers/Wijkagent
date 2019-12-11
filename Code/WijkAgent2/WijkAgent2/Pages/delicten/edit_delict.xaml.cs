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
using WijkAgent2.Modals;

namespace WijkAgent2.Pages.delicten
{
    /// <summary>
    /// Interaction logic for edit_delict.xaml
    /// </summary>
    public partial class edit_delict : Page
    {
        MainWindow mw;
        int currDelictID;
        List<CategoryList> categoryList = new List<CategoryList>();
        List<int> personsbsn = new List<int>();
        List<string> personstype = new List<string>();
        List<int> person_id = new List<int>();
        public edit_delict(MainWindow MW, int delictID)
        {
            InitializeComponent();
            mw = MW;
            currDelictID = delictID;
            LoadDelict(currDelictID);
        }

        private void LoadDelict(int currDelictID)
        {
            string provider = ConfigurationManager.AppSettings["provider"];
            string connectionstring = ConfigurationManager.AppSettings["connectionString"];

            DbProviderFactory factory = DbProviderFactories.GetFactory(provider);

            using (DbConnection connection = factory.CreateConnection())
            {
                connection.ConnectionString = connectionstring;
                connection.Open();
                DbCommand command = factory.CreateCommand();

                command.Connection = connection;
                command.CommandText = "SELECT * FROM dbo.delict WHERE delict_id = " + currDelictID;

                using (DbDataReader dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        string status = "";
                        if ((int)dataReader["status"] == 1)
                        {
                            status = "Actief";
                        }
                        else
                        {
                            status = "Inactief";
                        }
                        DelictPlaceLabel.Text = (string)dataReader["place"];
                        DelictIDLabel.Content += ": " + currDelictID;
                        DelictStreetLabel.Text = (string)dataReader["street"];
                        DelictHouseNumberLabel.Text = "" + dataReader["housenumber"] + " " + dataReader["housenumberAddition"];
                        DelictZipcodeLabel.Text = (string)dataReader["zipcode"];
                        DelictStatusLabel.Content += ": " + status;
                        DelictDescriptionTB.Text = (string)dataReader["description"];
                        DelictDateLabel.Content += ": " + dataReader["added_date"];
                    }
                }

                command.CommandText = "Select * from dbo.category";

                using (DbDataReader dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        CategoryList obj = new CategoryList((int)dataReader["category_id"], (string)(dataReader["name"]));
                        categoryList.Add(obj);
                    }
                }

                command.CommandText = "SELECT category.category_id FROM category_delict JOIN category ON category.category_id = category_delict.category_id WHERE delict_id =" + currDelictID;
                using (DbDataReader dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        foreach (var item in categoryList)
                        {
                            if (item.Category_ID == (int)dataReader["category_id"])
                            {
                                item.Check_Status = true;
                            }
                        }
                    }
                }
                BindCategoryDropDown();
                BindListBOX();
                command.CommandText = "SELECT dp.person_id, p.bsn, dp.type FROM delict_person dp JOIN person p on dp.person_id = p.person_id WHERE delict_id = " + currDelictID;
                using (DbDataReader dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        person_id.Add((int)dataReader["person_id"]);
                        personsbsn.Add((int)dataReader["bsn"]);
                        personstype.Add((string)dataReader["type"]);
                        RefreshPersonList();
                    }
                }
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            mw.ShowDelict(currDelictID);
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
                if (homeNumber.Length == 0 || homeNumbernum.Length == 0 || homeNumberLet.Length > 1 )
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
                mw.ShowMessage("Delict niet aangepast.");
            }
        }

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
                    for (int i = 0; i < person_id.Count; i++)
                    {
                        using (SqlCommand cmd = new SqlCommand(sqlPersonInsert, cnn))
                        {
                            cmd.Parameters.Add("@delictID", SqlDbType.Int).Value = currDelictID;
                            cmd.Parameters.Add("@personID", SqlDbType.Int).Value = person_id[i];
                            cmd.Parameters.Add("@type", SqlDbType.NVarChar).Value = personstype[i];

                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("ERROROR?:!" + ex.Message);
                }
                mw.ShowDelict(currDelictID);
                mw.ShowMessage("Delict succesvol gewijzigd");
            }
        }

        private void AddPerson_Click(object sender, RoutedEventArgs e)
        {
            AddNewPerson addperson = new AddNewPerson(mw, personstype, personsbsn, person_id);
            addperson.RefreshData();
            addperson.ShowDialog();
            personsbsn = addperson.bsnlist;
            personstype = addperson.typelist;
            person_id = addperson.person_idList;
            RefreshPersonList();
        }
        private void RefreshPersonList()
        {
            PersonenListbox.Items.Clear();
            for (int i = 0; i < person_id.Count; i++)
            {
                string text = personstype[i] + " - " + personsbsn[i];
                PersonenListbox.Items.Add(text);
            }
        }
    }
}