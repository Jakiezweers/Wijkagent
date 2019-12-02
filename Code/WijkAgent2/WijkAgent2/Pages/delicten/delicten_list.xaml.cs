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
        int i;
        int j;
        public delicten_list(MainWindow MW)
        {
            mw = MW;
            InitializeComponent();
            string provider = ConfigurationManager.AppSettings["provider"];
            string connectionstring = ConfigurationManager.AppSettings["connectionString"];

            DbProviderFactory factory = DbProviderFactories.GetFactory(provider);

            using (DbConnection connection = factory.CreateConnection())
            {
                connection.ConnectionString = connectionstring;
                connection.Open();
                DbCommand command = factory.CreateCommand();

                command.Connection = connection;
                command.CommandText = "SELECT DISTINCT delict.delict_id, delict.street, delict.added_date, COUNT(person.firstname) as firstname, COUNT(person.lastname) FROM dbo.delict LEFT JOIN dbo.delict_person ON delict.delict_id = delict_person.delict_id LEFT JOIN dbo.person ON person.person_id = delict_person.delict_person_id WHERE delict.status = 1 GROUP BY delict.delict_id, delict.street, delict.added_date ";

                using (DbDataReader dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        int id = Convert.ToInt32(dataReader["delict_id"]);
                        int count = Convert.ToInt32(dataReader["firstname"]);
                        Delict d1 = new Delict();
                        d1.id = id;
                        d1.street = GetDelictCategory(id);
                        d1.createtime = (DateTime)dataReader["added_date"];

                        d1.firstnamecount = count;
                        Console.WriteLine($"{dataReader["street"]}");
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
                return returnString.Substring(0, returnString.Length - 2);
            }
        }
        private void Activate(object sender, RoutedEventArgs e)
        {
            MessageBoxResult dialogResult = MessageBox.Show("Wil u dit delict archiveren?", "Archiveren", MessageBoxButton.YesNo);
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

        private void ViewDelict(object sender, RoutedEventArgs e)
        {
            var DelictID = (int)((System.Windows.Controls.Button)sender).Tag;
            mw.ShowDelict(DelictID);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            mw.LoadHomeScreen();
        }
        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string provider = ConfigurationManager.AppSettings["provider"];
            string connectionstring = ConfigurationManager.AppSettings["connectionString"];
            var iddelict = Delicten.SelectedItem as Delict;
            DbProviderFactory factory = DbProviderFactories.GetFactory(provider);
            using (DbConnection connection = factory.CreateConnection())
            {
                connection.ConnectionString = connectionstring;
                connection.Open();
                DbCommand command = factory.CreateCommand();
                command.Connection = connection;
                command.CommandText = "SELECT firstname FROM dbo.person JOIN dbo.delict_person ON person.person_id = delict_person.person_id WHERE delict_person.delict_id =" + iddelict.id;
                personnames.Items.Clear();
                using (DbDataReader dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        personnames.Items.Add(dataReader["firstname"]);
                    }
                }
            }

        }
        private void KeyDownEvent(object sender, KeyEventArgs e)
        {
            i = Delicten.SelectedIndex;
            if (e.Key == Key.Right)
            {
                if (Delicten.SelectedIndex + 10 < Delicten.Items.Count)
                {
                    i = i + 10;
                    Delicten.SelectedItem = Delicten.Items[i];
                    Delicten.ScrollIntoView(Delicten.SelectedItem);
                    Delicten.Focus();
                    Console.WriteLine(Delicten.SelectedIndex.ToString());
                }
                else
                {
                    Delicten.SelectedItem = Delicten.Items[Delicten.Items.Count - 1];
                    Delicten.ScrollIntoView(Delicten.SelectedItem);
                    Delicten.Focus();
                }
            }

            if (e.Key == Key.Left)
            {
                if (Delicten.SelectedIndex - 10 > 0)
                {
                    i = i - 10;
                    Delicten.SelectedItem = Delicten.Items[i];
                    Delicten.ScrollIntoView(Delicten.SelectedItem);
                    Delicten.Focus();
                }
                else
                {
                    Delicten.SelectedItem = Delicten.Items[0];
                    Delicten.ScrollIntoView(Delicten.SelectedItem);
                    Delicten.Focus();
                }
            }
        }
    }
}
