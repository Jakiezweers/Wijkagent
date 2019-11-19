using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wijkagent.Classes
{
    class Roles
    {
        public string RoleName;
        public List<Permissions> PermissionsList = new List<Permissions>();

        public Roles()
        {

        }
    }
}
