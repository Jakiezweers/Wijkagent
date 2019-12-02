using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
using WijkAgent2.Pages;
using WijkAgent2.Pages.delicten;
using WijkAgent2.Pages.User;

namespace WijkAgent2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public static Snackbar Snackbar;
        public MainWindow()
        {
            InitializeComponent();

            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(2500);
            }).ContinueWith(t =>
            {
                //note you can use the message queue from any thread, but just for the demo here we 
                //need to get the message queue from the snackbar, so need to be on the dispatcher
                MainSnackbar.MessageQueue.Enqueue("First Log in to get the Most Amazing SHOW");
            }, TaskScheduler.FromCurrentSynchronizationContext());
            Snackbar = this.MainSnackbar;

            MainFrame.NavigationUIVisibility = NavigationUIVisibility.Hidden;
            MainFrame.Navigate(new Login(this));
        }

        public void LoadHomeScreen()
        {
            MainFrame.Navigate(new HomePage(this));
        }

        public void ShowUserList()
        {
            MainFrame.Navigate(new User_List(this));
        }

        public void ShowDelictenList()
        {
            MainFrame.Navigate(new delicten_list(this));
        }

        public void AddUser()
        {
            MainFrame.Navigate(new user_registratie(this));
        }
        public void ShowDelictenArchive()
        {
            MainFrame.Navigate(new delict_archive(this));
        }

        public void AddDelict()
        {
            MainFrame.Navigate(new add_delict(this));
        }

        public void Logout()
        {
            MainFrame.Navigate(new Login(this));
        }

        public void ShowDelict(int delictID)
        {
            MainFrame.Navigate(new view_delict(this,delictID));
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
                    case "LBDelicten":
                        ShowDelictenList();
                        break;
                    case "LBGebruikers":
                        ShowUserList();
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