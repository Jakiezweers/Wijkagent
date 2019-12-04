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
                        DelictHouseNumberLabel.Text = "" + dataReader["housenumber"];
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
                            if(item.Category_ID == (int)dataReader["category_id"])
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
            string provider = ConfigurationManager.AppSettings["provider"];
            string connectionstring = ConfigurationManager.AppSettings["connectionString"];

            using (SqlConnection cnn = new SqlConnection(connectionstring))
            {
                try
                {
                    cnn.Open();
                    string sqlEditDelict = "UPDATE delict SET place = @placePara, street = @streetPara, zipcode = @zipcodePara, housenumber = @housenumberPara, description = @descriptionPara WHERE delict_id = " + currDelictID;
                    using (SqlCommand cmd = new SqlCommand(sqlEditDelict, cnn))
                    {
                        cmd.Parameters.Add("@streetPara", SqlDbType.NVarChar).Value = DelictStreetLabel.Text;
                        cmd.Parameters.Add("@placePara", SqlDbType.NVarChar).Value = DelictPlaceLabel.Text;
                        cmd.Parameters.Add("@zipcodePara", SqlDbType.NVarChar).Value = DelictZipcodeLabel.Text;
                        cmd.Parameters.Add("@housenumberPara", SqlDbType.Int).Value = DelictHouseNumberLabel.Text;
                        cmd.Parameters.Add("@descriptionPara", SqlDbType.NVarChar).Value = DelictDescriptionTB.Text;

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
                                    MessageBox.Show("insertperson");
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
            personentoevoegen addperson = new personentoevoegen(mw,personstype,personsbsn,person_id);
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