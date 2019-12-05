using System;
using System.Collections.Generic;
using System.Data;
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

namespace WijkAgent2.Pages
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Page
    {
        private MainWindow mw;
        public Login(MainWindow MW)
        {
            this.mw = MW;
            InitializeComponent();
        }


        private void LogInButton_Click(object sender, RoutedEventArgs e)
        {
            int badgeId = 0;

            try
            {
                badgeId = Int32.Parse(UsernameTextBox.Text.Trim());
            }
            catch (FormatException)
            {
                Console.WriteLine("String ingevoerd in plaats van badge ID");
                FoutLoginLabel.Visibility = Visibility.Visible;
            }

            //Hier wordt de connectie met de database gemaakt waar hij kijkt voor users die overeenkomen met de ingevoerde gegevens.
            Connection cn = new Connection();
            cn.OpenConection();
            SqlDataReader sq = cn.DataReader("SELECT * FROM [dbo].[user] WHERE badge_nr = '" + badgeId + "'");
            string password_db = "";
            int user_id = -1;
            string name = "";
            while (sq.Read())
            {
                password_db = (string)sq["password"];
                name = (string)sq["name"];
                user_id = Convert.ToInt32(sq["user_id"]);
            }

            if (!password_db.Equals("") && user_id != -1 && !name.Equals(""))
            {
                if (PasswordHandler.Validate(PasswordTextBox.Password.ToString(), password_db)) //boolean die de lijst ophaalt van overeenkomende users met de ingevoerde user_id en password
                {
                    mw.set_loggedin_user_id(user_id);
                    mw.ShowMessage("Welcome " + name);
                    mw.LoadHomeScreen();
                }
                else
                {
                    //Een message op het scherm dat verteld dat de login incorrect was//
                    Console.WriteLine("Fout Badge ID of wachtwoord!");
                    FoutLoginLabel.Visibility = Visibility.Visible;
                }
            }
            else
            {
                //Een message op het scherm dat verteld dat de login incorrect was//
                Console.WriteLine("Fout Badge ID of wachtwoord!");
                FoutLoginLabel.Visibility = Visibility.Visible;
            }
        }

        private void UsernameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LogInButton_Click(this, new RoutedEventArgs());
            }
        }

        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LogInButton_Click(this, new RoutedEventArgs());
            }
        }       
    }
}
