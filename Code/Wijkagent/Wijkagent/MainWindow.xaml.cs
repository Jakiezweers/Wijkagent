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

namespace Wijkagent
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void LogOut_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void AddDelict_Click(object sender, RoutedEventArgs e)
        {
            AddDelictWindow adddelict = new AddDelictWindow();
            adddelict.Show();
            this.Close();
        }

        private void DelictList_Click(object sender, RoutedEventArgs e)
        {
            DelictListWindow delictlist = new DelictListWindow();
            delictlist.Show();
            this.Close();
        }

        private void DelictArchive_Click(object sender, RoutedEventArgs e)
        {
            DelictArchiveWindow delictarchive = new DelictArchiveWindow();
            delictarchive.Show();
            this.Close();
        }

        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            RegisterWindow register = new RegisterWindow();
            register.Show();
            this.Close();
        }
    }
}
