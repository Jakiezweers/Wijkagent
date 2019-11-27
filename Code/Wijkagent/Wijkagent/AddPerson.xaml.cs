using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
//using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Wijkagent
{
    /// <summary>
    /// Interaction logic for AddPerson.xaml
    /// </summary>
    public partial class AddPerson : Window
    {
        public List<int> bsnlist = new List<int>();
        public List<string> typelist = new List<string>();

        public AddPerson()
        {
            InitializeComponent();
            AddToCategoryCB();
        }


        public void RefreshData()
        {
            Personen.Items.Clear();
            for (int i = 0; i < typelist.Count; i++)
            {
                Person p1 = new Person();
                p1.bsn = bsnlist[i];
                p1.type = typelist[i];
                Personen.Items.Add(p1);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            int parse = int.Parse(bsnfield.Text);
            bsnlist.Add(parse);
            typelist.Add(CategoryCB.Text);
            Console.WriteLine(CategoryCB.Text);
            Console.WriteLine(bsnfield.Text);
            RefreshData();
        }

        private void Personen_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Combobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void AddToCategoryCB()
        {
            CategoryCB.Items.Add("Verdachte");
            CategoryCB.Items.Add("Getuige");
            CategoryCB.Items.Add("Moordenaar");
        }

        private void CancelAddPerson_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Hidden;
        }
    }
}
