using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wijkagent2.Classes
{
    class User
    {
        private int userId;

        public int UserId { get => userId; set => userId = value; }
        public int BadgeId { get; set; }
        public string Name { get; set; }
        public int PhoneNumber { get; set; }
        internal Unit Unit { get; set; }
        internal Roles Role { get; set; }
        internal Function Function { get; set; }
        internal Uploads ProfilePicture { get; set; }

        public User()
        {

        }

        public User(int badgeId, string name, int phoneNumber, Roles role)
        {
            BadgeId = badgeId;
            Name = name;
            PhoneNumber = phoneNumber;
            Role = role;
        }
    }
}
