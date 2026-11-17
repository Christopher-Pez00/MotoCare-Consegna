using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//Client invia operazione e dati

namespace MotoCare.Shared
{
    public class Richiesta
    {
        public string Azione { get; set; } = "";
        public string Nome { get; set; } = "";
        public string Cognome { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";

        public Moto? Moto { get; set; }

        public string? Token { get; set; }
    }
}