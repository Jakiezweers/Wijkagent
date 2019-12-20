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
using System.Windows.Shapes;
using Wijkagent.Classes;

namespace Wijkagent
{
    /// <summary>
    /// Interaction logic for DelictListWindow.xaml
    /// </summary>
    public partial class DelictListWindow : Window
    {
        public DelictListWindow()
        {
            InitializeComponent();
<<<<<<< HEAD

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
                command.CommandText = "SELECT * FROM dbo.delict " +
                                      "WHERE status = 1";

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
=======
            
>>>>>>> cdd1651fb30f803bf92cb4f94727c36e78c5a78f
        }
    }
}
