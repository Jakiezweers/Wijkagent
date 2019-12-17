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
                command.CommandText = "SELECT d.delict_id, u.user_id, d.description, a.date_added FROM dbo.archive as a JOIN dbo.delict as d ON a.delict_id = d.delict_id JOIN dbo.[User] as u ON a.user_id = u.user_id WHERE d.status = 0 ORDER BY a.delict_id";


                using (DbDataReader dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        int id = Convert.ToInt32(dataReader["delict_id"]);
                        Delict d1 = new Delict();
                        d1.id = id;
                        d1.street = GetDelictCategory(id);
                        d1.changedBy = (int)dataReader["user_id"];
                        d1.addedDate = (DateTime)dataReader["date_added"];
                        Delicten.Items.Add(d1);
                    }
                }

            }
        }
        private string GetDelictCategory(int delictID)
        {
            string returnString = "";
            string provider = ConfigurationManager.AppSettings["provider"];
            string connectionstring = ConfigurationManager.AppSettings["connectionString"];

            DbProviderFactory factory = DbProviderFactories.GetFactory(provider);

            using (DbConnection connection = factory.CreateConnection())
            {
                connection.ConnectionString = connectionstring;
                connection.Open();
                DbCommand command = factory.CreateCommand();

                command.Connection = connection;
                command.CommandText = "SELECT name FROM category_delict JOIN category ON category.category_id = category_delict.category_id WHERE delict_id = " + delictID;
                using (DbDataReader dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        returnString += dataReader["name"];
                        returnString += ", ";
                    }
                }
                if (returnString.Length > 2)
                {
                    return returnString.Substring(0, returnString.Length - 2);
                }
                return returnString;
            }
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void Activate(object sender, RoutedEventArgs e)
        {
            MessageBoxResult dialogResult = MessageBox.Show("Wilt u dit delict activeren?", "Activeren", MessageBoxButton.YesNo);
            if (dialogResult == MessageBoxResult.Yes)
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


                            }
                            //Delicten_archief.


                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("ERROR:" + ex.Message);
                        }

                        Console.WriteLine("ID: " + myValue);

                    }
                }
                var currentRowIndex = Delicten.Items.IndexOf(Delicten.CurrentItem);
                Delicten.Items.RemoveAt(currentRowIndex);
            }
            else if (dialogResult == MessageBoxResult.No)
            {
                //do something else
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            mw.LoadHomeScreen();
        }
        private void ViewDelict(object sender, RoutedEventArgs e)
        {
            var DelictID = (int)((System.Windows.Controls.Button)sender).Tag;
            mw.ShowDelict(DelictID);
        }
    }
}