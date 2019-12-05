using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using WijkAgent2.Classes;
using WijkAgent2.Database;
using WijkAgent2.Modals;

namespace WijkAgent2.Pages.delicten
{
    /// <summary>
    /// Interaction logic for add_delict.xaml
    /// </summary>
    public partial class add_delict : Page
    {
        List<CategoryList> categoryList = new List<CategoryList>();
        List<int> personsbsn = new List<int>();
        List<string> personstype = new List<string>();
        List<int> person_id = new List<int>();

        private Connection cn = new Connection();
        int i = 0;
        private MainWindow mw;
        public add_delict(MainWindow MW)
        {
            this.mw = MW;
            InitializeComponent();
            categoryList = new List<CategoryList>();
            BindCategroryDropDown();
            DatumTB.SelectedDate = DateTime.Today;
            personentoevoegen addperson = new personentoevoegen(mw);
            AddPersonButton.Click += (sender, EventArgs) => { AddPerson_Click(sender, EventArgs, addperson); };

            cn.OpenConection();
            SqlDataReader sq = cn.DataReader("Select * from dbo.category");
            while (sq.Read())
            {
                CategoryList obj = new CategoryList((int)sq["category_id"], (string)(sq["name"]));
                categoryList.Add(obj);
            }
            cn.CloseConnection();
        }

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
            foreach (var category in categoryList)
            {
                if (category.Check_Status == true)
                {
                    testListbox.Items.Add(category.Category_Name);
                    categoryCB.Text = "";
                }
            }
        }

        private void SaveDelict_Click(object sender, RoutedEventArgs e)
        {
            string errorMessage = "De volgende velden zijn niet correct ingevoerd: ";
            bool errorBool = false;
            string placeName = PlaatsTB.Text;
            string zipCode = Regex.Replace(PostcodeTB.Text, @" ", "");
            string homeNumber = HuisnummerTB.Text;
            string street = StraatTB.Text;
            string description = OmschijvingTB.Text;
            string date = DatumTB.Text;
            double longCoord = 0.0000;
            double latCoord = 0.0000;

            StringBuilder homeNumbernum =
                  new StringBuilder();
            StringBuilder homeNumberLet =
                     new StringBuilder();

            for (int i = 0; i < homeNumber.Length; i++)
            {
                if (Char.IsDigit(homeNumber[i]))
                    homeNumbernum.Append(homeNumber[i]);
                else if ((homeNumber[i] >= 'A' &&
                         homeNumber[i] <= 'Z') ||
                         (homeNumber[i] >= 'a' &&
                          homeNumber[i] <= 'z'))
                    homeNumberLet.Append(homeNumber[i]);
            }

            StringBuilder zipCodeNum = new StringBuilder();
            StringBuilder zipCodeLet = new StringBuilder();

            for (int i = 0; i < zipCode.Length; i++)
            {
                if (Char.IsDigit(zipCode[i]))
                    zipCodeNum.Append(zipCode[i]);
                else if ((zipCode[i] >= 'A' &&
                         zipCode[i] <= 'Z') ||
                         (zipCode[i] >= 'a' &&
                          zipCode[i] <= 'z'))
                    zipCodeLet.Append(zipCode[i]);
            }

            if (!CheckCategorie())
            {
                errorMessage += "Categorie, ";
                errorBool = true;
            }
            if (description == "")
            {
                errorMessage += "Beschrijving, ";
                errorBool = true;
            }
            if (placeName == "")
            {
                errorMessage += "Plaats, ";
                errorBool = true;
            }
            if (zipCode == "" || zipCode.Length != 6 || zipCodeLet.Length != 2 || zipCodeNum.Length != 4)
            {
                errorMessage += "Postcode, ";
                errorBool = true;
            }
            if (homeNumber.Length > 0)
            {
                errorMessage += "Huisnummer, ";
                errorBool = true;
            }
            if (street == "")
            {
                errorMessage += "Straat, ";
                errorBool = true;
            }

            int homeNumberNumber = int.Parse(homeNumbernum.ToString());
            string homeNumberLetters = homeNumberLet.ToString().ToUpper();

            if (errorBool) //Hieronder alles wat gedaan moet worden als er iets fout gaat.
            {
                string errorBoxText = errorMessage.Substring(0, errorMessage.Length - 2);
                string errorCaption = "Delict toevoegen mislukt.";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBox.Show(errorBoxText, errorCaption, button);
            }
            else //Hieronder alles wat uitgevoerd moet worden als alles goed is. 
            {
                SendDelictToDatabase(date, mw.FirstCharToUpper(placeName), homeNumberNumber, homeNumberLetters, zipCode.ToUpper(), mw.FirstCharToUpper(street), description, longCoord, latCoord);
                mw.ShowMessage("Delict toegevoegd");
            }
        }

