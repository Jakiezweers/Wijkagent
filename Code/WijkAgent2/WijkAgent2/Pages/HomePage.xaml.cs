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

namespace WijkAgent2.Pages
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        private MainWindow mw;
        public HomePage(MainWindow MW)
        {
            mw = MW;
            InitializeComponent();
        }


        private void LogOut_Click(object sender, RoutedEventArgs e)
        {
            mw.Logout();
        }

        private void AddDelict_Click(object sender, RoutedEventArgs e)
        {
            mw.AddDelict();
        }

        private void DelictList_Click(object sender, RoutedEventArgs e)
        {
            mw.ShowDelictenList();
        }

        private void DelictArchive_Click(object sender, RoutedEventArgs e)
        {
            mw.ShowDelictenArchive();
        }

        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            mw.AddUser();
        }

    }

}
