using Microsoft.Win32;
using System;
using System.Collections.Generic;
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

namespace WijkAgent2.Pages.User
{
    /// <summary>
    /// Interaction logic for user_registratie.xaml
    /// </summary>
    public partial class user_registratie : Page
    {
        MainWindow mw;
        public user_registratie(MainWindow MW)
        {
            mw = MW;
            InitializeComponent();
        }

        private void BtnReturn_Click(object sender, RoutedEventArgs e)
        {
            mw.LoadHomeScreen();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            mw.ShowMessage("Gebruiker is toegevoegd.");
            mw.ShowUserList();
        }
        public async Task sendFileAsync(String file_data, string filename)
        {
            Uploader upload = new Uploader();


            Random r = new Random();
            String resp = await upload.SendFileAsync(file_data, filename, "icon/", r.Next(0, 10000000) + ".png");
            //LblResp.Content = resp;
        }

        private void BtnTakeImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg)|*.png;*.jpeg|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                BitmapImage image = new BitmapImage(new Uri(openFileDialog.FileName, UriKind.Absolute));
                ImgHead.Source = image;
                byte[] imageArray = System.IO.File.ReadAllBytes(openFileDialog.FileName);
                string base64ImageRepresentation = Convert.ToBase64String(imageArray);
                this.sendFileAsync(base64ImageRepresentation, openFileDialog.FileName);

            }
        }
    }
}
