using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public string PhoneNumber { get; set; }
        internal Unit Unit { get; set; }
        public Roles Role { get; set; }
        public string Rolenamel { get; set; }
        public List<string> Rolenamelist = new List<string>();
        internal Function Function { get; set; }
        public Uploads ProfilePicture { get; set; }

        public ObservableCollection<Roles> Roleslist = new ObservableCollection<Roles>() {
            new Roles() { rol_id = 1, RoleName = "admin" },
            new Roles() { rol_id = 2, RoleName = "politie" },
            new Roles() { rol_id = 3, RoleName = "wijkagent" } };

        public List<Permissions> permission_list { get; set; }
        public User()
        {

        }

        public User(int badgeId, string name, string phoneNumber, Roles role)
        {
            BadgeId = badgeId;
            Name = name;
            PhoneNumber = phoneNumber;
            Role = role; 
            
            Rolenamel = Role.RoleName;
        }
    }
}
