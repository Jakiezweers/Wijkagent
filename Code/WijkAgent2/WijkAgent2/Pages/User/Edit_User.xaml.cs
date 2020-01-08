using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Wijkagent2.Classes;
using WijkAgent2.Database;
using System.Text.RegularExpressions;

namespace WijkAgent2.Pages.User
{
    /// <summary>
    /// Interaction logic for Edit_User.xaml
    /// </summary>
    /// 
    
    public partial class Edit_User : Page
    {
        int UserId;
        MainWindow Mw;
        List<string> RolesList = new List<string>();
        List<string> FunctieList = new List<string>();
        Wijkagent2.Classes.User user = new Wijkagent2.Classes.User();
        Connection cn = new Connection();
        int kazerneID = 0;
        int EenheidID = 0;
        int Functionid = 0;
        string FunctionName = null;
        string Image_Uploaded = "";
        bool NewPhoto = false;
        string userRoleName;

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }


        public Edit_User(MainWindow MW, int userId, string fname, int fid)
        {
            UserId = userId;
            int rolid;
            string rolname;
            Mw = MW;
            InitializeComponent();
            Functionid = fid;
            FunctionName = fname;

            cn.OpenConection();
            SqlDataReader sq = cn.DataReader("select us.*, up.upload_path, r.rol_name from[dbo].[User] us join[dbo].[uploads] up on us.upload_id = up.upload_id join[dbo].[rol] r on us.rol_id = r.rol_id where us.user_id = " + UserId);
            
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
                kazerneID = (int)sq["kazerne_id"];
                EenheidID = (int)sq["eenheid_id"];

            }
            cn.CloseConnection();
            cn.OpenConection();
            SqlDataReader sq1 = cn.DataReader("select * from [dbo].[rol]");

            while (sq1.Read()) 
            {
                RolesList.Add((string)sq1["rol_name"]);
            }
            cn.CloseConnection();

            cn.OpenConection();
            SqlDataReader sq2 = cn.DataReader("select * from [dbo].[functie]");

            while (sq2.Read())
            {
                FunctieList.Add((string)sq2["name"]);
            }
            cn.CloseConnection();

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(user.ProfilePicture.Path);
            bitmap.EndInit();

            UserImage.Source = bitmap;
            UserNameTB.Text += user.Name;
            UserIDTB.Content += "" + user.UserId;
            BadgeIdTB.Content += "" + user.BadgeId;
            PhoneNumberTB.Text += user.PhoneNumber;
            RoleCB.ItemsSource = RolesList;
            RoleCB.SelectedItem = user.Role.RoleName;
            userRoleName = user.Role.RoleName;
            KazerneIdTB.Text += kazerneID;
            EenheidIdTB.Text += EenheidID;
            FunctieCB.ItemsSource = FunctieList;
            FunctieCB.SelectedItem = FunctionName;
        }

        private void Terug_Click(object sender, RoutedEventArgs e)
        {
             Mw.UserView(user.UserId);
        }

        private void Opslaan_Click(object sender, RoutedEventArgs e)
        {
            if (RoleCB.SelectedItem != "Admin" && userRoleName == "Admin") {
                MessageBoxResult dialogResult = MessageBox.Show("Weet u zeker dat u de admin rol wil veranderen?", "Gebruiker", MessageBoxButton.YesNo);
                if (dialogResult == MessageBoxResult.No)
                {
                    return;
                }
            }

            if (PhoneNumberTB.Text.Length > 10)
            {
                PhoneNumberTB.BorderBrush = System.Windows.Media.Brushes.Red;
                
                return;
            }

            Int32 upload_id = 0;
            if (NewPhoto == true)
            {
                string SaveImage = "INSERT into [dbo].[uploads]" +
                    "(upload_path)" +
                    "OUTPUT INSERTED.upload_id " +
                    "VALUES (@upload_path)";

                using (SqlCommand querySaveStaff = new SqlCommand(SaveImage))
                {
                    cn.OpenConection();
                    querySaveStaff.Connection = cn.GetConnection();
                    querySaveStaff.Parameters.Add("@upload_path", SqlDbType.VarChar, 64).Value = Image_Uploaded.ToString().Trim();

                    upload_id = (Int32)querySaveStaff.ExecuteScalar();
                    cn.CloseConnection();
                }
            }
            cn.OpenConection();
            int rolid = 0;
            SqlDataReader sq = cn.DataReader("select * from [DBO].[rol] where rol_name = '" + RoleCB.SelectedItem + "'");
            while (sq.Read()) 
            { 
                rolid = (int)sq["rol_id"];
                userRoleName = (string)sq["rol_name"];
                Console.WriteLine((int)sq["rol_id"]);
            }
            cn.CloseConnection();

            cn.OpenConection();
            SqlDataReader sq1 = cn.DataReader("select * from [DBO].[Functie] where name = '" + FunctieCB.SelectedItem + "'");
            while (sq1.Read())
            {
                Functionid = (int)sq1["functie_id"];
            }
            cn.CloseConnection();

            cn.OpenConection();
            if (NewPhoto == true)
            {
                cn.ExecuteQueries("UPDATE [dbo].[USER]" +
                    "set rol_id = " + rolid + ", name = '" + UserNameTB.Text + "', tel = '" + PhoneNumberTB.Text + "', functie_id = " + Functionid + ", kazerne_id = " + KazerneIdTB.Text + ", eenheid_id = " + EenheidIdTB.Text + ", upload_id = " + upload_id +
                    " Where user_id = " + user.UserId);
            } else
            {
                cn.ExecuteQueries("UPDATE [dbo].[USER]" +
                    "set rol_id = " + rolid + ", name = '" + UserNameTB.Text + "', tel = '" + PhoneNumberTB.Text + "', functie_id = " + Functionid + ", kazerne_id = " + KazerneIdTB.Text + ", eenheid_id = " + EenheidIdTB.Text +
                    " Where user_id = " + user.UserId);
            }
            cn.CloseConnection();
            Mw.UserView(user.UserId);
        }

        private void BtnTakeImage_Click(object sender, RoutedEventArgs e)
        {
            BtnTakeImage.Content = "Een moment geduld";
            string file = Mw.select_file_dialog();
            if (file != "")
            {
                set_image(file);
                sendFileAsync(file);
            }
            BtnTakeImage.Content = "Select";
            NewPhoto = true;
        }

        private async void set_image(string file)
        {
            UserImage.Source = null;
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.DecodePixelWidth = 100;
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.UriSource = new Uri(file);
            bi.EndInit();
            bi.Freeze();
            await UserImage.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (ThreadStart)delegate ()
            {
                UserImage.Source = bi;
            });
        }

        public async Task sendFileAsync(string filename)
        {
            Uploader upload = new Uploader();
            byte[] imageArray = System.IO.File.ReadAllBytes(filename);
            string base64ImageRepresentation = Convert.ToBase64String(imageArray);
            Array l = filename.Split('.');
            Random r = new Random();
            String resp = await upload.SendFileAsync(base64ImageRepresentation, filename, "icon/", DateTime.UtcNow.Ticks + "_" + r.Next(1000, 999999) + "." + l.GetValue(l.Length - 1));
            this.Image_Uploaded = resp;
            Console.WriteLine(resp);
        }
    }
}
