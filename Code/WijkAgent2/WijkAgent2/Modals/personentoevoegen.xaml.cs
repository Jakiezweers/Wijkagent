using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Wijkagent2.Classes;
using MessageBox = System.Windows.Forms.MessageBox;

namespace WijkAgent2.Modals
{
    /// <summary>
    /// Interaction logic for personentoevoegen.xaml
    /// </summary>
    public partial class personentoevoegen : Window
    {
        public List<int> bsnlist = new List<int>();
        public List<string> typelist = new List<string>();
        public List<int> person_idList = new List<int>();

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
                p1.person_id = person_idList[i];
                p1.bsn = bsnlist[i];
                p1.type = typelist[i];
                Personen.Items.Add(p1);
            }
        }
        private void AddPersonButton(object sender, RoutedEventArgs e)
        {
            int BSNNumber = int.Parse(bsnfield.Text);
            int person_id = CheckIfPersonExists(BSNNumber);
            if(person_id == 0)
            {
                MessageBox.Show("Er is iets foutgegaan!");
                return;
            }
            foreach (var item in bsnlist)
            {
                if(item == BSNNumber)
                {
                    BSNErrorLabel.Content = "Een persoon mag maar één keer worden toegevoegd.";
                    return;
                }
            }
            BSNErrorLabel.Content = "Persoon succesvol toegevoegd.";
            person_idList.Add(person_id);
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
        private int CheckIfPersonExists(int personBSN)
        {
            string provider = ConfigurationManager.AppSettings["provider"];
            string connectionstring = ConfigurationManager.AppSettings["connectionString"];

            DbProviderFactory factory = DbProviderFactories.GetFactory(provider);

            using (DbConnection connection = factory.CreateConnection())
            {
                connection.ConnectionString = connectionstring;
                connection.Open();
                DbCommand command = factory.CreateCommand();

                command.Connection = connection;
                command.CommandText = "Select * from dbo.person WHERE BSN = " + personBSN;

                int UserExists = 0;
                var executed = command.ExecuteScalar();
                if (executed != null)
                {
                    return UserExists = (int)executed;
                }
                else
                {
                    return AddNewPerson(personBSN);
                }
            }
        }
        private int AddNewPerson(int bsnNumber)
        {
            int promptValue = Prompt.ShowDialog(bsnNumber);
            return promptValue;
        }

        private void AddPersonCategoryCB()
        {
            CategoryCB.Items.Add("Verdachte");
            CategoryCB.Items.Add("Getuige");
            CategoryCB.Items.Add("Dader");
            CategoryCB.Items.Add("Slachtoffer");
        }
    }
    public class Prompt
    {
        public static int ShowDialog(int bsnNumber)
        {
            Form prompt = new Form()
            {
                Width = 500,
                Height = 270,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "Persoon toevoegen.",
                StartPosition = FormStartPosition.CenterScreen
            };

            System.Windows.Forms.Label NameTextLabel = new System.Windows.Forms.Label() { Left = 50, Top = 20, Text = "Voornaam: " };
            System.Windows.Forms.TextBox NameTextBox = new System.Windows.Forms.TextBox() { Left = 50, Top = 50, Width = 400 };
            System.Windows.Forms.Label SurNameTextLabel = new System.Windows.Forms.Label() { Left = 50, Top = 80, Text = "Achternaam: " };
            System.Windows.Forms.TextBox SurNameTextBox = new System.Windows.Forms.TextBox() { Left = 50, Top = 110, Width = 400 };
            System.Windows.Forms.Label BirthDateLabel = new System.Windows.Forms.Label() { Left = 50, Top = 140, Text = "Geboorte datum: " };
            DateTimePicker BirthDateTextBox = new DateTimePicker() { Left = 50, Top = 170, Width = 170 };
            System.Windows.Forms.Button confirmation = new System.Windows.Forms.Button() { Text = "Persoon toevoegen", Left = 350, Width = 130, Top = 200, DialogResult = DialogResult.OK };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(NameTextLabel);
            prompt.Controls.Add(NameTextBox);
            prompt.Controls.Add(SurNameTextLabel);
            prompt.Controls.Add(SurNameTextBox);
            prompt.Controls.Add(BirthDateLabel);
            prompt.Controls.Add(BirthDateTextBox);
            prompt.Controls.Add(confirmation);
            prompt.AcceptButton = confirmation;

            if(prompt.ShowDialog() == DialogResult.OK)
            {
                string firstName = NameTextBox.Text;
                string lastName = SurNameTextBox.Text;
                DateTime birthDate = BirthDateTextBox.Value;
                Prompt promptclass = new Prompt();
                int person_id = promptclass.AddPersonToDatabase(firstName, lastName, birthDate, bsnNumber);
                return person_id;
            }
            else
            {
                return 0;
            }
        }
        public int AddPersonToDatabase(string firstname, string lastname, DateTime birthdate, int bsnnummer)
        {
            string connectionstring = ConfigurationManager.AppSettings["connectionString"];
            string sqlInsertPerson = "insert into dbo.person (firstname, lastname, birthdate, BSN) OUTPUT INSERTED.person_id values (@firstname,@lastname,@birthdate,@BSN)";
            using (SqlConnection cnn = new SqlConnection(connectionstring))
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand(sqlInsertPerson, cnn))
                    {
                        cmd.Parameters.Add("@firstname", SqlDbType.NVarChar).Value = firstname;
                        cmd.Parameters.Add("@lastname", SqlDbType.NVarChar).Value = lastname;
                        cmd.Parameters.Add("@birthdate", SqlDbType.DateTime).Value = birthdate;
                        cmd.Parameters.Add("@BSN", SqlDbType.NVarChar).Value = bsnnummer;
                        cnn.Open();
                        int returnedID = (int)cmd.ExecuteScalar();
                        return returnedID;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("ERROR:" + ex.Message);
                    return 0;
                }
            }
        }
    }
}