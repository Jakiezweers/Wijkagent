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
using WijkAgent2.Classes;
using WijkAgent2.Database;

namespace WijkAgent2.Pages.delicten
{
    public partial class delicten_list : Page
    {
        MainWindow mw;
        int i;
        int j;
        List<Delict> delictenlist = new List<Delict>();
        List<Delict> delictenlistCheck = new List<Delict>();

        bool currPageIsActivated = true;

        bool sortID = false;
        bool sortDate = false;

        List<CategoryList> categoryList = new List<CategoryList>();
        List<String> emptylist = new List<String>();
        static int pageCounter = 1;
        decimal delictsPerPage = 10;


        private Connection cn = new Connection();
        public delicten_list(MainWindow MW, bool activeDelicts)
        {
            mw = MW;
            InitializeComponent();
            currPageIsActivated = activeDelicts;

            cn.OpenConection();
            SqlDataReader sq = cn.DataReader("Select * from dbo.category");
            while (sq.Read())
            {
                CategoryList obj = new CategoryList((int)sq["category_id"], (string)(sq["name"]));
                categoryList.Add(obj);
            }
            cn.CloseConnection();
            BindCategroryDropDown();
            if (currPageIsActivated)
            {
                DelictListSwapBTN.Content = "Gearchiveerde delicten lijst";
                GetActiveDelicts();
            }
            else
            {
                DelictListSwapBTN.Content = "Actieve delicten lijst";
                GetArchivedDelicts();
            }
        }
        public void GetActiveDelicts()
        {
            delictenlistCheck.Clear();
            delictenlist.Clear();
            currPageIsActivated = true;
            mw.TopHeader.Text = "Wijkagent - Delicten lijst";
            DelictArchiveBTN.Visibility = Visibility.Visible;
            DelictActivateBTN.Visibility = Visibility.Hidden;
            string provider = ConfigurationManager.AppSettings["provider"];
            string connectionstring = ConfigurationManager.AppSettings["connectionString"];

            DbProviderFactory factory = DbProviderFactories.GetFactory(provider);

            using (DbConnection connection = factory.CreateConnection())
            {
                connection.ConnectionString = connectionstring;
                connection.Open();
                DbCommand command = factory.CreateCommand();

                command.Connection = connection;
                command.CommandText = "SELECT DISTINCT delict.delict_id, delict.street, delict.date, COUNT(person.firstname) as firstname, COUNT(person.lastname) FROM dbo.delict LEFT JOIN dbo.delict_person ON delict.delict_id = delict_person.delict_id LEFT JOIN dbo.person ON person.person_id = delict_person.person_id WHERE delict.status = 1 GROUP BY delict.delict_id, delict.street, delict.date ORDER BY delict.delict_id DESC";

                using (DbDataReader dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        int id = Convert.ToInt32(dataReader["delict_id"]);
                        int count = Convert.ToInt32(dataReader["firstname"]);
                        Delict d1 = new Delict();
                        d1.id = id;
                        d1.street = GetDelictCategory(id);

                        d1.createtime = (DateTime)dataReader["date"];

                        d1.firstnamecount = count;
                        delictenlist.Add(d1);
                    }
                }
                foreach (var item in delictenlist)
                {
                    delictenlistCheck.Add(item);
                }
                ShowDelicts();
            }
        }

