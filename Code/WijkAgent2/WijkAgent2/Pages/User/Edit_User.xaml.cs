using System;
using System.Collections.Generic;
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
    /// Interaction logic for Edit_User.xaml
    /// </summary>
    public partial class Edit_User : Page
    {
        int UserId;
        MainWindow Mw;
        List<string> RolesList = new List<string>();
        public Edit_User(MainWindow MW, int userId)
        {
            UserId = userId;
            int rolid;
            string rolname;
            Mw = MW;
            InitializeComponent();

            Connection cn = new Connection();
            cn.OpenConection();
            SqlDataReader sq = cn.DataReader("select us.*, up.upload_path, r.rol_name from[dbo].[User] us join[dbo].[uploads] up on us.upload_id = up.upload_id join[dbo].[rol] r on us.rol_id = r.rol_id where us.user_id = " + UserId);

            Wijkagent2.Classes.User user = new Wijkagent2.Classes.User();

            while (sq.Read())
            {
                rolid = (int)sq["rol_id"];
                rolname = (string)sq["rol_name"];
                int id = Convert.ToInt32(sq["user_id"]);

                user.UserId = id;
                user.Role = new Roles(rolname, rolid);
                user.Name = (string)sq["name"];
                user.BadgeId = Convert.ToInt32(sq["badge_nr"]);
                user.PhoneNumber = (string)sq["tel"];
                user.ProfilePicture = new Uploads(Convert.ToInt32(sq["upload_id"]), (string)sq["upload_path"]);

            }

            sq = cn.DataReader("select * from [dbo].[rol]");

            while (sq.Read()) 
            {
                RolesList.Add((string)sq["rol_name"]);
            }

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(user.ProfilePicture.Path);
            bitmap.EndInit();

            UserImage.Source = bitmap;
            UserNameTB.Text += user.Name;
            UserIDTB.Text += user.UserId;
            BadgeIdTB.Text += user.BadgeId;
            PhoneNumberTB.Text += user.PhoneNumber;
            RoleCB.ItemsSource = RolesList;
            RoleCB.SelectedItem = user.Role.RoleName;
        }

        private void Terug_Click(object sender, RoutedEventArgs e)
        {
            Mw.EditUser(UserId);
        }
    }
}
