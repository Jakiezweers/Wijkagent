using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wijkagent2.Classes
{
    class User
    {
        public int BadgeId;
        public string Name;
        public int PhoneNumber;
        public Unit Unit;
        public Roles Role;
        public Function Function;
        public Uploads ProfilePicture;

        public User(int badgeId, string name, int phoneNumber, Roles role)
        {
            BadgeId = badgeId;
            Name = name;
            PhoneNumber = phoneNumber;
            Role = role;
        }
    }
}
