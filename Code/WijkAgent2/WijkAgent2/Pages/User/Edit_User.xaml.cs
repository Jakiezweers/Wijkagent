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
    
    public partial class Edit_User : Page
    {
        int userID;
        MainWindow mw;
        List<string> rolesList = new List<string>();
        List<string> functieList = new List<string>();
        Wijkagent2.Classes.User user = new Wijkagent2.Classes.User();
        Connection cn = new Connection();
        int kazerneID = 0;
        int eenheidID = 0;
        int functionID = 0;
        string functionName = null;
        string image_Uploaded = "";
        bool newPhoto = false;
        string userRoleName;

        //checking if the number of the user is valid
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        //main constructor
        public Edit_User(MainWindow MW, int UserID, string fName, int fID)
        {
            userID = UserID;
            int rolID;
            string rolName;
            mw = MW;
            InitializeComponent();
            functionID = fID;
            functionName = fName;

            //opening connection of the database
            cn.OpenConection();
            SqlDataReader sq = cn.DataReader("select us.*, up.upload_path, r.rol_name from[dbo].[User] us join[dbo].[uploads] up on us.upload_id = up.upload_id join[dbo].[rol] r on us.rol_id = r.rol_id where us.user_id = " + userID);
            
            //getting the current selected user so it can be edited later
            while (sq.Read())
            {
                rolID = (int)sq["rol_id"];
                rolName = (string)sq["rol_name"];
                int id = Convert.ToInt32(sq["user_id"]);

                user.UserId = id;
                user.Role = new Roles(rolName, rolID);
                user.Name = (string)sq["name"];
                user.BadgeId = Convert.ToInt32(sq["badge_nr"]);
                user.PhoneNumber = (string)sq["tel"];
                user.ProfilePicture = new Uploads(Convert.ToInt32(sq["upload_id"]), (string)sq["upload_path"]);
                kazerneID = (int)sq["kazerne_id"];
                eenheidID = (int)sq["eenheid_id"];

            }
            cn.CloseConnection();
            cn.OpenConection();
            SqlDataReader sq1 = cn.DataReader("select * from [dbo].[rol]");

            //adding all available roles to the roleslist
            while (sq1.Read()) 
            {
                rolesList.Add((string)sq1["rol_name"]);
            }
            cn.CloseConnection();

            cn.OpenConection();
            SqlDataReader sq2 = cn.DataReader("select * from [dbo].[functie]");

            //adding all functions to the functielist
            while (sq2.Read())
            {
                functieList.Add((string)sq2["name"]);
            }
            cn.CloseConnection();

            //setting the path of the user image
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(user.ProfilePicture.Path);
            bitmap.EndInit();

            //filling in all the fields for the user to edit
            userImage.Source = bitmap;
            UserNameTB.Text += user.Name;
            UserIDTB.Content += "" + user.UserId;
            BadgeIdTB.Content += "" + user.BadgeId;
            PhoneNumberTB.Text += user.PhoneNumber;
            RoleCB.ItemsSource = rolesList;
            RoleCB.SelectedItem = user.Role.RoleName;
            userRoleName = user.Role.RoleName;
            KazerneIdTB.Text += kazerneID;
            EenheidIdTB.Text += eenheidID;
            FunctieCB.ItemsSource = functieList;
            FunctieCB.SelectedItem = functionName;
        }

        //button for getting back to the user page
        private void Terug_Click(object sender, RoutedEventArgs e)
        {
             mw.UserView(user.UserId);
        }

        //button for saving the current user
        private void Opslaan_Click(object sender, RoutedEventArgs e)
        {
            //validator for the user if the admin role is getting removed
            if (RoleCB.SelectedItem != "Admin" && userRoleName == "Admin") {
                MessageBoxResult dialogResult = MessageBox.Show("Weet u zeker dat u de admin rol wil veranderen?", "Gebruiker", MessageBoxButton.YesNo);
                if (dialogResult == MessageBoxResult.No)
                {
                    return;
                }
            }

            // check if the phone number is correct
            if (PhoneNumberTB.Text.Length > 10)
            {
                PhoneNumberTB.BorderBrush = System.Windows.Media.Brushes.Red;
                
                return;
            }

            Int32 upload_id = 0;
            // if a new photo has been added, add that photo to the database
            if (newPhoto == true)
            {
                string SaveImage = "INSERT into [dbo].[uploads]" +
                    "(upload_path)" +
                    "OUTPUT INSERTED.upload_id " +
                    "VALUES (@upload_path)";

                using (SqlCommand querySaveStaff = new SqlCommand(SaveImage))
                {
                    cn.OpenConection();
                    querySaveStaff.Connection = cn.GetConnection();
                    querySaveStaff.Parameters.Add("@upload_path", SqlDbType.VarChar, 64).Value = image_Uploaded.ToString().Trim();

                    upload_id = (Int32)querySaveStaff.ExecuteScalar();
                    cn.CloseConnection();
                }
            }
            cn.OpenConection();

            //getting the id of the selected role
            int rolid = 0;
            SqlDataReader sq = cn.DataReader("select * from [DBO].[rol] where rol_name = '" + RoleCB.SelectedItem + "'");
            while (sq.Read()) 
            { 
                rolid = (int)sq["rol_id"];
                userRoleName = (string)sq["rol_name"];
                Console.WriteLine((int)sq["rol_id"]);
            }
            cn.CloseConnection();

            // getting the current selected function id
            cn.OpenConection();
            SqlDataReader sq1 = cn.DataReader("select * from [DBO].[Functie] where name = '" + FunctieCB.SelectedItem + "'");
            while (sq1.Read())
            {
                functionID = (int)sq1["functie_id"];
            }
            cn.CloseConnection();

            //updating the user
            cn.OpenConection();
            //if a new photo has been selected
            if (newPhoto == true)
            {
                cn.ExecuteQueries("UPDATE [dbo].[USER]" +
                    "set rol_id = " + rolid + ", name = '" + UserNameTB.Text + "', tel = '" + PhoneNumberTB.Text + "', functie_id = " + functionID + ", kazerne_id = " + KazerneIdTB.Text + ", eenheid_id = " + EenheidIdTB.Text + ", upload_id = " + upload_id +
                    " Where user_id = " + user.UserId);
            } else
            //if a new photo has not been selected
            {
                cn.ExecuteQueries("UPDATE [dbo].[USER]" +
                    "set rol_id = " + rolid + ", name = '" + UserNameTB.Text + "', tel = '" + PhoneNumberTB.Text + "', functie_id = " + functionID + ", kazerne_id = " + KazerneIdTB.Text + ", eenheid_id = " + EenheidIdTB.Text +
                    " Where user_id = " + user.UserId);
            }
            cn.CloseConnection();
            mw.UserView(user.UserId);
        }

        //selecting of a new photo
        private void BtnTakeImage_Click(object sender, RoutedEventArgs e)
        {
            BtnTakeImage.Content = "Een moment geduld";
            string file = mw.select_file_dialog();
            if (file != "")
            {
                set_image(file);
                sendFileAsync(file);
            }
            BtnTakeImage.Content = "Select";
            newPhoto = true;
        }

        //setting the new image
        private async void set_image(string file)
        {
            userImage.Source = null;
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.DecodePixelWidth = 100;
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.UriSource = new Uri(file);
            bi.EndInit();
            bi.Freeze();
            await userImage.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (ThreadStart)delegate ()
            {
                userImage.Source = bi;
            });
        }

        //uploading the selected photo
        public async Task sendFileAsync(string filename)
        {
            Uploader upload = new Uploader();
            byte[] imageArray = System.IO.File.ReadAllBytes(filename);
            string base64ImageRepresentation = Convert.ToBase64String(imageArray);
            Array l = filename.Split('.');
            Random r = new Random();
            String resp = await upload.SendFileAsync(base64ImageRepresentation, filename, "icon/", DateTime.UtcNow.Ticks + "_" + r.Next(1000, 999999) + "." + l.GetValue(l.Length - 1));
            this.image_Uploaded = resp;
            Console.WriteLine(resp);
        }
    }
}
