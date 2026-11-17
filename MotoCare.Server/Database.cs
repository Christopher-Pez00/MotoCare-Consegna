
using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace MotoCare.Server
{
    public static class Database
    {
        private static readonly string dbPath =

            Path.Combine(

                // Restituisce la cartella in cui si trova l'applicazione.
                AppContext.BaseDirectory,

                "MotoCare.db"
            );

        /*
        Dichiara la stringa di connessione al database.
        */
        private static readonly string connectionString =
            $"Data Source={dbPath}";

        public static void TestConnessione()
        {
            try
            {
                using SqliteConnection conn =

                    new SqliteConnection(connectionString);

                conn.Open();

                Console.WriteLine("Connessione SQLite riuscita!");

                // Richiama il metodo che crea le tabelle mancanti.
                // Passa come parametro la connessione già aperta.
                CreaTabelle(conn);

                Console.WriteLine("Tabelle verificate correttamente!");

                Console.WriteLine("Database: " + dbPath);
            }

            catch (Exception ex)
            {
                Console.WriteLine("Errore SQLite: " + ex.Message);

                throw;
            }
        }

        private static void CreaTabelle(SqliteConnection conn)
        {
            string sql = """

                -- Crea la tabella Utenti soltanto se non esiste.
                CREATE TABLE IF NOT EXISTS Utenti (

                    -- Identificativo univoco dell'utente.
                    -- INTEGER: numero intero.
                    -- PRIMARY KEY: chiave primaria.
                    -- AUTOINCREMENT: generazione automatica dell'ID.
                    IdUtente INTEGER PRIMARY KEY AUTOINCREMENT,

                    -- Nome dell'utente, obbligatorio.
                    Nome TEXT NOT NULL,

                    -- Cognome dell'utente, obbligatorio.
                    Cognome TEXT NOT NULL,

                    -- Email obbligatoria e non duplicabile.
                    Email TEXT NOT NULL UNIQUE,

                    -- Hash della password, obbligatorio.
                    PasswordHash TEXT NOT NULL,

                    -- Data di registrazione obbligatoria.
                    DataRegistrazione TEXT NOT NULL

                        -- Inserisce automaticamente data e ora correnti.
                        DEFAULT CURRENT_TIMESTAMP
                );

                -- Crea la tabella delle motociclette.
                CREATE TABLE IF NOT EXISTS Moto (

                    -- Identificativo univoco della moto.
                    IdMoto INTEGER PRIMARY KEY AUTOINCREMENT,

                    -- Identificativo del proprietario.
                    -- NOT NULL: obbligatorio.
                    -- UNIQUE: ogni utente può avere una sola moto.
                    IdUtente INTEGER NOT NULL UNIQUE,

                    -- Marca della motocicletta, obbligatoria.
                    Marca TEXT NOT NULL,

                    -- Modello della motocicletta, obbligatorio.
                    Modello TEXT NOT NULL,

                    -- Anno della moto, facoltativo.
                    Anno INTEGER,

                    -- Cilindrata della moto.
                    -- CHECK richiede un valore maggiore di zero, se presente.
                    Cilindrata INTEGER CHECK (Cilindrata > 0),

                    -- Targa della moto, facoltativa.
                    Targa TEXT,

                    -- Chilometraggio obbligatorio, inizialmente zero.
                    Chilometraggio INTEGER NOT NULL DEFAULT 0

                        -- Impedisce chilometraggi negativi.
                        CHECK (Chilometraggio >= 0),

                    -- Data di inserimento della motocicletta.
                    DataInserimento TEXT NOT NULL

                        -- Assegna automaticamente data e ora correnti.
                        DEFAULT CURRENT_TIMESTAMP,

                    -- Definisce IdUtente come chiave esterna.
                    FOREIGN KEY (IdUtente)

                        -- Collega la moto all'ID della tabella Utenti.
                        REFERENCES Utenti(IdUtente)
                );

                -- Crea la tabella per registrare gli interventi di manutenzione.
                CREATE TABLE IF NOT EXISTS Manutenzioni (

                    -- Identificativo univoco della manutenzione.
                    IdManutenzione INTEGER PRIMARY KEY AUTOINCREMENT,

                    -- Identificativo della moto interessata.
                    IdMoto INTEGER NOT NULL,

                    -- Tipo di manutenzione, obbligatorio.
                    Tipo TEXT NOT NULL,

                    -- Descrizione aggiuntiva, facoltativa.
                    Descrizione TEXT,

                    -- Data in cui è stato effettuato l'intervento.
                    DataIntervento TEXT NOT NULL,

                    -- Chilometraggio della moto al momento dell'intervento.
                    Chilometraggio INTEGER

                        -- Impedisce valori negativi, se presenti.
                        CHECK (Chilometraggio >= 0),

                    -- Costo dell'intervento, facoltativo.
                    Costo NUMERIC CHECK (Costo >= 0),

                    -- Data di registrazione della manutenzione.
                    DataRegistrazione TEXT NOT NULL

                        -- Inserisce automaticamente data e ora correnti.
                        DEFAULT CURRENT_TIMESTAMP,

                    -- Definisce IdMoto come chiave esterna.
                    FOREIGN KEY (IdMoto)

                        -- Collega la manutenzione alla tabella Moto.
                        REFERENCES Moto(IdMoto)
                );

                -- Crea la tabella delle scadenze.
                CREATE TABLE IF NOT EXISTS Scadenze (

                    -- Identificativo univoco della scadenza.
                    IdScadenza INTEGER PRIMARY KEY AUTOINCREMENT,

                    -- Identificativo della moto associata.
                    IdMoto INTEGER NOT NULL,

                    -- Tipo di scadenza, obbligatorio.
                    Tipo TEXT NOT NULL,

                    -- Descrizione aggiuntiva, facoltativa.
                    Descrizione TEXT,

                    -- Data entro cui deve essere gestita la scadenza.
                    DataScadenza TEXT NOT NULL,

                    -- Stato della scadenza.
                    -- 0 = non completata.
                    -- 1 = completata.
                    -- Il valore predefinito è 0.
                    Completata INTEGER NOT NULL DEFAULT 0

                        -- Accetta esclusivamente i valori 0 e 1.
                        CHECK (Completata IN (0, 1)),

                    -- Data di registrazione della scadenza.
                    DataRegistrazione TEXT NOT NULL

                        -- Inserisce automaticamente data e ora correnti.
                        DEFAULT CURRENT_TIMESTAMP,

                    -- Definisce IdMoto come chiave esterna.
                    FOREIGN KEY (IdMoto)

                        -- Collega la scadenza alla tabella Moto.
                        REFERENCES Moto(IdMoto)
                );

                -- Crea la tabella per memorizzare le spese.
                CREATE TABLE IF NOT EXISTS Spese (

                    -- Identificativo univoco della spesa.
                    IdSpesa INTEGER PRIMARY KEY AUTOINCREMENT,

                    -- Identificativo della moto associata.
                    IdMoto INTEGER NOT NULL,

                    -- Categoria della spesa, obbligatoria.
                    Categoria TEXT NOT NULL,

                    -- Descrizione aggiuntiva, facoltativa.
                    Descrizione TEXT,

                    -- Importo della spesa, obbligatorio.
                    Importo NUMERIC NOT NULL

                        -- Impedisce l'inserimento di importi negativi.
                        CHECK (Importo >= 0),

                    -- Data in cui è stata sostenuta la spesa.
                    DataSpesa TEXT NOT NULL,

                    -- Data di registrazione della spesa nel sistema.
                    DataRegistrazione TEXT NOT NULL

                        -- Inserisce automaticamente data e ora correnti.
                        DEFAULT CURRENT_TIMESTAMP,

                    -- Definisce IdMoto come chiave esterna.
                    FOREIGN KEY (IdMoto)

                        -- Collega la spesa alla tabella Moto.
                        REFERENCES Moto(IdMoto)
                );
                """;

            /*
            Crea un comando SQLite chiamato cmd.
            Il primo parametro contiene le istruzioni SQL.
            Il secondo parametro indica la connessione da utilizzare.
            */
            using SqliteCommand cmd =
                new SqliteCommand(sql, conn);

            /*
            ExecuteNonQuery() viene utilizzato per comandi SQL
            che non devono restituire righe di risultati.
            */
            cmd.ExecuteNonQuery();

            /*

            PRAGMA foreign_keys = ON abilita il controllo
            dell'integrità referenziale sulla connessione corrente.

            Questo controllo impedisce, per esempio,
            di associare una manutenzione a una moto inesistente.
            */
            using SqliteCommand fkCmd =
                new SqliteCommand(
                    "PRAGMA foreign_keys = ON;", conn);

            // Esegue il comando per abilitare le chiavi esterne.
            fkCmd.ExecuteNonQuery();
        }
    }
}
