using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WijkAgent2.Classes
{
    public class CategoryList
    {
        public CategoryList(int id, string name)
        {
            Category_ID = id;
            Category_Name = name;
        }
        public int Category_ID
        {
            get;
            set;
        }
        public string Category_Name
        {
            get;
            set;
        }
        public Boolean Check_Status
        {
            get;
            set;
        }
    }
}
