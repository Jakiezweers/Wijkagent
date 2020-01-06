using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wijkagent2.Classes;
using WijkAgent2.Classes;
using WijkAgent2.Pages;
using WijkAgent2.Pages.delicten;
using WijkAgent2.Pages.permissions;
using WijkAgent2.Pages.User;

namespace WijkAgent2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static Snackbar Snackbar;
        private User user;

        Validator validator;

        public MainWindow()
        {
            user = new User();
            validator = new Validator();
            InitializeComponent();
            TopHeader.Text = "Wijkagent - Login";
            MainFrame.NavigationUIVisibility = NavigationUIVisibility.Hidden;
            MenuToggleButton.Visibility = Visibility.Hidden;
            UserInfo.Visibility = Visibility.Hidden;
            MainFrame.Navigate(new Login(this));
        }
        public bool check_permission(string perrmission_on)
        {
            return validator.validate(perrmission_on);
        }

        public void set_loggedin_user_id(int user_id, string name, string Image_Url)
        {
            validator.logged_in_user_id = user_id;
            MenuToggleButton.Visibility = Visibility.Visible;

            if (validator.validate("list_delicten")) { LBDelicten.Visibility = Visibility.Visible; } else { LBDelicten.Visibility = Visibility.Collapsed; }
            if (validator.validate("list_archive")) { LBArchive.Visibility = Visibility.Visible; } else { LBArchive.Visibility = Visibility.Collapsed; }
            if (validator.validate("list_users")) { LBGebruikers.Visibility = Visibility.Visible; } else { LBGebruikers.Visibility = Visibility.Collapsed; }
            if (validator.validate("Delicten_Aanmaken")) { LBAddDelict.Visibility = Visibility.Visible; } else { LBAddDelict.Visibility = Visibility.Collapsed; }
            if (validator.validate("Permissies_Toewijzen")) { LBPermissions.Visibility = Visibility.Visible; } else { LBPermissions.Visibility = Visibility.Collapsed; }
            if (validator.validate("Delicten_Inzien")) { LBDelicten.Visibility = Visibility.Visible; } else { LBDelicten.Visibility = Visibility.Collapsed; }
            NameHeader.Text = name;
            UserInfo.Visibility = Visibility.Visible;
            UserImage.Source = new BitmapImage(new Uri(Image_Url, UriKind.RelativeOrAbsolute));
        }

        public int GetUserID()
        {
            return validator.logged_in_user_id;
        }

        public string select_file_dialog()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                DereferenceLinks = false,
            };
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            openFileDialog.Filter = "Image files (*.png;*.jpeg)|*.png;*.jpeg";
            if (openFileDialog.ShowDialog() == true)
            {
                return openFileDialog.FileName;
            }
            else
            {
                return "";
            }
        }

        public async void ShowDialog(string text)
        {
            Message ms = new Message();
            ms.message = text;
            await DialogHost.Show(ms, "MessageDialog");
        }

        public void ShowMessage(string Message)
        {
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(250);
            }).ContinueWith(t =>
            {
                MainSnackbar.MessageQueue.Enqueue(Message);
            }, TaskScheduler.FromCurrentSynchronizationContext());
            Snackbar = this.MainSnackbar;
        }
        public string FirstCharToUpper(string source)
        {
            if (string.IsNullOrEmpty(source))
                return string.Empty;
            // convert to char array of the string
            char[] letters = source.ToCharArray();
            // upper case the first char
            letters[0] = char.ToUpper(letters[0]);
            // return the array made of the new char array
            return new string(letters);
        }

        public void LoadPermissionPage()
        {
            MainFrame.Navigate(new permission_window(this));
            TopHeader.Text = "Wijkagent - Permissions";

        }

        public void LoadHomeScreen()
        {
            MainFrame.Navigate(new HomePage(this));
            TopHeader.Text = "Wijkagent - Home";
        }

        public void ShowUserList()
        {
            MainFrame.Navigate(new User_List(this));
            TopHeader.Text = "Wijkagent - Gebruiker lijst";
        }

        public void ShowDelictenList(bool activeDelicts)
        {
            MainFrame.Navigate(new delicten_list(this, activeDelicts));
            TopHeader.Text = "Wijkagent - Delicten lijst";
        }

        public void AddUser()
        {
            MainFrame.Navigate(new user_registratie(this));
            TopHeader.Text = "Wijkagent - Gebruiker toevoegen";
        }

        public void ShowDelictenArchive()
        {
            MainFrame.Navigate(new delicten_list(this,false));
            TopHeader.Text = "Wijkagent - Delicten Archief";
        }

        public void AddDelict()
        {
            MainFrame.Navigate(new add_delict(this));
            TopHeader.Text = "Wijkagent - Delict toevoegen";
        }
        public void AddDelict(double lon, double lat)
        {
            MainFrame.Navigate(new add_delict(this,lon,lat));
            TopHeader.Text = "Wijkagent - Delict toevoegen";
        }

        public void UserView(int id)
        {
            MainFrame.Navigate(new UserView(this, id));
            TopHeader.Text = "Wijkagent - Delict toevoegen";
        }

        public void Logout()
        {
            MainFrame.Navigate(new Login(this));
            MenuToggleButton.Visibility = Visibility.Hidden;
            NameHeader.Text = "";
            UserInfo.Visibility = Visibility.Hidden;
            TopHeader.Text = "Wijkagent - Login";
        }

        public void ShowDelict(int delictID, int originalPage)
        {
            MainFrame.Navigate(new view_delict(this,delictID,originalPage));
            TopHeader.Text = "Wijkagent - Delict " + delictID;
        }
        public void EditDelict(int delictID, int previousPage)
        {
            MainFrame.Navigate(new edit_delict(this, delictID, previousPage));
        }

        public void EditUser(int userId, string fname, int fid)
        {
            MainFrame.Navigate(new Edit_User(this, userId, fname, fid));
            TopHeader.Text = "Wijkagent - Gebruiker " + userId;
        }

        public void close()
        {
            Close();
        }


        private void UIElement_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //until we had a StaysOpen glag to Drawer, this will help with scroll bars
            var dependencyObject = Mouse.Captured as DependencyObject;
            while (dependencyObject != null)
            {
                if (dependencyObject is ScrollBar) return;
                dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
            }
            var item = ItemsControl.ContainerFromElement(sender as ListBox, e.OriginalSource as DependencyObject) as ListBoxItem;
            if (item != null)
            {
                switch (item.Name)
                {
                    case "LBHome":
                        LoadHomeScreen();
                        break;
                    case "LBAddDelict":
                        AddDelict();
                        break;
                    case "LBDelicten":
                        ShowDelictenList(true);
                        break;
                    case "LBGebruikers":
                        ShowUserList();
                        break;
                    case "LBArchive":
                        ShowDelictenArchive();
                        break;
                    case "LBPermissions":
                        LoadPermissionPage();
                        break;
                    case "LBLogout":
                        Logout();
                        break;
                }
                
            }
            MenuToggleButton.IsChecked = false;
        }

        private void OnCopy(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is string stringValue)
            {
                try
                {
                    Clipboard.SetDataObject(stringValue);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.ToString());
                }
            }
        }
    }
}