﻿using Google.Maps;
using Google.Maps.Geocoding;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
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
        List<CategoryList> categoryList;

        public AddDelictWindow()
        {
            InitializeComponent();
            categoryList = new List<CategoryList>();
            BindCountryDropDown();
            DatumTB.SelectedDate = DateTime.Today;

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


        private void BindCountryDropDown()
        {
            categoryCB.ItemsSource = categoryList;
        }
        private void category_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void category_TextChanged(object sender, TextChangedEventArgs e)
        {
            categoryCB.ItemsSource = categoryList.Where(x => x.Category_Name.StartsWith(categoryCB.Text.Trim()));
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
            string zipCode = PostcodeTB.Text;
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

            if (errorBool == false) //Hieronder alles wat uitgevoerd moet worden als alles goed is.
            {
                SendDelictToDatabase(date, placeName,homeNumber,zipCode,street,description,longCoord,latCoord);
                this.Close();
            }
            else //Hieronder alles wat gedaan moet worden als er iets fout gaat.
            {
                string errorBoxText = errorMessage.Substring(0, errorMessage.Length - 2);
                string errorCaption = "Delict toevoegen mislukt.";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBox.Show(errorBoxText, errorCaption, button);

            }
        }

        private void GetLongLat()
        {

        }

        private void SendDelictToDatabase(string date, string placeName, string homeNumber, string zipCode, string street, string description, double longCoord, double latCoord)
        {
            string provider = ConfigurationManager.AppSettings["provider"];
            string connectionstring = ConfigurationManager.AppSettings["connectionString"];

            string sqlDelictInsert = "insert into dbo.delict (date, place, homenumber, zipcode, street, description, long, lat, status, added_date) OUTPUT INSERTED.ID values(@first,@second,@third,@fourth,@fifth,@sixth,@seventh,@eight,@ninth,GETDATE())";
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
                    MessageBox.Show("ERROR:" + ex.Message);
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

        private void CancelDelict_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    public class CategoryList
    {
        public CategoryList(int id, string name)
        {
            Category_ID = id;
            Category_Name = name;
        }
        public int Category_ID
        {
            get;
            set;
        }
        public string Category_Name
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