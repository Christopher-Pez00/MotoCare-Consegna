using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


//Server comunica risultato

namespace MotoCare.Shared
{
    public class Risposta
    {
        public bool Successo { get; set; }
        public string Messaggio { get; set; } = "";
        public int? IdUtente { get; set; }

        public string? Token { get; set; }
        public Moto? Moto { get; set; }
    }
}