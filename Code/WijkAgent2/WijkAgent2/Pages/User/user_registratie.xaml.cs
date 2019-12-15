using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
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
    /// Interaction logic for user_registratie.xaml
    /// </summary>
    public partial class user_registratie : Page
    {
        MainWindow mw;Connection cn;
        string Image_Uploaded = "";
        public user_registratie(MainWindow MW)
        {
            mw = MW;
            InitializeComponent();

            cn = new Connection();
            cn.OpenConection();

            //Get Rollen
            SqlDataReader sq_rollen = cn.DataReader("SELECT * FROM dbo.rol");
            while (sq_rollen.Read())
            {
                Roles r = new Roles((string)sq_rollen["name"], Convert.ToInt32(sq_rollen["rol_id"]));
                CBRol.Items.Add(r);
            }
            cn.CloseConnection();

            //Get Functies
            cn.OpenConection();
            SqlDataReader sq_functie = cn.DataReader("SELECT * FROM dbo.Functie");
            while (sq_functie.Read())
            {
                functie f = new functie((string)sq_functie["name"], Convert.ToInt32(sq_functie["functie_id"]));
                CBFuntie.Items.Add(f);
            }
            cn.CloseConnection();

            //Get Kazernes
            cn.OpenConection();
            SqlDataReader sq_kazerne = cn.DataReader("SELECT * FROM dbo.Kazerne");
            while (sq_kazerne.Read())
            {
                Kazerne k = new Kazerne((string)sq_kazerne["name"], Convert.ToInt32(sq_kazerne["kazerne_id"]));
                CBKazerna.Items.Add(k);
            }
            cn.CloseConnection();

        }
        private void BtnReturn_Click(object sender, RoutedEventArgs e)
        {
            mw.ShowUserList();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (Validate())
            {
                //Image_Uploaded
                Int32 upload_id;
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


                string saveStaff = "INSERT into [dbo].[user]" +
                    "(rol_id,eenheid_id,functie_id,kazerne_id,upload_id,name,badge_nr,password,tel,status) " +
                    "VALUES (@rol_id,@eenheid_id,@functie_id,@kazerne_id,@upload_id,@name,@badge_nr,@password,@tel,@status)";

                using (SqlCommand querySaveStaff = new SqlCommand(saveStaff))
                {
                    cn.OpenConection();
                    querySaveStaff.Connection = cn.GetConnection();


                    Roles RO = (Roles)CBRol.SelectedItem;
                    Kazerne KA = (Kazerne)CBKazerna.SelectedItem;
                    Eenheid EH = (Eenheid)CBEenheid.SelectedItem;
                    functie FU = (functie)CBFuntie.SelectedItem;


                    querySaveStaff.Parameters.Add("@rol_id", SqlDbType.Int, 11).Value = RO.rol_id;
                    querySaveStaff.Parameters.Add("@eenheid_id", SqlDbType.Int, 11).Value = EH.eenheid_id;
                    querySaveStaff.Parameters.Add("@functie_id", SqlDbType.Int, 11).Value = FU.functie_id;
                    querySaveStaff.Parameters.Add("@kazerne_id", SqlDbType.Int, 11).Value = KA.kazerne_id;
                    querySaveStaff.Parameters.Add("@upload_id", SqlDbType.Int, 11).Value = upload_id;


                    querySaveStaff.Parameters.Add("@name", SqlDbType.VarChar, 64).Value = TxtName.Text.ToString().Trim();
                    querySaveStaff.Parameters.Add("@badge_nr", SqlDbType.VarChar, 128).Value = TxtBadgeNr.Text.ToString().Trim();
                    querySaveStaff.Parameters.Add("@tel", SqlDbType.VarChar, 128).Value = TxtTel.Text.ToString().Trim();


                    querySaveStaff.Parameters.Add("@password", SqlDbType.VarChar, 512).Value = PasswordHandler.CreatePasswordHash(TxtPassword.Password.ToString());
                    querySaveStaff.Parameters.Add("@status", SqlDbType.Int, 11).Value = 1;
                    querySaveStaff.ExecuteNonQuery();
                }

                mw.ShowMessage("Gebruiker is toegevoegd.");
                mw.ShowUserList();
            }
        }


        public async Task sendFileAsync( string filename)
        {
            Uploader upload = new Uploader();
            byte[] imageArray = System.IO.File.ReadAllBytes(filename);
            string base64ImageRepresentation = Convert.ToBase64String(imageArray);
            Array l = filename.Split('.');
            Random r = new Random();
            String resp = await upload.SendFileAsync(base64ImageRepresentation, filename, "icon/", DateTime.UtcNow.Ticks + "_" + r.Next(1000,999999) + "." + l.GetValue(l.Length - 1));
            this.Image_Uploaded = resp;
            Console.WriteLine(resp);
        }

        private async void set_image(string file)
        {
            ImgHead.Source = null;
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.DecodePixelWidth = 100;
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.UriSource = new Uri(file);
            bi.EndInit();
            bi.Freeze();
            await ImgHead.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (ThreadStart)delegate ()
            {
                 ImgHead.Source = bi;
            });
        }

        private void BtnTakeImage_Click(object sender, RoutedEventArgs e)
        {
            BtnTakeImage.Content = "Een moment geduld";
            string file = mw.select_file_dialog();
            if(file != "")
            {
                set_image(file);
                sendFileAsync(file);
            }
            BtnTakeImage.Content = "Select";
        }

        private void CBKazerna_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Get Eenheid based on Kazerne ID
            Kazerne KA = (Kazerne)CBKazerna.SelectedItem;
            CBEenheid.Items.Clear();
            cn.OpenConection();
            SqlDataReader sq_eenheid = cn.DataReader("SELECT eh.name,eh.eenheid_id FROM DBO.kazerne_eenheid ke JOIN dbo.eenheid eh on ke.eenheid_id = eh.eenheid_id WHERE ke.kazerne_id = "
                + KA.kazerne_id);
            while (sq_eenheid.Read())
            {
                Eenheid E = new Eenheid((string)sq_eenheid["name"], Convert.ToInt32(sq_eenheid["eenheid_id"]));
                CBEenheid.Items.Add(E);
            }
            cn.CloseConnection();
        }

        private bool Validate()
        {
            bool validated = true;
            foreach (Control ctrl in Regular.Children)
            {
                if (ctrl is TextBox)
                {
                    if (((TextBox)ctrl).Text.ToString().Trim().Equals(""))
                    {
                        ((TextBox)ctrl).BorderBrush = System.Windows.Media.Brushes.Red;
                        validated = false;
                    }
                    else
                    {
                        ((TextBox)ctrl).BorderBrush  = System.Windows.Media.Brushes.Black;
                    }
                }
                if (ctrl is PasswordBox)
                {
                    if (((PasswordBox)ctrl).Password.ToString().Trim().Equals(""))
                    {
                        ((PasswordBox)ctrl).BorderBrush = System.Windows.Media.Brushes.Red;
                        validated = false;
                    }
                    else
                    {
                        ((PasswordBox)ctrl).BorderBrush = System.Windows.Media.Brushes.Black;
                    }
                }
            }

            foreach (Control ctrl in ComboUsers.Children)
            {
                if (ctrl is ComboBox)
                {
                    if (((ComboBox)ctrl).SelectedIndex == -1)
                    {
                        ((ComboBox)ctrl).BorderBrush = System.Windows.Media.Brushes.Red;
                        validated = false;
                    }
                    else
                    {
                        ((ComboBox)ctrl).BorderBrush = System.Windows.Media.Brushes.Black;
                    }
                }
            }
            if (!validated)
            {
                mw.ShowDialog("Niet alle velden zijn ingevoerd");
            }
            else
            {
                if (Image_Uploaded.Equals(""))
                {
                    mw.ShowDialog("Please selecteer een image");
                    validated = false;
                }
            }
            if (validated)
            {
                cn.OpenConection();
                SqlDataReader SDR = cn.DataReader("select * from[dbo].[User] where badge_nr = '" + TxtBadgeNr.Text.ToString().Trim() + "'");
                if (SDR.HasRows)
                {
                    mw.ShowDialog("Deze Badge NR bestaat al");
                    validated = false;
                }
                else
                {
                    if (!TxtPassword.Password.ToString().Trim().Equals(TxtPasswordRepeat.Password.ToString().Trim())){
                        mw.ShowDialog("Wachtwoorden komen niet overeen");
                        validated = false;
                    }
                }
                cn.CloseConnection();
            }
            return validated;

        }
    }
}
