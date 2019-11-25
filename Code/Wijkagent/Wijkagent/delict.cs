using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wijkagent
{
    class Delict
    {
        public int id { get; set; }
        public DateTime datetime1 { get; set; }
        public string plaats { get; set; }
        public int huisnummer { get; set; }
        public string zip { get; set; }
        public string street { get; set; }
        public StringBuilder description { get; set; }
        public decimal longitude { get; set; }
        public decimal lat { get; set; }
        public int status { get; set; }
        public DateTime createtime { get; set; }


    }
}
