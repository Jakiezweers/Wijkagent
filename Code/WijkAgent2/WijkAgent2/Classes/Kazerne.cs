using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wijkagent2.Classes
{
    class Kazerne
    {
        public string KazerneNaam { get; set; }

        public int kazerne_id { get; set; }

        public Kazerne(string kazerneNaam, int kazerne_id)
        {
            KazerneNaam = kazerneNaam;
            this.kazerne_id = kazerne_id;
        }

        public Kazerne()
        {

        }

        public override string ToString()
        {
            // Generates the text shown in the combo box
            return KazerneNaam;
        }
    }
}
