using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WijkAgent2.Classes
{
    class functie
    {
        public string FunctieName { get; set; }

        public int functie_id { get; set; }

        public functie(string functieName, int functie_id)
        {
            FunctieName = functieName;
            this.functie_id = functie_id;
        }

        public functie()
        {

        }
        public override string ToString()
        {
            // Generates the text shown in the combo box
            return FunctieName;
        }
    }
}
