
using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace MotoCare.Server
{
    /*
    Gestisce le sessioni degli utenti autenticati.
    Ogni sessione associa un token a un utente
    e rimane valida per un periodo limitato.
    */
    public static class Sessioni
    {
        /*
        Dizionario thread-safe che memorizza le sessioni.
        La chiave è il token, il valore è un oggetto Sessione.
        ConcurrentDictionary permette accessi contemporanei
        da parte di più client.
        */
        private static readonly ConcurrentDictionary<string, Sessione> sessioni =
            new ConcurrentDictionary<string, Sessione>();

        // Imposta la durata delle sessioni a due ore.
        private static readonly TimeSpan Durata =
            TimeSpan.FromHours(2);

        /*
        Record che contiene l'ID dell'utente
        e la data di scadenza della sessione in UTC.
        */
        private record Sessione(
            int IdUtente,
            DateTime ScadenzaUtc);

        /*
        Crea una nuova sessione per l'utente.

        RandomNumberGenerator genera 32 byte casuali sicuri.
        Convert.ToHexString li converte in un token
        esadecimale di 64 caratteri.
        */
        public static string Crea(int idUtente)
        {
            string token = Convert.ToHexString(
                RandomNumberGenerator.GetBytes(32));

            /*
            Memorizza il token nel dizionario, associandolo
            all'utente e a una scadenza di due ore.

            DateTime.UtcNow.Add(Durata) calcola
            la data e l'ora di scadenza.
            */
            sessioni[token] = new Sessione(
                idUtente,
                DateTime.UtcNow.Add(Durata));

            return token;
        }

        /*
        Verifica se un token è valido.
        Restituisce l'ID utente se la sessione è valida,
        altrimenti null.
        */
        public static int? Verifica(string? token)
        {
            // Rifiuta token null, vuoti o contenenti solo spazi.
            if (string.IsNullOrWhiteSpace(token))
                return null;

            /*
            Cerca il token nel dizionario.
            TryGetValue restituisce false se non esiste.
            out var sessione recupera i dati associati al token.
            */
            if (!sessioni.TryGetValue(token, out var sessione))
                return null;

            /*
            Controlla se la sessione è scaduta.
            Se l'orario attuale ha raggiunto la scadenza,
            il token viene eliminato dal dizionario.
            */
            if (DateTime.UtcNow >= sessione.ScadenzaUtc)
            {
                // out _ scarta il valore della sessione rimossa.
                sessioni.TryRemove(token, out _);
                return null;
            }

            // Restituisce l'ID dell'utente associato al token valido.
            return sessione.IdUtente;
        }

        /*
        Revoca una sessione, ad esempio durante il logout.
        Il token viene eliminato e non può più essere utilizzato.
        */
        public static void Revoca(string? token)
        {
            if (!string.IsNullOrWhiteSpace(token))
                sessioni.TryRemove(token, out _);
        }
    }
}
