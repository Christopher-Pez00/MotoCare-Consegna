
using System;
using System.Security.Cryptography;
using Microsoft.Data.Sqlite;

namespace MotoCare.Server
{
    public static class UtentiRepository
    {
        // Verifica le credenziali dell'utente confrontando la password inserita con l'hash salvato.
        public static bool VerificaLogin(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
                return false;

            // Normalizza l'email eliminando spazi e differenze tra maiuscole e minuscole.
            email = email.Trim().ToLowerInvariant();

            using var conn = new Microsoft.Data.Sqlite.SqliteConnection(
                "Data Source=MotoCare.db");

            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText =
                "SELECT PasswordHash FROM Utenti WHERE Email = $email";

            // Utilizza un parametro SQL per evitare SQL Injection.
            cmd.Parameters.AddWithValue("$email", email);

            // ExecuteScalar recupera il primo valore restituito dalla query.
            string? passwordSalvata = cmd.ExecuteScalar() as string;

            if (passwordSalvata == null)
                return false;

            // Divide la stringa salvata in algoritmo, iterazioni, salt e hash.
            string[] parti = passwordSalvata.Split(':');

            // Controlla il formato dell'hash e converte il numero di iterazioni in intero.
            if (parti.Length != 4 ||
                parti[0] != "pbkdf2-sha256" ||
                !int.TryParse(parti[1], out int iterazioni) ||
                iterazioni < 1)
                return false;

            try
            {
                // Converte salt e hash dal formato Base64 in array di byte.
                byte[] salt = Convert.FromBase64String(parti[2]);
                byte[] hashSalvato = Convert.FromBase64String(parti[3]);

                // Ricalcola l'hash della password inserita usando PBKDF2-SHA256 e gli stessi parametri.
                byte[] hashInserito =
                    System.Security.Cryptography.Rfc2898DeriveBytes.Pbkdf2(
                        password,
                        salt,
                        iterazioni,
                        System.Security.Cryptography.HashAlgorithmName.SHA256,
                        hashSalvato.Length);

                // Confronta i due hash in tempo costante rispetto al contenuto.
                return System.Security.Cryptography.CryptographicOperations
                    .FixedTimeEquals(hashInserito, hashSalvato);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        // Registra un nuovo utente dopo aver validato i dati e protetto la password.
        public static bool Registra(
            string nome,
            string cognome,
            string email,
            string password)
        {
            nome = nome.Trim();
            cognome = cognome.Trim();
            email = email.Trim().ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(nome) ||
                string.IsNullOrWhiteSpace(cognome) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            if (password.Length < 8)
                return false;

            // Genera 16 byte casuali di salt per rendere diversi gli hash di password uguali.
            byte[] salt = RandomNumberGenerator.GetBytes(16);

            // Genera un hash di 32 byte con PBKDF2-SHA256 usando 600000 iterazioni.
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                600000,
                HashAlgorithmName.SHA256,
                32
            );

            // Memorizza algoritmo, iterazioni, salt e hash senza salvare la password originale.
            string passwordHash =
                "pbkdf2-sha256:600000:" +
                Convert.ToBase64String(salt) + ":" +
                Convert.ToBase64String(hash);

            // Costruisce il percorso del database nella cartella dell'applicazione.
            string dbPath = System.IO.Path.Combine(
                AppContext.BaseDirectory,
                "MotoCare.db"
            );

            using SqliteConnection conn =
                new SqliteConnection($"Data Source={dbPath}");

            conn.Open();

            using SqliteCommand cmd = conn.CreateCommand();

            // Prepara l'inserimento dell'utente utilizzando parametri SQL.
            cmd.CommandText = """
                INSERT INTO Utenti
                    (Nome, Cognome, Email, PasswordHash)
                VALUES
                    (@nome, @cognome, @email, @hash);
                """;

            cmd.Parameters.AddWithValue("@nome", nome);
            cmd.Parameters.AddWithValue("@cognome", cognome);
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@hash", passwordHash);

            try
            {
                // ExecuteNonQuery restituisce il numero di righe inserite.
                return cmd.ExecuteNonQuery() == 1;
            }
            // L'errore SQLite 19 indica una violazione di vincolo, ad esempio un'email duplicata.
            catch (SqliteException ex)
                when (ex.SqliteErrorCode == 19)
            {
                // Email già registrata o altro vincolo violato
                return false;
            }
        }

        // Recupera l'ID dell'utente tramite email, restituendo null se non esiste.
        public static int? TrovaIdUtente(string email)
        {
            using var conn = new Microsoft.Data.Sqlite.SqliteConnection(
                "Data Source=MotoCare.db");

            conn.Open();

            using var cmd = conn.CreateCommand();

            cmd.CommandText =
                "SELECT IdUtente FROM Utenti WHERE Email = $email";

            cmd.Parameters.AddWithValue(
                "$email", email.Trim().ToLowerInvariant());

            // Recupera il primo valore della query, oppure null se non esiste.
            object? risultato = cmd.ExecuteScalar();

            // L'operatore ternario restituisce null oppure converte il risultato in intero.
            return risultato == null
                ? null
                : Convert.ToInt32(risultato);
        }
    }
}
