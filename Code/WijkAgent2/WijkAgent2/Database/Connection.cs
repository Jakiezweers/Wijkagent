using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WijkAgent2.Database
{
    class Connection
    {

        string provider = ConfigurationManager.AppSettings["provider"];
        string connectionstring = ConfigurationManager.AppSettings["connectionString"];
        SqlConnection con;

        public Connection()
        {

        }

        public void OpenConection()
        {
            con = new SqlConnection(connectionstring);
            con.Open();
        }

        public SqlConnection GetConnection()
        {
            return con;
        }


        public void CloseConnection()
        {
            con.Close();
        }

        public void ExecuteQueries(string Query_)
        {
            SqlCommand cmd = new SqlCommand(Query_, con);
            cmd.ExecuteNonQuery();

        }


        public SqlDataReader DataReader(string Query_)
        {
            SqlCommand cmd = new SqlCommand(Query_, con);
            SqlDataReader dr = cmd.ExecuteReader();
            return dr;
        }

        /*

        public object ShowDataInGridView(string Query_)
        {
            SqlDataAdapter dr = new SqlDataAdapter(Query_, connectionstring);
            DataSet ds = new DataSet();
            dr.Fill(ds);
            object dataum = ds.Tables[0];
            return dataum;
        }

        */
    }
}
