using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wijkagent2.Classes
{
    class Eenheid
    {
        public string EenheidNaam { get; set; }

        public int eenheid_id { get; set; }

        public Eenheid(string eenheidNaam, int eenheid_id)
        {
            EenheidNaam = eenheidNaam;
            this.eenheid_id = eenheid_id;
        }

        public Eenheid()
        {

        }

        public override string ToString()
        {
            // Generates the text shown in the combo box
            return EenheidNaam;
        }
    }
}
