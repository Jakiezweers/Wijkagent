using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Wijkagent
{


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();

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
                command.CommandText = "Select * from dbo.delict";

                using (DbDataReader dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        int id = Convert.ToInt32(dataReader["id"]);
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

        private void Activate(object sender, RoutedEventArgs e)
        {
            var myValue = ((System.Windows.Controls.Button)sender).Tag;
            Console.WriteLine("ID: " + myValue);
            sender.ToString();
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
