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
            SqlConnection connection = new SqlConnection("Data Source=141.138.137.63;Initial Catalog=Wijkagent;Persist Security Info=True;User ID=SA;Password=Student123!;");
            string query = "SELECT * FROM [Wijkagent].[dbo].[user] WHERE user_id = '" + badgeId + "' AND password ='" + PasswordTextBox.Password.Trim() + "'";
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(query, connection);
            DataTable dataTable = new DataTable();
            sqlDataAdapter.Fill(dataTable);

            if (dataTable.Rows.Count == 1) //boolean die de lijst ophaalt van overeenkomende users met de ingevoerde user_id en password
            {
                mw.LoadHomeScreen();
            }
            else
            {
                //Een message op het scherm dat verteld dat de login incorrect was//
                Console.WriteLine("Fout Badge ID of wachtwoord!");
                FoutLoginLabel.Visibility = Visibility.Visible;

            }
            sqlDataAdapter.Dispose();
        }

        private void UsernameTextBox_Changed(object sender, TextChangedEventArgs e)
        {

        }

        private void PasswordBox_Changed(object sender, RoutedEventArgs e)
        {

        }

        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LogInButton_Click(this, new RoutedEventArgs());
            }
        }

        private void UsernameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LogInButton_Click(this, new RoutedEventArgs());
            }
        }
    }
}
