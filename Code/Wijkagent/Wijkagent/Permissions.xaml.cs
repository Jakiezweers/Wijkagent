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

namespace Wijkagent
{
    /// <summary>
    /// Interaction logic for Permissions.xaml
    /// </summary>
    public partial class Permissions : Window
    {
        
        public Permissions()
        {
            InitializeComponent();
            //DataGrid_SelectionChanged(DataGrid, );
        }

        public struct MyData
        {
            public string Naam { set; get; }
            public int Leeftijd { set; get; }
            public int BadgeID { set; get; }
            public string Rol { set; get; }
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid.Items.Add(new MyData { Naam = "bob", Leeftijd = 12, BadgeID = 112, Rol = "slave"});
        }
    }
}
