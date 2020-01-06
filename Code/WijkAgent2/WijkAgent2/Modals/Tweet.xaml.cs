using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace WijkAgent2.Modals
{
    /// <summary>
    /// Interaction logic for Tweet.xaml
    /// </summary>
    public partial class Tweet : UserControl
    {
        public Tweet()
        {
            InitializeComponent();
        }

        private void BtnShowTweet_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(Link.Text.ToString().Trim());
            }
            catch (Exception) { }
        }
    }
}
