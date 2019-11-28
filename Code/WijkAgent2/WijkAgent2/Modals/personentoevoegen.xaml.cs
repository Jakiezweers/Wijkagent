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
using System.Windows.Shapes;
using Wijkagent2.Classes;

namespace WijkAgent2.Modals
{
    /// <summary>
    /// Interaction logic for personentoevoegen.xaml
    /// </summary>
    public partial class personentoevoegen : Window
    {
        public List<int> bsnlist = new List<int>();
        public List<string> typelist = new List<string>();

        public personentoevoegen()
        {
            InitializeComponent();
            AddPersonCategoryCB();
            base.Closing += this.CloseWindow;
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
        private void AddPersonButton(object sender, RoutedEventArgs e)
        {
            int BSNNumber = int.Parse(bsnfield.Text);
            foreach (var item in bsnlist)
            {
                if(item == BSNNumber)
                {
                    BSNErrorLabel.Content = "BSN nummer is al gebruikt.";
                    return;
                }
            }
            BSNErrorLabel.Content = "BSN succesvol toegevoegd.";
            bsnlist.Add(BSNNumber);
            typelist.Add(CategoryCB.Text);
            RefreshData();
        }

        private void Personen_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Combobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ClickCancel(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Hidden;
        }
        private void CloseWindow(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Visibility = Visibility.Hidden;
        }
        private void AddPersonCategoryCB()
        {
            CategoryCB.Items.Add("Verdachte");
            CategoryCB.Items.Add("Getuige");
            CategoryCB.Items.Add("Dader");
            CategoryCB.Items.Add("Slachtoffer");
        }
    }
}