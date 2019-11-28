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
using WijkAgent2.Pages;
using WijkAgent2.Pages.delicten;

namespace WijkAgent2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.NavigationUIVisibility = NavigationUIVisibility.Hidden;
            MainFrame.Navigate(new Login(this));
        }

        public void LoadHomeScreen()
        {
            MainFrame.Navigate(new HomePage(this));
        }

        public void ShowDelictenList()
        {
            MainFrame.Navigate(new delicten_list(this));
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

    }
}
