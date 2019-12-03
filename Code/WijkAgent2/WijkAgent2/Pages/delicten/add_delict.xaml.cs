﻿using System;
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
    /// Interaction logic for add_delict.xaml
    /// </summary>
    public partial class add_delict : Page
    {
        List<CategoryList> categoryList = new List<CategoryList>();
        List<int> personsbsn = new List<int>();
        List<string> personstype = new List<string>();
        List<int> person_id = new List<int>();
        int i = 0;
        private MainWindow mw;
        public add_delict(MainWindow MW)
        {
            this.mw = MW;
            InitializeComponent();
            categoryList = new List<CategoryList>();
            BindCategroryDropDown();
            DatumTB.SelectedDate = DateTime.Today;

            personentoevoegen addperson = new personentoevoegen(mw);
            AddPersonButton.Click += (sender, EventArgs) => { AddPerson_Click(sender, EventArgs, addperson); };


            string provider = ConfigurationManager.AppSettings["provider"];
            string connectionstring = ConfigurationManager.AppSettings["connectionString"];

            DbProviderFactory factory = DbProviderFactories.GetFactory(provider);

            using (DbConnection connection = factory.CreateConnection())
            {
                connection.ConnectionString = connectionstring;
                connection.Open();
                DbCommand command = factory.CreateCommand();

                command.Connection = connection;
                command.CommandText = "Select * from dbo.category";

                using (DbDataReader dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        CategoryList obj = new CategoryList((int)dataReader["category_id"], (string)(dataReader["name"]));
                        categoryList.Add(obj);
                    }
                }

            }
        }

        private void BindCategroryDropDown()
        {
            categoryCB.ItemsSource = categoryList;
        }

        private void category_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(!categoryCB.IsDropDownOpen)
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

        private void SaveDelict_Click(object sender, RoutedEventArgs e)
        {
            string errorMessage = "De volgende velden zijn niet correct ingevoerd: ";
            bool errorBool = false;
            string placeName = PlaatsTB.Text;
            string zipCode = Regex.Replace(PostcodeTB.Text, @" ", "");
            string homeNumber = HuisnummerTB.Text;
            string street = StraatTB.Text;
            string description = OmschijvingTB.Text;
            string date = DatumTB.Text;
            double longCoord = 0.0000;
            double latCoord = 0.0000;

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
            if (zipCode == "" || zipCode.Length != 6)
            {
                errorMessage += "Postcode, ";
                errorBool = true;
            }
            if (homeNumber == "")
            {
                errorMessage += "Huisnummer, ";
                errorBool = true;
            }
            if(street == "")
            {
                errorMessage += "Straat, ";
                errorBool = true;
            }

            if (errorBool == false) //Hieronder alles wat uitgevoerd moet worden als alles goed is.
            {
                SendDelictToDatabase(date, placeName, homeNumber, zipCode, street, description, longCoord, latCoord);
                mw.ShowMessage("Delict toegevoegd");
            }
            else //Hieronder alles wat gedaan moet worden als er iets fout gaat.
            {
                string errorBoxText = errorMessage.Substring(0, errorMessage.Length - 2);
                string errorCaption = "Delict toevoegen mislukt.";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBox.Show(errorBoxText, errorCaption, button);
            }
        }

        private void GetLat()
        {

        }
        private void GetLong()
        {

        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            categoryCB.SelectedIndex = -1;
        }

        private void SendDelictToDatabase(string date, string placeName, string homeNumber, string zipCode, string street, string description, double longCoord, double latCoord)
        {
            string provider = ConfigurationManager.AppSettings["provider"];
            string connectionstring = ConfigurationManager.AppSettings["connectionString"];

            string sqlDelictInsert = "insert into dbo.delict (date, place, housenumber, zipcode, street, description, long, lat, status, added_date) OUTPUT INSERTED.delict_id values(@first,@second,@third,@fourth,@fifth,@sixth,@seventh,@eight,@ninth,GETDATE())";
            string sqlCategoryInsert = "insert into dbo.category_delict (delict_id, category_id) values (@delictID,@categoryID)";

            int id = 0;
            using (SqlConnection cnn = new SqlConnection(connectionstring))
            {
                try
                {
                    cnn.Open();
                    using (SqlCommand cmd = new SqlCommand(sqlDelictInsert, cnn))
                    {
                        cmd.Parameters.Add("@first", SqlDbType.DateTime).Value = date;
                        cmd.Parameters.Add("@second", SqlDbType.NVarChar).Value = placeName;
                        cmd.Parameters.Add("@third", SqlDbType.NVarChar).Value = homeNumber;
                        cmd.Parameters.Add("@fourth", SqlDbType.NVarChar).Value = zipCode;
                        cmd.Parameters.Add("@fifth", SqlDbType.NVarChar).Value = street;
                        cmd.Parameters.Add("@sixth", SqlDbType.NVarChar).Value = description;
                        cmd.Parameters.Add("@seventh", SqlDbType.NVarChar).Value = longCoord;
                        cmd.Parameters.Add("@eight", SqlDbType.NVarChar).Value = latCoord;
                        cmd.Parameters.Add("@ninth", SqlDbType.NVarChar).Value = 1;

                        id = (int)cmd.ExecuteScalar();
                    }

                    if (personsbsn != null)
                    {
                        string sqlPersonInsert = "insert into dbo.delict_person (delict_id, person_id, type) values (@delictID, @person_id, @type)";
                        //insert personen in database
                        foreach (var item in person_id)
                        {
                            using (SqlCommand cmd = new SqlCommand(sqlPersonInsert, cnn))
                            {
                                cmd.Parameters.Add("@delictID", SqlDbType.NVarChar).Value = id;
                                cmd.Parameters.Add("@person_id", SqlDbType.NVarChar).Value = person_id[i];
                                cmd.Parameters.Add("@type", SqlDbType.NVarChar).Value = personstype[i];
                                cmd.ExecuteNonQuery();
                                i++;
                            }

                        }
                    }

                    //Insert delict met gekoppelde categorieen in de database.
                    foreach (var item in categoryList)
                    {
                        if (item.Check_Status == true)
                        {
                            using (SqlCommand cmd = new SqlCommand(sqlCategoryInsert, cnn))
                            {
                                cmd.Parameters.Add("@delictID", SqlDbType.NVarChar).Value = id;
                                cmd.Parameters.Add("@categoryID", SqlDbType.NVarChar).Value = item.Category_ID;
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("ERROROR?:" + ex.Message);
                }
                mw.ShowDelictenList();
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

        private void CancelDelict_Click(object sender, RoutedEventArgs e)
        {
            mw.ShowDelictenList();
        }
        private void AddPerson_Click(object sender, RoutedEventArgs e, personentoevoegen addperson)
        {
            addperson.ShowDialog();
            personsbsn = addperson.bsnlist;
            personstype = addperson.typelist;
            person_id = addperson.person_idList;
        }
    }
}
