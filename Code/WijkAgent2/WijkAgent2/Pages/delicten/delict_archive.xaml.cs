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
    /// Interaction logic for delict_archive.xaml
    /// </summary>
    public partial class delict_archive : Page
    {
        private MainWindow mw;
        public delict_archive(MainWindow MW)
        {
            mw = MW;
            getData();
        }
        private void getData() {
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
                                      "WHERE d.status = 0 " +
                                      "ORDER BY a.delict_id";

                Delicten_archief.Items.Clear();
                using (DbDataReader dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        int id = Convert.ToInt32(dataReader["delict_id"]);
                        Delict d1 = new Delict();
                        d1.id = id;
                        d1.street = (string)dataReader["street"];
                        d1.createtime = (DateTime)dataReader["added_date"];
                        //    Console.WriteLine($"{dataReader["street"]}");
                        Delicten_archief.Items.Add(d1);
                    }
                }

            }
        }
    
        public enum DialogResult { }


        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void Activate(object sender, RoutedEventArgs e)
        {

            MessageBoxResult dialogResult = MessageBox.Show("", "Archiveren", MessageBoxButton.YesNo);
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
                    string archive = "UPDATE delict " +
                                     "SET status = 1 " +
                                     "WHERE delict_id = @delictID";
                    //TODO add use id 
                    string toActivate = "DELETE FROM dbo.archive " +
                                         "WHERE delict_id = @delictID";

                    using (SqlConnection cnn = new SqlConnection(connectionstring))
                    {
                        try
                        {
                            cnn.Open();
                            using (SqlCommand cmd = new SqlCommand(archive, cnn))
                            {
                                cmd.Parameters.Add("@delictID", SqlDbType.NVarChar).Value = myValue;
                                cmd.ExecuteNonQuery();


                            }
                            using (SqlCommand cmd = new SqlCommand(toActivate, cnn))
                            {

                                cmd.Parameters.Add("@delictID", SqlDbType.NVarChar).Value = myValue;
                                cmd.ExecuteNonQuery();

                                // dgPlan.ItemsSource = null;
                                //dgPlan.ItemsSource = ;
                                getData();
                            }
                          


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