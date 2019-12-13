using System;
using System.Text;

namespace Wijkagent2.Classes
{
    class Delict: Person
    {

        public int id { get; set; }
        public string datetime1 { get; set; }
        public string plaats { get; set; }
        public int huisnummer { get; set; }
        public string zip { get; set; }
        public string street { get; set; }
        public StringBuilder description { get; set; }
        public decimal longitude { get; set; }
        public decimal lat { get; set; }
        public int status { get; set; }
        
        public string person { get; set; }
        public string category { get; set; }
        public DateTime createtime { get; set; }
        public int changedBy { get; set; }
        public DateTime addedDate { get; set; }


    }
}
