using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WijkAgent2.Database;

namespace WijkAgent2.Pages.permissions
{
    public partial class permission_window : Page
    {
        private int role_id;
        public List<string> role = new List<string>();
        public List<int> role_idList = new List<int>();
        List<int> permissionList = new List<int>();
        List<int> usedPermissionList = new List<int>();
        List<int> unusedPermissionList = new List<int>();

        MainWindow mw;
        // main constructor
        public permission_window(MainWindow MW)
        {
            this.mw = MW;
            string provider = ConfigurationManager.AppSettings["provider"];
            string connectionstring = ConfigurationManager.AppSettings["connectionString"];

            DbProviderFactory factory = DbProviderFactories.GetFactory(provider);

            //testing the current connection
            using (DbConnection connection = factory.CreateConnection())
            {
                if (connection == null)
                {
                    Console.WriteLine("connection Error");
                    Console.ReadLine();
                    return;
                }
                Console.WriteLine("connection geslaagd");

                connection.ConnectionString = connectionstring;
                connection.Open();
                DbCommand command = factory.CreateCommand();
                if (command == null)
                {
                    Console.WriteLine("geen command gegeven");
                    Console.ReadLine();
                    return;
                }

                command.Connection = connection;

                //getting all the available roles from the database
                command.CommandText = "SELECT * FROM [Wijkagent].[dbo].[rol];";

                using (DbDataReader dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        role.Add(Convert.ToString(dataReader["rol_name"]));
                        role_idList.Add(Convert.ToInt32(dataReader["rol_id"]));
                    }
                }
            }

            InitializeComponent();

            comboBox.ItemsSource = role;
        }
        public string val;
        public object Sender;
        public SelectionChangedEventArgs E;

        // when the selection of the combobox with the roles has been changed, update the permissions selection
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Sender = sender;
            E = e;
            Console.WriteLine(sender);
            Console.WriteLine(e);
            val = comboBox.SelectedValue.ToString();
            LBPermissions.Items.Clear();
            permissionList.Clear();
            usedPermissionList.Clear();
            unusedPermissionList.Clear();
            string provider = ConfigurationManager.AppSettings["provider"];
            string connectionstring = ConfigurationManager.AppSettings["connectionString"];

            DbProviderFactory factory = DbProviderFactories.GetFactory(provider);

            // testing the current connection
            using (DbConnection connection = factory.CreateConnection())
            {
                if (connection == null)
                {
                    Console.WriteLine("connection Error");
                    Console.ReadLine();
                    return;
                }
                Console.WriteLine("connection geslaagd");

                connection.ConnectionString = connectionstring;
                connection.Open();
                DbCommand command = factory.CreateCommand();
                if (command == null)
                {
                    Console.WriteLine("geen command gegeven");
                    Console.ReadLine();
                    return;
                }

                // getting the current id of the selected role
                command.Connection = connection;
                command.CommandText = "SELECT * FROM [Wijkagent].[dbo].[rol] WHERE rol_name = '" + val + "';";

                using (DbDataReader dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        role_id = Convert.ToInt32(dataReader["rol_id"]);
                    }
                }

                // getting all the available permissions
                command.CommandText = "SELECT * FROM [Wijkagent].[dbo].[permission];";

