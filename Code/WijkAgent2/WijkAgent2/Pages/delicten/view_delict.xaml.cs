using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
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
    /// Interaction logic for view_delict.xaml
    /// </summary>
    public partial class view_delict : Page
    {
        MainWindow mw;
        bool redirect = true;
        public view_delict(MainWindow MW, int delictID)
        {
            InitializeComponent();
            LoadDelict(delictID);
            mw = MW;
        }

        private void LoadDelict(int delictID)
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
                command.CommandText = "SELECT * FROM dbo.delict WHERE delict_id = " + delictID;

                using (DbDataReader dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        string status = "";
                        if((int)dataReader["status"] == 1)
                        {
                            status = "Actief";
                            redirect = true;
                        } else
                        {
                            status = "Inactief";
                        }

                        DelictPlaceLabel.Content += ": " + dataReader["place"];
                        DelictIDLabel.Content += ": " + dataReader["delict_id"];
                        DelictStreetLabel.Content += ": " + dataReader["street"];
                        DelictStreetLabel.Content += ": " + dataReader["housenumber"];
                        DelictZipcodeLabel.Content += ": " + dataReader["zipcode"];
                        DelictStatusLabel.Content += ": " + status;
                        DelictDescriptionTB.Text = (string)dataReader["description"];
                        DelictDateLabel.Content += ": " + dataReader["added_date"];
                    }
                }
                command.CommandText = "SELECT category.name FROM category_delict JOIN category ON category.category_id = category_delict.category_id WHERE delict_id = " + delictID;
                using (DbDataReader dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        CategoryListbox.Items.Add(dataReader["name"]);
                    }
                }
                command.CommandText = "SELECT bsn, type FROM delict_person WHERE delict_id = " + delictID;
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
            if (redirect)
            {
                mw.ShowDelictenList();
            }
            mw.ShowDelictenArchive();
        }
    }
}
