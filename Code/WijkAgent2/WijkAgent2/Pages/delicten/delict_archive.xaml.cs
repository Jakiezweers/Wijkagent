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
using Wijkagent2.Classes;

namespace WijkAgent2.Pages.delicten
{
    /// <summary>
    /// Interaction logic for delict_archive.xaml
    /// </summary>
    public partial class delict_archive : Page
    {
        private MainWindow mw;
        public delict_archive(MainWindow MW)
        {
            mw = MW;
            InitializeComponent(); string provider = ConfigurationManager.AppSettings["provider"];
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
                command.CommandText = "SELECT * FROM dbo.archive as a " +
                                      "JOIN dbo.delict as d ON a.delict_id = d.delict_id " +
                                      "WHERE d.status = 0 ";


                using (DbDataReader dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        int id = Convert.ToInt32(dataReader["delict_id"]);
                        Delict d1 = new Delict();
                        d1.id = id;
                        d1.street = (string)dataReader["street"];
                        d1.createtime = (DateTime)dataReader["added_date"];
                        Console.WriteLine($"{dataReader["street"]}");
                        Delicten.Items.Add(d1);
                    }
                }

            }
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void Activate(object sender, RoutedEventArgs e)
        {
            var myValue = ((System.Windows.Controls.Button)sender).Tag;
            Console.WriteLine("ID: " + myValue);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            mw.LoadHomeScreen();
        }
    }

}