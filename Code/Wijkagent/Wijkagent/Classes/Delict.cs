using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wijkagent.Classes
{
    class Delict
    {
        public int DelictId;
        public DateTime Date;
        public List<Person> PersonList = new List<Person>();
        public string Place;
        public string HouseNumber; //Double?
        public string ZipCode;
        public string Street;
        public string Description;
        public decimal Long;
        public decimal Lat;
        public int Status;
        public DateTime CreationDate;

        public Delict()
        {

        }
    }
}
