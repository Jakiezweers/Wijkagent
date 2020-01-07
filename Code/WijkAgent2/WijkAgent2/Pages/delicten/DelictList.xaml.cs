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
    public partial class DelictList : Page
    {
        readonly MainWindow mw; //Required mainwindow

        readonly List<Delict> delictenlist = new List<Delict>(); //List containing all delicts.
        List<Delict> sortedDelictList = new List<Delict>(); //List containing the sorted delicts. <- This list gets shown in the table.

        bool currPageIsActiveDelicts = true; //If bool is true, it gets only active delicts. If false it gets archived delicts.

        bool sortID = false; //Bool if list is sorted on ID.
        bool sortDate = false; //Bool if list is sorted on Date.

        readonly List<CategoryList> categoryList = new List<CategoryList>(); //List containing all categories on wich the delicts can be sorted.

        List<String> emptylist = new List<String>(); //List to selected category.

        static int pageCounter = 1; //Number of the current page ur on.
        readonly decimal delictsPerPage = 10; //Amount of delicts one page is allowed to show.


        private readonly Connection cn = new Connection();

        //Constructor retrieves all categories for filtering, Also retrieves the correct list based on currPageIsActiveDelicts state.
        public DelictList(MainWindow MW, bool activeDelicts)
        {

            Validator validator = new Validator();
            mw = MW;
            InitializeComponent();
            currPageIsActiveDelicts = activeDelicts;

            cn.OpenConection();
            SqlDataReader sq = cn.DataReader("Select * from dbo.category");
            while (sq.Read())
            {
                CategoryList obj = new CategoryList((int)sq["category_id"], (string)(sq["name"]));
                categoryList.Add(obj);
            }
            cn.CloseConnection();
            BindCategroryDropDown();
            if (currPageIsActiveDelicts)
            {
                DelictListSwapBTN.Content = "Gearchiveerde delicten lijst";
                GetActiveDelicts();
            }
            else
            {
                DelictListSwapBTN.Content = "Actieve delicten lijst";
                GetArchivedDelicts();
            }

            int user_id = mw.GetUserID();
            validator.logged_in_user_id = user_id;
            if (activeDelicts)
            {
                DelictActivateBTN.Visibility = Visibility.Hidden;
                if (validator.validate("Delicten_Archiveren")) { DelictArchiveBTN.Visibility = Visibility.Visible; } else { DelictArchiveBTN.Visibility = Visibility.Hidden; }
            }
            else
            {
                DelictArchiveBTN.Visibility = Visibility.Hidden;
                if (validator.validate("Delicten_Activeren")) { DelictActivateBTN.Visibility = Visibility.Visible; } else { DelictActivateBTN.Visibility = Visibility.Hidden; }
            }
            cn.CloseConnection();
        }

        //Method to retrieve active delicts.
        public void GetActiveDelicts()
        {
            sortedDelictList.Clear();
            delictenlist.Clear();
            currPageIsActiveDelicts = true;
            mw.TopHeader.Text = "Wijkagent - Delicten lijst";
            DelictArchiveBTN.Visibility = Visibility.Visible;
            DelictActivateBTN.Visibility = Visibility.Hidden;

            cn.OpenConection();
            SqlDataReader sq = cn.DataReader("SELECT DISTINCT delict.delict_id, delict.street, delict.date, COUNT(person.firstname) as firstname, COUNT(person.lastname) FROM dbo.delict LEFT JOIN dbo.delict_person ON delict.delict_id = delict_person.delict_id LEFT JOIN dbo.person ON person.person_id = delict_person.person_id WHERE delict.status = 1 GROUP BY delict.delict_id, delict.street, delict.date ORDER BY delict.delict_id DESC");
            while (sq.Read())
            {
                int id = Convert.ToInt32(sq["delict_id"]);
                int count = Convert.ToInt32(sq["firstname"]);
                Delict d1 = new Delict();
                d1.id = id;
                d1.street = GetDelictCategory(id);

                d1.createtime = (DateTime)sq["date"];

                d1.firstnamecount = count;
                delictenlist.Add(d1);
            }
            cn.CloseConnection();

            foreach (var item in delictenlist)
            {
                sortedDelictList.Add(item);
            }
            ShowDelicts();
        }

        //Method to retrieve archived delicts.
        public void GetArchivedDelicts()
        {
            sortedDelictList.Clear();
            delictenlist.Clear();
            currPageIsActiveDelicts = false;
            mw.TopHeader.Text = "Wijkagent - Delicten archief lijst";
            DelictActivateBTN.Visibility = Visibility.Visible;
            DelictArchiveBTN.Visibility = Visibility.Hidden;

            cn.OpenConection();
            SqlDataReader sq = cn.DataReader("SELECT d.delict_id, u.badge_nr, d.description, a.date_added FROM dbo.archive as a JOIN dbo.delict as d ON a.delict_id = d.delict_id JOIN dbo.[User] as u ON a.user_id = u.user_id WHERE d.status = 0 ORDER BY a.delict_id DESC");
            while (sq.Read())
            {
                int id = Convert.ToInt32(sq["delict_id"]);
                Delict d1 = new Delict();
                d1.id = id;
                d1.street = GetDelictCategory(id);
                d1.changedBy = Convert.ToInt32(sq["badge_nr"]);
                d1.addedDate = (DateTime)sq["date_added"];
                delictenlist.Add(d1);
            }
            cn.CloseConnection();

            foreach (var item in delictenlist)
            {
                sortedDelictList.Add(item);
            }
            ShowDelicts();
        }

        //Method to show delicts that are retrieved in the table.
        public void ShowDelicts()
        {
            Delicten.Items.Clear();
            DelictCountLabel.Content = "Resultaten: " + sortedDelictList.Count();
            int counter = pageCounter * 10;
            decimal delictCount = sortedDelictList.Count();
            for (int i = counter - 10; i < counter; i++)
            {
                if (sortedDelictList.ElementAtOrDefault(i) != null)
                {
                    Delicten.Items.Add(sortedDelictList[i]);
                }
            }

            if (pageCounter == 1)
            {
                PreviousButton.IsEnabled = false;
            }
            else
            {
                PreviousButton.IsEnabled = true;
            }
            if (pageCounter >= Math.Ceiling(delictCount / delictsPerPage))
            {
                NextButton.IsEnabled = false;
            }
            else
            {
                NextButton.IsEnabled = true;
            }
            PageLabel.Content = "Pagina: " + pageCounter + " / " + Math.Ceiling(sortedDelictList.Count() / delictsPerPage);
            if (delictCount == 0)
            {
                mw.ShowMessage("Geen delicten gevonden");
            }
        }

        //Method to view the next 10 (Depends on amount of delicts per page) delicts if possible.
        private void NextDelictsPage(object sender, RoutedEventArgs e) //next
        {
            pageCounter++;
            ShowDelicts();
        }

        //Method to view the previous 10 (Depends on amount of delicts per page) delicts if possible.
        private void PreviousDelictsPage(object sender, RoutedEventArgs e) //previous
        {
            pageCounter--;
            ShowDelicts();
        }

        //Method to get categories connected to delicts.
        private string GetDelictCategory(int delictID)
        {
            string returnString = "";
            cn.OpenConection();
            SqlDataReader sq = cn.DataReader("SELECT name FROM category_delict JOIN category ON category.category_id = category_delict.category_id WHERE delict_id = " + delictID);
            while (sq.Read())
            {
                returnString += sq["name"];
                returnString += ", ";
            }
            cn.CloseConnection();

            if (returnString.Length > 2)
            {
                return returnString.Substring(0, returnString.Length - 2);
            }
            return returnString;
        }

        //Method to view a certain delict.
        private void ViewDelict(object sender, RoutedEventArgs e)
        {
            var DelictID = (int)((System.Windows.Controls.Button)sender).Tag;
            mw.ShowDelict(DelictID, 1);
        }

        //Method to go back to homescreen.
        private void Back_Button(object sender, RoutedEventArgs e)
        {
            mw.LoadHomeScreen();
        }

        //Method to retrieve persons connected to the selected delict.
        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var iddelict = Delicten.SelectedItem as Delict;
            if (iddelict != null)
            {
                cn.OpenConection();
                SqlDataReader sq = cn.DataReader("SELECT firstname, lastname FROM dbo.person JOIN dbo.delict_person ON person.person_id = delict_person.person_id WHERE delict_person.delict_id =" + iddelict.id);
                personnames.Items.Clear();
                while (sq.Read())
                {
                    personnames.Items.Add(sq["firstname"] + " " + sq["lastname"]);
                }
                cn.CloseConnection();
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

        //Method to open the AddDelict screen.
        private void AddDelict_Click(object sender, RoutedEventArgs e)
        {
            mw.AddDelict();
        }

        //Method to archive archived delicts. Button is shown in the "Actie" column in the table.
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
                sortedDelictList.RemoveAt(currentRowIndex);
                mw.ShowMessage("Delict succesvol gearchiveerd.");
                ShowDelicts();
            }
            else if (dialogResult == MessageBoxResult.No)
            {
                //do something else
            }
        }

        //Method to activate archived delicts. Button is shown in the "Actie" column in the table.
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
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("ERROR:" + ex.Message);
                        }
                    }
                }
                var currentRowIndex = Delicten.Items.IndexOf(Delicten.CurrentItem);
                sortedDelictList.RemoveAt(currentRowIndex);
                mw.ShowMessage("Delict succesvol geactiveerd.");
                ShowDelicts();
            }
            else if (dialogResult == MessageBoxResult.No)
            {
                //do something else
            }
        }

        //Method to uncheck all categories previously checked for filtering. (Gets fired when swapping to and from archived and active delict list)
        public void UncheckAllCategories()
        {
            foreach (var item in categoryList)
            {
                item.Check_Status = false;
            }
            categoryCB.ItemsSource = emptylist;
            categorieListBox.Items.Clear();
            sortedDelictList.Clear();
            BindCategroryDropDown();
            BindListBOX();
            ShowDelicts();
        }

        //Method for custom sorting the table wich the delicts are placed in.
        private void CustomSort(object sender, DataGridSortingEventArgs e)
        {
            e.Handled = true;
            if (e.Column.Header.ToString() == "Delict")
            {
                if (sortID)
                {
                    sortedDelictList = sortedDelictList.OrderBy(o => o.id).ToList();
                    sortDate = false;
                    sortID = false;
                }
                else
                {
                    sortedDelictList = sortedDelictList.OrderByDescending(o => o.id).ToList();
                    sortDate = false;
                    sortID = true;
                }
            }
            if (e.Column.Header.ToString() == "Aanmaakdatum")
            {
                if (sortDate)
                {
                    sortedDelictList = sortedDelictList.OrderBy(o => o.createtime).ToList();
                    sortID = false;
                    sortDate = false;
                }
                else
                {
                    sortedDelictList = sortedDelictList.OrderByDescending(o => o.createtime).ToList();
                    sortID = false;
                    sortDate = true;
                }
            }
            ShowDelicts();
        }

        //Method to swap between active and archived delicts.
        private void SwapDelictList(object sender, RoutedEventArgs e)
        {
            if (currPageIsActiveDelicts == true)
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

                sortedDelictList.Clear();
                foreach (var item in delictenlist)
                {
                    sortedDelictList.Add(item);
                }
                DateCB_Click(sender, e);
                DateCB.IsEnabled = false;
                ResetFilterBTN.IsEnabled = false;
                return;
            }
            if (currPageIsActiveDelicts == false)
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

                sortedDelictList.Clear();
                foreach (var item in delictenlist)
                {
                    sortedDelictList.Add(item);
                }
                DateCB_Click(sender, e);
                DateCB.IsEnabled = false;
                ResetFilterBTN.IsEnabled = false;
                return;
            }
        }

        //Method to only filter on the first selected date.
        private void DateCB_Click(object sender, RoutedEventArgs e)
        {
            if (DateCB.IsChecked == true)
            {
                EndDateDP.IsEnabled = false;
                EndDateChanged(sender, e);
                if (StartDateDP.SelectedDate == null && EndDateDP.SelectedDate == null)
                {
                    ResetFilterBTN.IsEnabled = false;
                }
            }
            else
            {
                EndDateDP.IsEnabled = true;
                EndDateChanged(sender, e);
                if (StartDateDP == null && EndDateDP == null)
                {
                    ResetFilterBTN.IsEnabled = false;
                }
            }
        }

        //Method to make sure the user cant selected certain dates the user shouldn't be able to select.
        // EXAMPLE: Start date : 1-1-2020 with End date : 12-12-2019   <-- Cant happen
        private void StartDateChanged(object sender, RoutedEventArgs e)
        {
            DateCB.IsEnabled = true;
            ResetFilterBTN.IsEnabled = true;
            sortedDelictList.Clear();
            foreach (var item in delictenlist)
            {
                sortedDelictList.Add(item);
            }
            if (DateCB.IsChecked == true)
            {
                foreach (var item in delictenlist)
                {
                    if (item.createtime != StartDateDP.SelectedDate)
                    {
                        sortedDelictList.Remove(item);
                    }
                }
            }
            else
            {
                foreach (var item in delictenlist)
                {
                    if (item.createtime < StartDateDP.SelectedDate)
                    {
                        sortedDelictList.Remove(item);
                    }
                    if (EndDateDP.SelectedDate != null)
                    {
                        foreach (var var in delictenlist)
                        {
                            if (var.createtime > EndDateDP.SelectedDate && DateCB.IsChecked == false)
                            {
                                sortedDelictList.Remove(var);
                            }
                        }
                    }
                }
            }
            EndDateDP.DisplayDateStart = StartDateDP.SelectedDate;
            pageCounter = 1;
            ShowDelicts();
        }

        //Method to make sure the user cant selected certain dates the user shouldn't be able to select.
        // EXAMPLE: Start date : 1-1-2020 with End date : 12-12-2019   <-- Cant happen
        private void EndDateChanged(object sender, RoutedEventArgs e)
        {
            ResetFilterBTN.IsEnabled = true;
            foreach (var item in delictenlist)
            {
                if (item.createtime > EndDateDP.SelectedDate && DateCB.IsChecked == false)
                {
                    sortedDelictList.Remove(item);
                }
            }
            StartDateDP.DisplayDateEnd = EndDateDP.SelectedDate;
            StartDateChanged(sender, e);
        }

        //Method to reset filters and show every delict in the list again.
        private void RemoveFilter_Click(object sender, RoutedEventArgs e)
        {
            StartDateDP.SelectedDate = null;
            EndDateDP.SelectedDate = null;
            DateCB.IsChecked = false;

            sortedDelictList.Clear();
            foreach (var item in delictenlist)
            {
                sortedDelictList.Add(item);
            }
            DateCB_Click(sender, e);
            DateCB.IsEnabled = false;
            ResetFilterBTN.IsEnabled = false;
        }

        // ---- Combobox ---- //

        //Method that gives the dropdown the right list containing all the categories.
        private void BindCategroryDropDown()
        {
            categoryCB.ItemsSource = categoryList;
        }

        //Method that makes the user able to search for categories.
        private void category_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!categoryCB.IsDropDownOpen)
            {
                categoryCB.IsDropDownOpen = true;
            }
            var t = categoryCB.SelectedIndex;
            categoryCB.ItemsSource = categoryList.Where(x => x.Category_Name.ToLower().StartsWith(categoryCB.Text.Trim().ToLower()));
        }

        //Method that fires whenever a categories gets checked or unchecked
        private void AllCheckbocx_CheckedAndUnchecked(object sender, RoutedEventArgs e)
        {
            BindListBOX();
        }

        //Shows selected categories in a list box and filters the table
        private void BindListBOX()
        {
            categorieListBox.Items.Clear();
            sortedDelictList.Clear();
            foreach (var category in categoryList)
            {
                if (category.Check_Status == true)
                {
                    categorieListBox.Items.Add(category.Category_Name);
                    categoryCB.Text = "";
                    foreach (var item in delictenlist)
                    {
                        bool alreadyExists = sortedDelictList.Any(x => x.id == item.id);
                        if (item.street.Contains(category.Category_Name) && !alreadyExists)
                        {
                            sortedDelictList.Add(item);
                        }
                    }
                }
            }
            if (categorieListBox.Items.Count == 0)
            {
                foreach (var item in delictenlist)
                {
                    sortedDelictList.Add(item);
                }
            }

            ShowDelicts();
        }

        //Method to fix a known combobox bug in WPF.
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            categoryCB.SelectedIndex = -1;
        }
    }
}