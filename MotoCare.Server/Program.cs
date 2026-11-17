
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using MotoCare.Shared;
using MotoCare.Server;

/*
AVVIO DEL SERVER
*/

// Inizializza il database SQLite.
Database.TestConnessione();

/*
Crea un Server TCP sulla porta 5000.
IPAddress.Loopback corrisponde al localhost
server accetta solo connessione locale*/
TcpListener server = new TcpListener(
    IPAddress.Loopback, 5000);

server.Start();

Console.WriteLine("=== MOTO CARE SERVER ===");
Console.WriteLine("Server avviato sulla porta 5000.");

/*
ACCETTAZIONE DELLE CONNESSIONI

Il while mantiene il Server in esecuzione.
Ogni client viene gestito attraverso un metodo asincrono,
permettendo al Server di accettare più connessioni
contemporaneamente.
*/
while (true)
{
    // Attende la connessione di un client.
    TcpClient client = await server.AcceptTcpClientAsync();

    Console.WriteLine("Client connesso.");

    // Avvia la gestione del client senza attendere che termini.
    _ = GestisciClientAsync(client);
}

static async Task GestisciClientAsync(TcpClient client)
{
    /*
    Il blocco using garantisce che la connessione TCP
    venga chiusa e le risorse liberate al termine
    dell'elaborazione della richiesta.
    */
    using (client)
    {
        try
        {
            /*
            PREPARAZIONE DELLA COMUNICAZIONE

            NetworkStream è il flusso di dati TCP.

            AutoFlush = true invia immediatamente i dati
            scritti, senza attendere il riempimento del buffer.
            */
            using NetworkStream stream = client.GetStream();

            using StreamReader reader = new StreamReader(stream);//legge messaggi
            using StreamWriter writer = new StreamWriter(stream) //scrive risposte
            {
                AutoFlush = true
            };

            /*
            RICEZIONE RICHIESTA

            Il protocollo utilizzato prevede un messaggio
            JSON per ogni riga di testo.

            ReadLineAsync legge la richiesta fino
            al carattere di nuova riga
            */
            string? json = await reader.ReadLineAsync();

            // Se il client chiude la connessione, termina il metodo.
            if (json == null)
                return;

            /*
            DESERIALIZZAZIONE

            Converte la stringa JSON ricevuta in un oggetto
            della classe Richiesta, definita nel progetto Shared.
            */
            Richiesta? richiesta =
                JsonSerializer.Deserialize<Richiesta>(json);

            // risultato
            Risposta risposta;

            /*
            REGISTRAZIONE UTENTE
            Il Repository controlla i dati e tenta
            di inserire il nuovo utente nel database.

            Il metodo restituisce un valore booleano:
            true se registrazione riesce;
            false se no.
            */
            if (richiesta?.Azione == "REGISTRA")
            {
                bool registrato = UtentiRepository.Registra(
                    richiesta.Nome,
                    richiesta.Cognome,
                    richiesta.Email,
                    richiesta.Password
                );

                /*
                Costruisce la risposta da inviare al client.

                Successo contiene l'esito della registrazione.
                Messaggio contiene il testo corrispondente.

                ? : permette di scegliere
                il messaggio in base al valore di registrato.
                */
                risposta = new Risposta
                {
                    Successo = registrato,
                    Messaggio = registrato
                        ? "Registrazione completata!"
                        : "Registrazione non riuscita. Controlla i dati o l'email."
                };
            }

            /*
            LOGIN UTENTE

            Il Server verifica le credenziali ricevute.

            Se email e password sono corrette, recupera
            l'identificativo dell'utente dal database
            e genera un token di sessione.

            Il token permette di identificare l'utente
            nelle richieste successive senza inviare
            nuovamente email e password.
            */
            else if (richiesta?.Azione == "LOGIN")
            {
                // Verifica email e password.
                bool valido = UtentiRepository.VerificaLogin(
                    richiesta.Email,
                    richiesta.Password);

                /*
                Recupera l'ID solo se le credenziali
                sono state verificate correttamente.

                int? indica un intero nullable:
                può contenere un numero oppure null(per token)
                */
                int? idUtente = valido
                    ? UtentiRepository.TrovaIdUtente(richiesta.Email)
                    : null;

                // Controlla che le credenziali e l'ID siano validi.
                bool accessoConsentito = valido && idUtente.HasValue;

                /*
                Se il login riesce, Sessioni.Crea()
                genera un token associato all'utente.

                Se il login fallisce, il token è null.
                */
                risposta = new Risposta
                {
                    Successo = accessoConsentito,
                    Messaggio = accessoConsentito
                        ? "Accesso effettuato!"
                        : "Email o password errati.",
                    IdUtente = idUtente,
                    Token = accessoConsentito
                        ? Sessioni.Crea(idUtente!.Value)
                        : null
                };
            }

            /*
            REGISTRAZIONE MOTO
            Prima di registrare la moto,
            due controlli:
            1. Validità della sessione.
            2. Presenza dei dati della moto.

            Il Server determina il proprietario attraverso
            il token, evitando di utilizzare un ID utente
            eventualmente modificato dal client.
            */
            else if (richiesta?.Azione == "REGISTRA_MOTO")
            {
                // Recupera l'ID utente associato al token.
                int? idUtente = Sessioni.Verifica(richiesta.Token);

                // Sessione assente, non valida oppure scaduta.
                if (!idUtente.HasValue)
                {
                    risposta = new Risposta
                    {
                        Successo = false,
                        Messaggio = "Sessione non valida o scaduta."
                    };
                }

                // Il client non ha inviato un oggetto Moto.
                else if (richiesta.Moto == null)
                {
                    risposta = new Risposta
                    {
                        Successo = false,
                        Messaggio = "Dati della moto mancanti."
                    };
                }
                else
                {
                    
                    //Associa la moto all'utente autenticato.
                   
                    richiesta.Moto.IdUtente = idUtente.Value;

                    bool salvata = MotoRepository.RegistraMoto(
                        richiesta.Moto);

                    // Restituisce il risultato della registrazione.
                    risposta = new Risposta
                    {
                        Successo = salvata,
                        Messaggio = salvata
                            ? "Moto registrata correttamente!"
                            : "Registrazione non riuscita. Controlla i dati oppure verifica di non avere già una moto."
                    };
                }
            }

            /*
            RECUPERO DELLA MOTOCICLETTA

            Permette al client di ottenere i dati della moto
            associata all'utente attualmente autenticato.

            Il Server verifica il token e utilizza
            l'ID utente per cercare la moto nel database.

            Se la moto non esiste, viene restituito null.
            */
            else if (richiesta?.Azione == "OTTIENI_MOTO")
            {
                // Verifica la sessione dell'utente.
                int? idUtente = Sessioni.Verifica(richiesta.Token);

                if (!idUtente.HasValue)
                {
                    risposta = new Risposta
                    {
                        Successo = false,
                        Messaggio = "Sessione non valida o scaduta."
                    };
                }
                else
                {
                    // Cerca la moto appartenente all'utente.
                    Moto? moto = MotoRepository.TrovaMoto(idUtente.Value);

                                                 
                    risposta = new Risposta   //Restituisce l'oggetto Moto al client.
                    {
                        Successo = true,
                        Messaggio = moto == null
                            ? "Nessuna moto registrata."
                            : "Moto trovata.",
                        Moto = moto
                    };
                }
            }

            /*
            MODIFICA DELLA MOTOCICLETTA
            */

            else if (richiesta?.Azione == "MODIFICA_MOTO")
            {
                // Identifica l'utente tramite il token.
                int? idUtente = Sessioni.Verifica(richiesta.Token);

                // Impedisce modifiche senza una sessione valida.
                if (!idUtente.HasValue)
                {
                    risposta = new Risposta
                    {
                        Successo = false,
                        Messaggio = "Sessione non valida o scaduta."
                    };
                }

                // Controlla la presenza dei dati da aggiornare.
                else if (richiesta.Moto == null)
                {
                    risposta = new Risposta
                    {
                        Successo = false,
                        Messaggio = "Dati della moto mancanti."
                    };
                }
                else
                {
                    richiesta.Moto.IdUtente = idUtente.Value;

                    // Aggiorna i dati attraverso il Repository.
                    bool modificata = MotoRepository.ModificaMoto(
                        richiesta.Moto);

                    // Comunica al client l'esito della modifica.
                    risposta = new Risposta
                    {
                        Successo = modificata,
                        Messaggio = modificata
                            ? "Moto aggiornata correttamente!"
                            : "Modifica non riuscita."
                    };
                }
            }

            
            //AZIONE NON RICONOSCIUTA
            else
            {
                risposta = new Risposta
                {
                    Successo = false,
                    Messaggio = "Azione non riconosciuta."
                };
            }

            /*
            INVIO DELLA RISPOSTA

            L'oggetto Risposta viene serializzato in JSON.

            WriteLineAsync() invia il messaggio al client
         
            Il client può leggere la risposta
            utilizzando ReadLineAsync().
            */
            string jsonRisposta =
                JsonSerializer.Serialize(risposta);

            await writer.WriteLineAsync(jsonRisposta); //await serve ad aspettare il completamento di un'operazione asincrona senza bloccare inutilmente il thread

            Console.WriteLine(
                "Operazione: " + richiesta?.Azione);
        }

        catch (Exception ex)
        {
            Console.WriteLine(
                "Errore client: " + ex.Message);
        }
    }
}
