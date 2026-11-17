
using Microsoft.Data.Sqlite;
using MotoCare.Shared;
namespace MotoCare.Server
{
    /*
    CLASSE MotoRepository

    Gestisce le operazioni sul database relative alle motociclette.

    Le operazioni disponibili sono:
    - RegistraMoto: inserisce una nuova moto.
    - TrovaMoto: recupera la moto di un utente.
    - ModificaMoto: aggiorna i dati di una moto esistente.

    */
    public static class MotoRepository
    {
        /*
        METODO RegistraMoto

        Riceve un oggetto Moto contenente i dati da salvare.
        Verifica che i dati siano validi e successivamente
        esegue un'istruzione INSERT nel database.

        Restituisce:
        true = motocicletta registrata correttamente.
        false = dati non validi oppure violazione di un vincolo SQL.
        */
        public static bool RegistraMoto(Moto moto)
        {
            /*
            CONTROLLI SUI DATI
            */
            if (
                // L'identificativo dell'utente deve essere positivo.
                moto.IdUtente <= 0 ||

                string.IsNullOrWhiteSpace(moto.Marca) ||

                string.IsNullOrWhiteSpace(moto.Modello) ||

                moto.Anno < 1885 ||

                moto.Anno > DateTime.Now.Year + 1 ||

                moto.Cilindrata < 0 ||

                moto.Chilometraggio < 0)
            {
                // Interrompe il metodo se almeno un dato non è valido.
                return false;
            }

            using var conn = new SqliteConnection(
                "Data Source=MotoCare.db;Foreign Keys=True");

            conn.Open();

            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
                INSERT INTO Moto
                (IdUtente, Marca, Modello, Anno,
                 Cilindrata, Targa, Chilometraggio)
                VALUES
                ($idUtente, $marca, $modello, $anno,
                 $cilindrata, $targa, $chilometraggio)";

            /*
            ASSOCIAZIONE DEI PARAMETRI SQL

            AddWithValue associa un valore C# a un parametro SQL.
            In questo modo SQLite utilizza i valori indicati
            quando esegue l'istruzione INSERT.
            */

            // Associa l'ID del proprietario al parametro $idUtente.
            cmd.Parameters.AddWithValue(
                "$idUtente", moto.IdUtente);

            // Associa la marca, eliminando gli spazi iniziali e finali.
            cmd.Parameters.AddWithValue(
                "$marca", moto.Marca.Trim());

            cmd.Parameters.AddWithValue(
                "$modello", moto.Modello.Trim());

            cmd.Parameters.AddWithValue(
                "$anno", moto.Anno);

            cmd.Parameters.AddWithValue(
                "$cilindrata", moto.Cilindrata);

            cmd.Parameters.AddWithValue(
                "$targa", moto.Targa.Trim());

            cmd.Parameters.AddWithValue(
                "$chilometraggio", moto.Chilometraggio);

            /*
            ESECUZIONE DELL'INSERT

            ExecuteNonQuery() esegue l'INSERT e restituisce
            il numero di righe interessate dall'operazione.

            */
            try
            {
                return cmd.ExecuteNonQuery() == 1;
            }

            catch (SqliteException ex)
                when (ex.SqliteErrorCode == 19)
            {
                return false;
            }
        }
        public static Moto? TrovaMoto(int idUtente)
        {
            using var conn = new SqliteConnection(
                "Data Source=MotoCare.db;Foreign Keys=True");

            conn.Open();

            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
        SELECT IdMoto, IdUtente, Marca, Modello,
               Anno, Targa, Cilindrata, Chilometraggio
        FROM Moto
        WHERE IdUtente = $idUtente";

            // Assegna al parametro SQL l'ID dell'utente da cercare.
            cmd.Parameters.AddWithValue("$idUtente", idUtente);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())

                // Nessuna moto trovata per l'utente.
                return null;
            return new Moto
            {
                IdMoto = reader.GetInt32(0),

                IdUtente = reader.GetInt32(1),

                Marca = reader.GetString(2),

                Modello = reader.GetString(3),

                Anno = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),

                Targa = reader.IsDBNull(5) ? "" : reader.GetString(5),

                Cilindrata = reader.IsDBNull(6) ? 0 : reader.GetInt32(6),

                Chilometraggio = reader.GetInt32(7)
            };
        }

        public static bool ModificaMoto(Moto moto)
        {
            /*
            VALIDAZIONE DEI DATI

            Controlla che l'utente sia valido,
            che marca e modello siano presenti
            e che i valori numerici rispettino
            gli intervalli previsti.
            */
            if (
                moto.IdUtente <= 0 ||

                string.IsNullOrWhiteSpace(moto.Marca) ||

                string.IsNullOrWhiteSpace(moto.Modello) ||

                moto.Anno < 1885 ||

                moto.Anno > DateTime.Now.Year + 1 ||

                moto.Cilindrata < 0 ||

                moto.Chilometraggio < 0)
            {
                // Interrompe l'aggiornamento se un controllo fallisce.
                return false;
            }

            using var conn = new SqliteConnection(
                "Data Source=MotoCare.db;Foreign Keys=True");

            conn.Open();

            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
        UPDATE Moto
        SET Marca = $marca,
            Modello = $modello,
            Anno = $anno,
            Cilindrata = $cilindrata,
            Targa = $targa,
            Chilometraggio = $km
        WHERE IdUtente = $idUtente";

            // Assegna il nuovo valore della marca.
            cmd.Parameters.AddWithValue("$marca", moto.Marca.Trim());

            cmd.Parameters.AddWithValue("$modello", moto.Modello.Trim());

            cmd.Parameters.AddWithValue("$anno", moto.Anno);

            cmd.Parameters.AddWithValue("$cilindrata", moto.Cilindrata);

            cmd.Parameters.AddWithValue("$targa", moto.Targa.Trim());

            cmd.Parameters.AddWithValue("$km", moto.Chilometraggio);

            cmd.Parameters.AddWithValue("$idUtente", moto.IdUtente);

            /*
            ESECUZIONE DELL'UPDATE

            ExecuteNonQuery() esegue l'istruzione SQL
            e restituisce il numero di righe interessate.

            Il metodo restituisce true se viene aggiornata
            esattamente una riga, altrimenti false.
            */
            return cmd.ExecuteNonQuery() == 1;
        }
    }
}
