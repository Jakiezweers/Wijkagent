using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wijkagent2.Classes
{
    class Roles
    {
        public string RoleName { get; set; }
        internal List<Permissions> PermissionsList { get; set; } = new List<Permissions>();

        public int rol_id { get; set; }

        public Roles(string roleName, int rol_id)
        {
            RoleName = roleName;
            this.rol_id = rol_id;
        }

        public Roles()
        {

        }

        public override string ToString()
        {
            // Generates the text shown in the combo box
            return RoleName;
        }
    }
}
