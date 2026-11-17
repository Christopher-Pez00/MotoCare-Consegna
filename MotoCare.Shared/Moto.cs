using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MotoCare.Shared
{
    public class Moto
    {
        public int IdMoto { get; set; }
        public int IdUtente { get; set; }

        public string Marca { get; set; } = "";
        public string Modello { get; set; } = "";
        public int Anno { get; set; }
        public string Targa { get; set; } = "";
        public int Cilindrata { get; set; }
        public int Chilometraggio { get; set; }
    }
}
