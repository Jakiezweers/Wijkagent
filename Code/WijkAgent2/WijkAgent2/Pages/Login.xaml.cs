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
        public int badgeId;
        public string passwordInput = "";
        public string usernameInput = "";
        private Boolean loginButtonClicked = false;
        private Boolean loginCheck = false;

        private MainWindow mw;
        public Login(MainWindow MW)
        {
            this.mw = MW;
            InitializeComponent();
        }

        private void LogInButton_Click(object sender, RoutedEventArgs e)
        {
            loginButtonClicked = true;
            SqlConnection connection = new SqlConnection("Data Source=141.138.137.63;Initial Catalog=Wijkagent;Persist Security Info=True;User ID=SA;Password=Student123!;");
            string query = "SELECT * FROM [Wijkagent].[dbo].[user] WHERE user_id = '" + UsernameTextBox.Text.Trim() + "' AND password ='" + PasswordTextBox.Text.Trim() + "'";
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(query, connection);
            DataTable dataTable = new DataTable();
            sqlDataAdapter.Fill(dataTable);

            Console.WriteLine("Click!");

            if (dataTable.Rows.Count == 1) //boolean die checkt voor de correcte Login
            {
                mw.LoadHomeScreen();
            }
            else
            {
                //Een message op het scherm dat verteld dat de login incorrect was//
                Console.WriteLine("");
                mw.close();
            }
        }


        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {

        }

    }
}
