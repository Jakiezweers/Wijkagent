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
using Wijkagent2.Classes;

namespace WijkAgent2.Pages.delicten
{
    /// <summary>
    /// Interaction logic for lijst.xaml
    /// </summary>
    public partial class delicten_list : Page
    {
        MainWindow mw;
        public delicten_list(MainWindow MW)
        {
            mw = MW;
            getData();
        }
       private void getData() { 
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
                command.CommandText = "SELECT * FROM dbo.delict " +
                                      "WHERE status = 1";
                Delicten.Items.Clear();
                using (DbDataReader dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        int id = Convert.ToInt32(dataReader["delict_id"]);
                        Delict d1 = new Delict();
                        d1.id = id;
                        d1.street = (string)dataReader["street"];
                        d1.createtime = (DateTime)dataReader["added_date"];
                       // Console.WriteLine($"{dataReader["street"]}");
                        Delicten.Items.Add(d1);
                    }
                }
            }
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void Archive(object sender, RoutedEventArgs e)
        {
            MessageBoxResult dialogResult = MessageBox.Show("Sure", "Some Title", MessageBoxButton.YesNo);
            if (dialogResult == MessageBoxResult.Yes)
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


                    var myValue = ((System.Windows.Controls.Button)sender).Tag;
                    string statusChange = "UPDATE delict " +
                                     "SET status = 0 " +
                                     "WHERE delict_id = @delictID";
                    //TODO add use id 
                    string addToArchive = "INSERT INTO dbo.archive (delict_id, date_added) " +
                                          "VALUES (@delictID, @Datetime)";

                    using (SqlConnection cnn = new SqlConnection(connectionstring))
                    {
                        try
                        {
                            cnn.Open();
                            using (SqlCommand cmd = new SqlCommand(statusChange, cnn))
                            {
                                cmd.Parameters.Add("@delictID", SqlDbType.NVarChar).Value = myValue;
                                cmd.ExecuteNonQuery();


                            }
                            using (SqlCommand cmd = new SqlCommand(addToArchive, cnn))
                            {

                                cmd.Parameters.Add("@delictID", SqlDbType.NVarChar).Value = myValue;
                                cmd.Parameters.Add("@Datetime", SqlDbType.DateTime).Value = DateTime.Today;
                                cmd.ExecuteNonQuery();
                            }
                            getData();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("ERROR:" + ex.Message);
                        }

                        Console.WriteLine("ID: " + myValue);
                    }
                }
            }
            else if (dialogResult == MessageBoxResult.No)
            {
                //do something else
            }



        }
    }
}