                using (DbDataReader dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        permissionList.Add(Convert.ToInt32(dataReader["permission_id"]));
                    }
                }

                // getting all the permissions that were already bound to the selected role
                command.CommandText = "SELECT * FROM [Wijkagent].[dbo].[permission_rol] WHERE rol_id = " + role_id + ";";

                using (DbDataReader dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        usedPermissionList.Add(Convert.ToInt32(dataReader["permission_id"]));
                    }
                }

                // getting all the permissions that were not bound to the selected role
                foreach (var item in permissionList)
                {
                    if (!usedPermissionList.Contains(item)) 
                    {
                        unusedPermissionList.Add(item);
                    }
                }

                // for every used permission
                foreach (var item in usedPermissionList)
                {
                    //selecting the link per item
                    command.CommandText = "SELECT * FROM [Wijkagent].[dbo].[permission] WHERE permission_id = " + item + ";";

                    using (DbDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            bool Checked = true;
                            string name = Convert.ToString(dataReader["name"]);
                            Permission perm = new Permission(name, true, item);
                            // adding the permission to the list of permissions in the xaml window
                            LBPermissions.Items.Add(perm);
                        }
                    }
                }

                // for every unused permission
                foreach (var item in unusedPermissionList)
                {
                    command.CommandText = "SELECT * FROM [Wijkagent].[dbo].[permission] WHERE permission_id = " + item + ";";

                    using (DbDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            bool Checked = true;
                            string name = Convert.ToString(dataReader["name"]);
                            Permission perm = new Permission(name, false, item);

                            LBPermissions.Items.Add(perm);
                        }
                    }
                }
            }
        }

        // saving the permissions
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // a list for every checked permission
            List<int> checkedPermissionList = new List<int>();

            // adding the checked permissions to the list
            foreach (Permission permission in LBPermissions.Items)
            {
                if (permission.Checked) {
                    checkedPermissionList.Add(permission.Permission_id);
                }
            }

            // several checks for the user if certain permissions are checked but others are not
            if (checkedPermissionList.Contains(2) && !checkedPermissionList.Contains(1))
            {
                MessageBoxResult dialogResult = MessageBox.Show("Weet u zeker dat delicten kunnen worden gewijzigd, zonder dat deze kunnen worden ingezien?", "Permissies", MessageBoxButton.YesNo);
                if (dialogResult == MessageBoxResult.No)
                {
                    return;
                }
            }

            if (checkedPermissionList.Contains(18) && !checkedPermissionList.Contains(14))
            {
                MessageBoxResult dialogResult = MessageBox.Show("Weet u zeker dat u gebruikers kunnen worden aangepast, zonder dat deze kunnen worden ingezien?", "Permissies", MessageBoxButton.YesNo);
                if (dialogResult == MessageBoxResult.No)
                {
                    return;
                }
            }

            if ((checkedPermissionList.Contains(4) || checkedPermissionList.Contains(5)) && !checkedPermissionList.Contains(16))
            {
                MessageBoxResult dialogResult = MessageBox.Show("Weet u zeker dat u delicten kan archiveren/activeren, zonder dat de lijst kan worden getoond?", "Permissies", MessageBoxButton.YesNo);
                if (dialogResult == MessageBoxResult.No)
                {
                    return;
                }
            }

            string provider = ConfigurationManager.AppSettings["provider"];
            string connectionstring = ConfigurationManager.AppSettings["connectionString"];

            DbProviderFactory factory = DbProviderFactories.GetFactory(provider);

            //checking the current connection
            using (DbConnection connection = factory.CreateConnection())
            {
                if (connection == null)
                {
                    Console.WriteLine("connection Error");
                    Console.ReadLine();
                    return;
                }
                Console.WriteLine("connection geslaagd");

                connection.ConnectionString = connectionstring;
                connection.Open();
                DbCommand command = factory.CreateCommand();
                if (command == null)
                {
                    Console.WriteLine("geen command gegeven");
                    Console.ReadLine();
                    return;
                }

                Connection CN = new Connection();
                CN.OpenConection();
                // deleting all the current links from the database
                CN.ExecuteQueries("DELETE FROM [Wijkagent].[dbo].[permission_rol] where rol_id = " + role_id + ";");
                CN.CloseConnection();

                foreach (Permission permission in LBPermissions.Items)
                {
                    if (permission.Checked)
                    {

                        CN.OpenConection();
                        // inserting the new links into the database
                        CN.ExecuteQueries("INSERT INTO [Wijkagent].[dbo].[permission_rol](permission_id, rol_id)" +
                                " VALUES (" + permission.Permission_id + "," + role_id + ");");
                        CN.CloseConnection();
                    }
                }
                mw.ShowDialog("wijzigingen zijn opgeslagen");
            }
        }

        // the permission class
        public class Permission
        {
            public Permission(string name, bool checkedd, int id)
            {
                Name = name;
                Checked = checkedd;
                Permission_id = id;
            }
            public bool Checked
            {
                get;
                set;
            }
            public int Permission_id
            {
                get;
                set;
            }
            public string Name
            {
                get;
                set;
            }
        }



        private void Button_ClickR(object sender, RoutedEventArgs e)
        {
            Connection CN = new Connection();
            CN.OpenConection();
            CN.ExecuteQueries("INSERT INTO [Wijkagent].[dbo].[rol] (rol_name) values ('" + Rolbox.Text + "');");
            CN.CloseConnection();
            Rolbox.Clear();
            mw.ShowDialog("rol is toegevoegd");

        }
    }
}