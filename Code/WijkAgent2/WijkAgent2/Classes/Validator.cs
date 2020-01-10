using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WijkAgent2.Database;

namespace WijkAgent2.Classes
{
    class Validator
    {
        //Local values
        Connection cn;
        public int logged_in_user_id { get; set; }

        public bool validate(string permission)
        {
            //Open connection and retrieve permissions
            cn.OpenConection();
            SqlDataReader sq = cn.DataReader("select p.* " +
                                            "FROM [dbo].[User] us " +
                                            "JOIN rol r on us.rol_id = r.rol_id " +
                                            "JOIN permission_rol perrol on r.rol_id = perrol.rol_id " +
                                            "JOIN permission p on perrol.permission_id = p.permission_id " +
                                            "WHERE us.user_id = '" + logged_in_user_id + "'; "); ;
            bool found = false;
            while (sq.Read())
            {
                string t = (string)sq["act"];
                //Check if the person has permission and return TRUE
                if (t.Trim().Equals(permission))
                {
                    found = true;
                }
            }
            cn.CloseConnection();
            //Return FALSE if permission is not found
            return found;
        }

        public Validator()
        {
            cn = new Connection();
        }

    }
}
