using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WijkAgent2.Classes
{
    public class CategoryList
    {
        //Categorylist class for constructing the category dropdown. Category is found in the database, same goes for the category name. 
        //The check status is checking if its checked or not. It saves the state of checked categories bound to delicts in the database.
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
