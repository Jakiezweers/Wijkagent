using System;
using System.Collections.Generic;
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
    /// Interaction logic for personentoevoegen.xaml
    /// </summary>
    public partial class personentoevoegen : Window
    {
        
            string provider = ConfigurationManager.AppSettings["provider"];
            string connectionstring = ConfigurationManager.AppSettings["connectionString"];
            public List<int> bsnlist = new List<int>();
            public List<string> typelist = new List<string>();
        int i = 0;


        public personentoevoegen()
        {
            InitializeComponent();
            combobox.Items.Add("Verdachte");
            combobox.Items.Add("Getuige");
            combobox.Items.Add("Moordenaar");

        }


        public void refreshData()
        {

            //DbProviderFactory factory = DbProviderFactories.GetFactory(provider);
            //using (DbConnection connection = factory.CreateConnection())
            //{
            //    if (connection == null)
            //    {
            //        Console.WriteLine("connection Error");
            //        Console.ReadLine();
            //        return;
            //    }
            //    Console.WriteLine("connection geslaagd");

            //    connection.ConnectionString = connectionstring;
            //    connection.Open();
            //    DbCommand command = factory.CreateCommand();
            //    if (command == null)
            //    {
            //        Console.WriteLine("geen command gegeven");
            //        Console.ReadLine();
            //        return;
            //    }
            //    command.Connection = connection;
            //    command.CommandText = "Select TOP 1 * from dbo.delict_person ORDER BY delict_person_id DESC";
            //    using (DbDataReader dataReader1 = command.ExecuteReader())
            //    {
            //        while (dataReader1.Read())
            //        {
            //            Person p1 = new Person();
            //            p1.bsn = (int)dataReader1["bsn"];
            //            p1.type = (string)dataReader1["type"];
            //            Console.WriteLine($"{dataReader1["bsn"]}");
            //            Personen.Items.Add(p1);
            //        }
            //    }

            //    connection.Close();
            //}
            int i = 0;
            Personen.Items.Clear();
            foreach (string item in typelist) {
                Person p1 = new Person();
                    p1.bsn = bsnlist[i];
                    p1.type = typelist[i];
                    Personen.Items.Add(p1);
                
                i++;
                    }
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            int parse = int.Parse(bsnfield.Text);
            bsnlist.Add(parse);
            typelist.Add(combobox.Text);
            Console.WriteLine(combobox.Text);
            Console.WriteLine(bsnfield.Text);
            refreshData();
        }

        private void Personen_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Combobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ClickCancel(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
