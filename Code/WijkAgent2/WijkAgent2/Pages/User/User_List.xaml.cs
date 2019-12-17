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
using Wijkagent2.Classes;
using WijkAgent2.Database;

namespace WijkAgent2.Pages.User
{
    /// <summary>
    /// Interaction logic for User_List.xaml
    /// </summary>
    public partial class User_List : Page
    {
        private MainWindow mw;
        public User_List(MainWindow MW)
        {
            mw = MW;
            InitializeComponent();

            Connection cn = new Connection();
            cn.OpenConection();
            SqlDataReader sq = cn.DataReader("select us.*, up.upload_path " +
                "from[dbo].[User] us " +
                "join[dbo].[uploads] up on us.upload_id = up.upload_id");

            while (sq.Read())
            {
                int id = Convert.ToInt32(sq["user_id"]);
                Wijkagent2.Classes.User user = new Wijkagent2.Classes.User();
                user.UserId = id;
                user.Name = (string)sq["name"];
                user.BadgeId = Convert.ToInt32(sq["badge_nr"]);
                user.PhoneNumber = (string)sq["tel"];
                user.ProfilePicture = new Uploads(Convert.ToInt32(sq["upload_id"]), (string)sq["upload_path"]);
                UserList.Items.Add(user);
            }
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void BtnAddUser_Click(object sender, RoutedEventArgs e)
        {
            mw.AddUser();
        }
    }
}