        public void GetArchivedDelicts()
        {
            delictenlistCheck.Clear();
            delictenlist.Clear();
            currPageIsActivated = false;
            mw.TopHeader.Text = "Wijkagent - Delicten archief lijst";
            DelictActivateBTN.Visibility = Visibility.Visible;
            DelictArchiveBTN.Visibility = Visibility.Hidden;
            string provider = ConfigurationManager.AppSettings["provider"];
            string connectionstring = ConfigurationManager.AppSettings["connectionString"];

            DbProviderFactory factory = DbProviderFactories.GetFactory(provider);

            using (DbConnection connection = factory.CreateConnection())
            {
                connection.ConnectionString = connectionstring;
                connection.Open();
                DbCommand command = factory.CreateCommand();

                command.Connection = connection;
                command.CommandText = "SELECT d.delict_id, u.badge_nr, d.description, a.date_added FROM dbo.archive as a JOIN dbo.delict as d ON a.delict_id = d.delict_id JOIN dbo.[User] as u ON a.user_id = u.user_id WHERE d.status = 0 ORDER BY a.delict_id DESC";

                using (DbDataReader dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        int id = Convert.ToInt32(dataReader["delict_id"]);
                        Delict d1 = new Delict();
                        d1.id = id;
                        d1.street = GetDelictCategory(id);
                        d1.changedBy = Convert.ToInt32(dataReader["badge_nr"]);
                        d1.addedDate = (DateTime)dataReader["date_added"];
                        delictenlist.Add(d1);
                    }
                }
                foreach (var item in delictenlist)
                {
                    delictenlistCheck.Add(item);
                }
                ShowDelicts();
            }
        }


        public void ShowDelicts()
        {
            Delicten.Items.Clear();
            DelictCountLabel.Content = "Resultaten: " + delictenlistCheck.Count();
            int counter = pageCounter * 10;
            decimal delictCount = delictenlistCheck.Count();
            for (int i = counter - 10; i < counter; i++)
            {
                if (delictenlistCheck.ElementAtOrDefault(i) != null)
                {
                    Delicten.Items.Add(delictenlistCheck[i]);
                }
            }

            if(pageCounter == 1)
            {
                PreviousButton.IsEnabled = false;
            }
            else
            {
                PreviousButton.IsEnabled = true;
            }
            if (pageCounter >= Math.Ceiling(delictCount / delictsPerPage)){
                NextButton.IsEnabled = false;
            }
            else
            {
                NextButton.IsEnabled = true;
            }
            PageLabel.Content = "Pagina: " + pageCounter + " / " + Math.Ceiling(delictenlistCheck.Count() / delictsPerPage);
            if(delictCount == 0)
            {
                mw.ShowMessage("Geen delicten gevonden");
            }
        }

        private void NextDelictsPage(object sender, RoutedEventArgs e) //next
        {
            pageCounter++;
            ShowDelicts();
        }

        private void PreviousDelictsPage(object sender, RoutedEventArgs e) //previous
        {
            pageCounter--;
            ShowDelicts();
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

        private void ViewDelict(object sender, RoutedEventArgs e)
        {
            var DelictID = (int)((System.Windows.Controls.Button)sender).Tag;
            mw.ShowDelict(DelictID,1);
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
            if (iddelict != null)
            {
                DbProviderFactory factory = DbProviderFactories.GetFactory(provider);
                using (DbConnection connection = factory.CreateConnection())
                {
                    connection.ConnectionString = connectionstring;
                    connection.Open();
                    DbCommand command = factory.CreateCommand();
                    command.Connection = connection;
                    command.CommandText = "SELECT firstname, lastname FROM dbo.person JOIN dbo.delict_person ON person.person_id = delict_person.person_id WHERE delict_person.delict_id =" + iddelict.id;
                    personnames.Items.Clear();
                    using (DbDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            personnames.Items.Add(dataReader["firstname"] + " " + dataReader["lastname"]);
                        }
                    }
                }
            }
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Right && NextButton.IsEnabled == true)
            {
                NextDelictsPage(sender, e);
            }

            if (e.Key == Key.Left && PreviousButton.IsEnabled == true)
            {
                PreviousDelictsPage(sender, e);
            }
        }
        private void AddDelict_Click(object sender, RoutedEventArgs e)
        {
            mw.AddDelict();
        }

        // ---- Combobox ---- //
        private void BindCategroryDropDown()
        {
            categoryCB.ItemsSource = categoryList;
        }

        private void category_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!categoryCB.IsDropDownOpen)
            {
                categoryCB.IsDropDownOpen = true;
            }
            var t = categoryCB.SelectedIndex;
            categoryCB.ItemsSource = categoryList.Where(x => x.Category_Name.ToLower().StartsWith(categoryCB.Text.Trim().ToLower()));
        }

        private void AllCheckbocx_CheckedAndUnchecked(object sender, RoutedEventArgs e)
        {
            BindListBOX();
        }

        private void BindListBOX()
        {
            testListbox.Items.Clear();
            delictenlistCheck.Clear();
            foreach (var category in categoryList)
            {
                if (category.Check_Status == true)
                {
                    testListbox.Items.Add(category.Category_Name);
                    categoryCB.Text = "";
                    foreach(var item in delictenlist)
                    {
                        bool alreadyExists = delictenlistCheck.Any(x => x.id == item.id);
                        if (item.street.Contains(category.Category_Name) && !alreadyExists)
                        {
                            delictenlistCheck.Add(item);
                        }
                    }
                }
            }
            if (testListbox.Items.Count == 0)
            {
                foreach (var item in delictenlist)
                {
                    delictenlistCheck.Add(item);
                }
            }

            ShowDelicts();
        }
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            categoryCB.SelectedIndex = -1;
        }

        private void Archiveren(object sender, RoutedEventArgs e)
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
                    string addToArchive = "INSERT INTO dbo.archive (delict_id,user_id, date_added) " +
                                          "VALUES (@delictID,@userID, GETDATE())";
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
                                cmd.Parameters.Add("@userID", SqlDbType.Int).Value = mw.GetUserID();
                                cmd.ExecuteNonQuery();
                            }

                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("ERROR:" + ex.Message);
                        }
                    }
                }
                var currentRowIndex = Delicten.Items.IndexOf(Delicten.CurrentItem);
                delictenlistCheck.RemoveAt(currentRowIndex);
                mw.ShowMessage("Delict succesvol gearchiveerd.");
                ShowDelicts();
            }
            else if (dialogResult == MessageBoxResult.No)
            {
                //do something else
            }
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
                    }
                }
                var currentRowIndex = Delicten.Items.IndexOf(Delicten.CurrentItem);
                delictenlistCheck.RemoveAt(currentRowIndex);
                mw.ShowMessage("Delict succesvol geactiveerd.");
                ShowDelicts();
            }
            else if (dialogResult == MessageBoxResult.No)
            {
                //do something else
            }
        }
        public void UncheckAllCategories()
        {
            foreach (var item in categoryList)
            {
                item.Check_Status = false;
            }
            categoryCB.ItemsSource = emptylist;
            testListbox.Items.Clear();
            delictenlistCheck.Clear();
            BindCategroryDropDown();
            BindListBOX();
            ShowDelicts();
        }
        private void CustomSort(object sender, DataGridSortingEventArgs e)
        {
            e.Handled = true;
            if (e.Column.Header.ToString() == "Delict")
            {
                if (sortID)
                {
                    delictenlistCheck = delictenlistCheck.OrderBy(o => o.id).ToList();
                    sortDate = false;
                    sortID = false;
                }
                else
                {
                    delictenlistCheck = delictenlistCheck.OrderByDescending(o => o.id).ToList();
                    sortDate = false;
                    sortID = true;
                }
            }
            if(e.Column.Header.ToString() == "Aanmaakdatum")
            {
                if (sortDate)
                {
                    delictenlistCheck = delictenlistCheck.OrderBy(o => o.createtime).ToList();
                    sortID = false;
                    sortDate = false;
                }
                else
                {
                    delictenlistCheck = delictenlistCheck.OrderByDescending(o => o.createtime).ToList();
                    sortID = false;
                    sortDate = true;
                }
            }
            ShowDelicts();
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if(currPageIsActivated == true)
            {
                ActivelistCreatedtime.Visibility = Visibility.Hidden;
                ActivelistFirstnamecount.Visibility = Visibility.Hidden;
                ArchivelistAddedDate.Visibility = Visibility.Visible;
                ArchivelistChangedBy.Visibility = Visibility.Visible;
                DelictListSwapBTN.Content = "Actieve delicten lijst";
                GetArchivedDelicts();
                UncheckAllCategories();
                StartDateDP.SelectedDate = null;
                EndDateDP.SelectedDate = null;
                DateCB.IsChecked = false;

                delictenlistCheck.Clear();
                foreach (var item in delictenlist)
                {
                    delictenlistCheck.Add(item);
                }
                DateCB_Click(sender, e);
                DateCB.IsEnabled = false;
                ResetFilterBTN.IsEnabled = false;
                return;
            }
            if(currPageIsActivated == false)
            {
                ActivelistCreatedtime.Visibility = Visibility.Visible;
                ActivelistFirstnamecount.Visibility = Visibility.Visible;
                ArchivelistAddedDate.Visibility = Visibility.Hidden;
                ArchivelistChangedBy.Visibility = Visibility.Hidden;
                DelictListSwapBTN.Content = "Gearchiveerde delicten lijst";
                GetActiveDelicts();
                UncheckAllCategories();
                StartDateDP.SelectedDate = null;
                EndDateDP.SelectedDate = null;
                DateCB.IsChecked = false;

                delictenlistCheck.Clear();
                foreach (var item in delictenlist)
                {
                    delictenlistCheck.Add(item);
                }
                DateCB_Click(sender, e);
                DateCB.IsEnabled = false;
                ResetFilterBTN.IsEnabled = false;
                return;
            }
        }

        private void DateCB_Click(object sender, RoutedEventArgs e)
        {
            if (DateCB.IsChecked == true)
            {
                EndDateDP.IsEnabled = false;
                EndDateChanged(sender, e);
                if(StartDateDP.SelectedDate == null && EndDateDP.SelectedDate == null)
                {
                    ResetFilterBTN.IsEnabled = false;
                }
/*                EndDateDP.SelectedDate = null;
                StartDateDP.DisplayDateEnd = EndDateDP.SelectedDate;*/
            }
            else
            {
                EndDateDP.IsEnabled = true;
                EndDateChanged(sender, e);
                if (StartDateDP == null && EndDateDP == null)
                {
                    ResetFilterBTN.IsEnabled = false;
                }
                /*                EndDateDP.SelectedDate = null;
                                StartDateDP.DisplayDateEnd = EndDateDP.SelectedDate;*/
            }
        }

        private void StartDateChanged(object sender, RoutedEventArgs e)
        {
            DateCB.IsEnabled = true;
            ResetFilterBTN.IsEnabled = true;
            delictenlistCheck.Clear();
            foreach (var item in delictenlist)
            {
                delictenlistCheck.Add(item);
            }
            if (DateCB.IsChecked == true)
            {
                foreach (var item in delictenlist)
                {
                    if (item.createtime != StartDateDP.SelectedDate)
                    {
                        delictenlistCheck.Remove(item);
                    }
                }
            }
            else
            {
                foreach (var item in delictenlist)
                {
                    if (item.createtime < StartDateDP.SelectedDate)
                    {
                        delictenlistCheck.Remove(item);
                    }
                    if(EndDateDP.SelectedDate != null)
                    {
                        foreach (var var in delictenlist)
                        {
                            if (var.createtime > EndDateDP.SelectedDate && DateCB.IsChecked == false)
                            {
                                delictenlistCheck.Remove(var);
                            }
                        }
                    }
                }
            }
            EndDateDP.DisplayDateStart = StartDateDP.SelectedDate;
            pageCounter = 1;
            ShowDelicts();
        }

        private void EndDateChanged(object sender, RoutedEventArgs e)
        {
            ResetFilterBTN.IsEnabled = true;
            foreach (var item in delictenlist)
            {
                if (item.createtime > EndDateDP.SelectedDate && DateCB.IsChecked == false)
                {
                    delictenlistCheck.Remove(item);
                }
            }
            StartDateDP.DisplayDateEnd = EndDateDP.SelectedDate;
            StartDateChanged(sender, e);
        }

        private void RemoveFilter_Click(object sender, RoutedEventArgs e)
        {
            StartDateDP.SelectedDate = null;
            EndDateDP.SelectedDate = null;
            DateCB.IsChecked = false;

            delictenlistCheck.Clear();
            foreach (var item in delictenlist)
            {
                delictenlistCheck.Add(item);
            }
            DateCB_Click(sender, e);
            DateCB.IsEnabled = false;
            ResetFilterBTN.IsEnabled = false;
        }
    }
}