using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wijkagent.Classes
{
    class Unit
    {
        public int UnitId;
        public Base Base;
        public List<User> UsersInUnit = new List<User>();
        
        public Unit()
        {

        }

        public void AddUserToUnit(int badgeId)
        {

        }
    }
}
