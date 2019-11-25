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
            Employee john = new Employee();
            john.test1 = "test1";
            john.test2 = "test2";
            john.test3 = "test3";
            john.test4 = "test4";

            Delicten.Items.Add(john);
            Delicten.Items.Add(john);

            InitializeComponent();
        }
        public class Employee
        {
            public string test1 { get; set; }
            public string test2 { get; set; }
            public string test3 { get; set; }
            public string test4 { get; set; }

        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
