using System.Windows;

namespace Wijkagent
{


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
<<<<<<< HEAD
            InitializeComponent();
            
        }

        /*

        public async Task sendFileAsync(String file_data, string filename)
        {
            Uploader upload = new Uploader();


            Random r = new Random();
            String resp = await upload.SendFileAsync(file_data, filename, "icon/", r.Next(0,10000000)+ ".png");
            //LblResp.Content = resp;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg)|*.png;*.jpeg|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {


                byte[] imageArray = System.IO.File.ReadAllBytes(openFileDialog.FileName);
                string base64ImageRepresentation = Convert.ToBase64String(imageArray);
                this.sendFileAsync(base64ImageRepresentation, openFileDialog.FileName);


            }
        }
        */

        private void LogOut_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void AddDelict_Click(object sender, RoutedEventArgs e)
        {
            AddDelictWindow adddelict = new AddDelictWindow();
            adddelict.Show();
        }

        private void DelictList_Click(object sender, RoutedEventArgs e)
        {
            DelictListWindow delictlist = new DelictListWindow();
            delictlist.Show();
        }

        private void DelictArchive_Click(object sender, RoutedEventArgs e)
        {
            DelictArchiveWindow delictarchive = new DelictArchiveWindow();
            delictarchive.Show();
        }

        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            RegisterWindow register = new RegisterWindow();
            register.Show();
=======
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

>>>>>>> cdd1651fb30f803bf92cb4f94727c36e78c5a78f
        }
        
    }
}
