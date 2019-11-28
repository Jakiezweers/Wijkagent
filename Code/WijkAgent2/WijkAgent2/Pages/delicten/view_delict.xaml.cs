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
                if (connection == null)
                {
                    Console.WriteLine("connection Error");
                    Console.ReadLine();
                    return;
                }
                Console.WriteLine("connection geslaagd");

                connection.ConnectionString = connectionstring;
                connection.Open();
                DbCommand command = factory.CreateCommand();
                if (command == null)
                {
                    Console.WriteLine("geen command gegeven");
                    Console.ReadLine();
                    return;
                }

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
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            mw.ShowDelictenList();
        }
    }
}
