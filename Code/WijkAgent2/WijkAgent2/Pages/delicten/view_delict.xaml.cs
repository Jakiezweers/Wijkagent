using System;
using System.Collections.Generic;
using System.Configuration;
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
using WijkAgent2.Classes;
using WijkAgent2.Database;

namespace WijkAgent2.Pages.delicten
{
    /// <summary>
    /// Interaction logic for view_delict.xaml
    /// </summary>
    public partial class view_delict : Page
    {
        MainWindow mw;
        int viewDelictID;
        private Connection cn = new Connection();
        int returnPage = 0;
        public view_delict(MainWindow MW, int delictID, int originalPage)
        {
            Validator validator = new Validator();

            viewDelictID = delictID;
            InitializeComponent();
            LoadDelict(delictID);
            mw = MW;
            returnPage = originalPage;

            int user_id = mw.GetUserID();
            validator.logged_in_user_id = user_id;
            if (validator.validate("Delicten_Wijzigen")) { BTNPasDelictAan.Visibility = Visibility.Visible; } else { BTNPasDelictAan.Visibility = Visibility.Hidden; }
        }

        private void LoadDelict(int viewDelictID)
        {
            cn.OpenConection();
            SqlDataReader sqc = cn.DataReader("SELECT * FROM dbo.delict WHERE delict_id = " + viewDelictID);
            while (sqc.Read())
            {
                string status;
                if ((int)sqc["status"] == 1)
                {
                    status = "Actief";
                }
                else
                {
                    status = "Inactief";
                }

                DelictPlaceLabel.Content += ": " + sqc["place"];
                DelictIDLabel.Content += ": " + sqc["delict_id"];
                DelictStreetLabel.Content += ": " + sqc["street"];
                DelictHouseNumberLabel.Content += ": " + sqc["housenumber"] + " " + sqc["housenumberAddition"];
                DelictZipcodeLabel.Content += ": " + sqc["zipcode"];
                DelictStatusLabel.Content += ": " + status;
                DelictDescriptionTB.Text = (string)sqc["description"];
                DelictDateLabel.Content += ": " + sqc["added_date"];
            }

            cn.CloseConnection();
            cn.OpenConection();

            SqlDataReader sqcd = cn.DataReader("SELECT category.name FROM category_delict JOIN category ON category.category_id = category_delict.category_id WHERE delict_id = " + viewDelictID);

            while (sqcd.Read())
            {
                CategoryListbox.Items.Add(sqcd["name"]);
            }

            cn.CloseConnection();
            cn.OpenConection();

            SqlDataReader sqp = cn.DataReader("SELECT p.bsn, dp.type FROM delict_person dp JOIN person p on dp.person_id = p.person_id WHERE delict_id = " + viewDelictID);

            while (sqp.Read())
            {
                string text = sqp["type"] + " - " + sqp["bsn"];
                PersonenListbox.Items.Add(text);
            }
            cn.CloseConnection();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(returnPage == 1)
            {
                mw.ShowDelictenList(true);
                return;
            }
            if (returnPage == 2)
            {
                mw.ShowDelictenArchive();
                return;
            }
            if (returnPage == 3)
            {
                mw.LoadHomeScreen();
                return;
            }
            mw.LoadHomeScreen();
        }
        private void EditDelict_Click(object sender, RoutedEventArgs e)
        {
            mw.EditDelict(viewDelictID,returnPage);
        }
    }
}
