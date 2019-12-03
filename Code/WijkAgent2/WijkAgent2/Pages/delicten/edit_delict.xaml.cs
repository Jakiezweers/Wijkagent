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

namespace WijkAgent2.Pages.delicten
{
    /// <summary>
    /// Interaction logic for edit_delict.xaml
    /// </summary>
    public partial class edit_delict : Page
    {
        MainWindow mw;
        int currDelictID;
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
                command.CommandText = "SELECT category.name FROM category_delict JOIN category ON category.category_id = category_delict.category_id WHERE delict_id = " + currDelictID;
                using (DbDataReader dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        CategoryListbox.Items.Add(dataReader["name"]);
                    }
                }
                command.CommandText = "SELECT p.bsn, dp.type FROM delict_person dp JOIN person p on dp.person_id = p.person_id WHERE delict_id = " + currDelictID;
                using (DbDataReader dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        string text = dataReader["type"] + " - " + dataReader["bsn"];
                        PersonenListbox.Items.Add(text);
                    }
                }
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            mw.ShowDelict(currDelictID);
        }

        private void SaveEditDelict_Click(object sender, RoutedEventArgs e)
        {
            string provider = ConfigurationManager.AppSettings["provider"];
            string connectionstring = ConfigurationManager.AppSettings["connectionString"];

            string sqlEditDelict = "UPDATE delict SET place = @placePara, street = @streetPara, zipcode = @zipcodePara, housenumber = @housenumberPara, description = @descriptionPara WHERE delict_id = " + currDelictID;

            using (SqlConnection cnn = new SqlConnection(connectionstring))
            {
                try
                {
                    cnn.Open();
                    using (SqlCommand cmd = new SqlCommand(sqlEditDelict, cnn))
                    {
                        cmd.Parameters.Add("@placePara", SqlDbType.NVarChar).Value = DelictPlaceLabel.Text;
                        cmd.Parameters.Add("@streetPara", SqlDbType.NVarChar).Value = DelictStreetLabel.Text;
                        cmd.Parameters.Add("@zipcodePara", SqlDbType.NVarChar).Value = DelictZipcodeLabel.Text;
                        cmd.Parameters.Add("@housenumberPara", SqlDbType.Int).Value = DelictHouseNumberLabel.Text;
                        cmd.Parameters.Add("@descriptionPara", SqlDbType.NVarChar).Value = DelictDescriptionTB.Text;

                        cmd.ExecuteScalar();
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
    }
}