        private void GetLat()
        {

        }
        private void GetLong()
        {

        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            categoryCB.SelectedIndex = -1;
        }

        private void SendDelictToDatabase(string date, string placeName, int homeNumberNumber, string homeNumberLetters, string zipCode, string street, string description, double longCoord, double latCoord)
        {
            cn.OpenConection();

            string sqlDelictInsert = "insert into dbo.delict (date, place, housenumber,housenumberAddition, zipcode, street, description, long, lat, status, added_date) OUTPUT INSERTED.delict_id values(@first,@second,@third,@thirdAddition,@fourth,@fifth,@sixth,@seventh,@eight,@ninth,GETDATE())";
            string sqlPersonInsert = "insert into dbo.delict_person (delict_id, person_id, type) values (@delictID, @person_id, @type)";
            string sqlCategoryInsert = "insert into dbo.category_delict (delict_id, category_id) values (@delictID,@categoryID)";

            int id = 0;

            using (SqlCommand cmd = new SqlCommand(sqlDelictInsert))
            {
                cmd.Connection = cn.GetConnection();
                cmd.Parameters.Add("@first", SqlDbType.DateTime).Value = date;
                cmd.Parameters.Add("@second", SqlDbType.NVarChar).Value = placeName;
                cmd.Parameters.Add("@third", SqlDbType.Int).Value = homeNumberNumber;
                cmd.Parameters.Add("@thirdAddition", SqlDbType.NVarChar).Value = homeNumberLetters;
                cmd.Parameters.Add("@fourth", SqlDbType.NVarChar).Value = zipCode;
                cmd.Parameters.Add("@fifth", SqlDbType.NVarChar).Value = street;
                cmd.Parameters.Add("@sixth", SqlDbType.NVarChar).Value = description;
                cmd.Parameters.Add("@seventh", SqlDbType.NVarChar).Value = longCoord;
                cmd.Parameters.Add("@eight", SqlDbType.NVarChar).Value = latCoord;
                cmd.Parameters.Add("@ninth", SqlDbType.NVarChar).Value = 1;

                id = (int)cmd.ExecuteScalar();
            }

            if (personsbsn != null)
            {
                //insert personen in database
                foreach (var item in person_id)
                {
                    using (SqlCommand cmd = new SqlCommand(sqlPersonInsert))
                    {
                        cmd.Connection = cn.GetConnection();
                        cmd.Parameters.Add("@delictID", SqlDbType.NVarChar).Value = id;
                        cmd.Parameters.Add("@person_id", SqlDbType.NVarChar).Value = person_id[i];
                        cmd.Parameters.Add("@type", SqlDbType.NVarChar).Value = personstype[i];
                        cmd.ExecuteNonQuery();
                        i++;
                    }

                }
            }

            //Insert delict met gekoppelde categorieen in de database.
            foreach (var item in categoryList)
            {
                if (item.Check_Status == true)
                {
                    using (SqlCommand cmd = new SqlCommand(sqlCategoryInsert))
                    {
                        cmd.Connection = cn.GetConnection();
                        cmd.Parameters.Add("@delictID", SqlDbType.NVarChar).Value = id;
                        cmd.Parameters.Add("@categoryID", SqlDbType.NVarChar).Value = item.Category_ID;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            mw.ShowDelictenList();
        }

        private bool CheckCategorie()
        {
            foreach (var item in categoryList)
            {
                if (item.Check_Status)
                {
                    return true;
                }
            }
            return false;
        }

        private void CancelDelict_Click(object sender, RoutedEventArgs e)
        {
            mw.ShowDelictenList();
        }
        private void AddPerson_Click(object sender, RoutedEventArgs e, personentoevoegen addperson)
        {
            addperson.ShowDialog();
            personsbsn = addperson.bsnlist;
            personstype = addperson.typelist;
            person_id = addperson.person_idList;
        }
    }
}
