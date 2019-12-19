using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            int rolid;
            string rolname;
            mw = MW;
            InitializeComponent();
            
            Connection cn = new Connection();
            cn.OpenConection();
            SqlDataReader sq = cn.DataReader("select us.*, up.upload_path, r.rol_name " +
                "from[dbo].[User] us " +
                "join[dbo].[uploads] up on us.upload_id = up.upload_id " + 
                "join[dbo].[rol] r on us.rol_id = r.rol_id");

            while (sq.Read())
            {
                rolid = (int)sq["rol_id"];
                rolname = (string)sq["rol_name"];
                int id = Convert.ToInt32(sq["user_id"]);
                Wijkagent2.Classes.User user = new Wijkagent2.Classes.User();

                Console.WriteLine(rolname + rolid);

                user.UserId = id;
                user.Role = new Roles(rolname, rolid);
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            mw.LoadHomeScreen();
        }
    }
}
