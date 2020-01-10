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
using WijkAgent2.Classes;
using WijkAgent2.Database;

namespace WijkAgent2.Pages.User
{
    /// <summary>
    /// Interaction logic for UserView.xaml
    /// </summary>
    public partial class UserView : Page
    {
        MainWindow Mw;
        int UserID;
        string FunctionName = null;
        int functionid = 69;
        int KazerneID = 0;
        int EenheidID = 0;

        public UserView(MainWindow MW, int userID)
        {
            UserID = userID;
            int rolid;
            string rolname;
            Mw = MW;
            InitializeComponent();

            Connection cn = new Connection();
            cn.OpenConection();
            SqlDataReader sq = cn.DataReader("select us.*, up.upload_path, r.rol_name from[dbo].[User] us join[dbo].[uploads] up on us.upload_id = up.upload_id join[dbo].[rol] r on us.rol_id = r.rol_id where us.user_id = " + UserID);

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
                functionid = (int)sq["functie_id"];
                KazerneID = (int)sq["kazerne_id"];
                EenheidID = (int)sq["eenheid_id"];

            }
            cn.CloseConnection();
            cn.OpenConection();
            SqlDataReader sq1 = cn.DataReader("Select * from [dbo].[Functie] where functie_id = " + functionid);
            while (sq1.Read())
            {
                FunctionName = (string)sq1["name"];
            }

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(user.ProfilePicture.Path);
            bitmap.EndInit();

            UserImage.Source = bitmap;
            UserName.Content += ": " + user.Name;
            UserIDV.Content += ": " + user.UserId;
            Role.Content += ": " + user.Role;
            BadgeId.Content += ": " + user.BadgeId;
            PhoneNumber.Content += ": " + user.PhoneNumber;
            Function.Content += ": " + FunctionName;
            KazerneId.Content += ": " + KazerneID;
            EenheidId.Content += ": " + EenheidID;

            Validator validator = new Validator();
            MainWindow mw = MW;
            int user_id = mw.GetUserID();
            validator.logged_in_user_id = user_id;
            if (validator.validate("Gebruikers_Aanpassen")) { BTNPasGebruikerAan.Visibility = Visibility.Visible; } else { BTNPasGebruikerAan.Visibility = Visibility.Hidden; }
        }

        private void EditDelict_Click(object sender, RoutedEventArgs e)
        {
            Mw.EditUser(UserID, FunctionName, functionid);
        }

        private void Terug_Click(object sender, RoutedEventArgs e)
        {
            Mw.ShowUserList();
        }
    }
}
