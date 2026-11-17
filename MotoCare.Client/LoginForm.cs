
using System;
using System.Drawing;        
using System.Windows.Forms;    
using System.Net.Sockets;     
using System.IO;               
using System.Text.Json;       
using MotoCare.Shared;        

namespace MotoCare.Client
{
   
    public partial class LoginForm : Form
    {
      
        private TextBox txtEmail = null!;
        private TextBox txtPassword = null!;
        private Button btnAccedi = null!;
        private Button btnRegistrati = null!;

       
        private readonly Color arancione = Color.FromArgb(255, 102, 0);
        private readonly Color sfondo = Color.FromArgb(21, 21, 21);
        private readonly Color pannello = Color.FromArgb(36, 36, 36);

    
        public LoginForm()
        {
           
            InitializeComponent();

           
            CreaInterfaccia();
        }

       
        private void CreaInterfaccia()
        {
           
            Text = "MotoCare - Login";                      
            ClientSize = new Size(500, 580);                
            BackColor = sfondo;                            
            FormBorderStyle = FormBorderStyle.FixedSingle;  
            MaximizeBox = false;                           
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 10);              

           
            Label logo = new Label
            {
                Text = "MOTOCARE",                         
                ForeColor = arancione,                  
                Font = new Font("Segoe UI", 23, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter, 
                Location = new Point(50, 35),             
                Size = new Size(400, 60)                  
            };

          
            Panel card = new Panel
            {
                BackColor = pannello,              
                Location = new Point(35, 120),   
                Size = new Size(430, 410)          
            };

         
            Label titolo = new Label
            {
                Text = "ACCEDI AL TUO ACCOUNT",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 17, FontStyle.Bold),
                Location = new Point(30, 30),
                AutoSize = true                    
            };

            Label lblEmail = new Label
            {
                Text = "EMAIL",
                ForeColor = Color.LightGray,
                Location = new Point(30, 105),
                AutoSize = true
            };

         
            txtEmail = new TextBox
            {
                Location = new Point(30, 130),    
                Size = new Size(370, 30),        
                BackColor = Color.FromArgb(54, 54, 54),
                ForeColor = Color.White,       
                BorderStyle = BorderStyle.FixedSingle 
            };

           
            Label lblPassword = new Label
            {
                Text = "PASSWORD",
                ForeColor = Color.LightGray,
                Location = new Point(30, 185),
                AutoSize = true
            };

         
            txtPassword = new TextBox
            {
                Location = new Point(30, 210),
                Size = new Size(370, 30),
                BackColor = Color.FromArgb(54, 54, 54),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,

           
                UseSystemPasswordChar = true
            };

         
            btnAccedi = new Button
            {
                Text = "ACCEDI",
                Location = new Point(30, 275),
                Size = new Size(370, 42),
                BackColor = arancione,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

           
            btnAccedi.FlatAppearance.BorderSize = 0;

          
            btnRegistrati = new Button
            {
                Text = "CREA ACCOUNT",
                Location = new Point(30, 330),
                Size = new Size(370, 38),
                BackColor = pannello,
                ForeColor = arancione,
                FlatStyle = FlatStyle.Flat
            };

        
            btnRegistrati.FlatAppearance.BorderColor = arancione;

         
            btnAccedi.Click += BtnAccedi_Click;

      
            btnRegistrati.Click += BtnRegistrati_Click;

            card.Controls.AddRange(new Control[]
            {
                titolo, lblEmail, txtEmail,
                lblPassword, txtPassword,
                btnAccedi, btnRegistrati
            });

        
            Controls.Add(logo);
            Controls.Add(card);
        }


        
        // "async" permette di eseguire operazioni che richiedono tempo, come la connessione al Server,
        // senza bloccare completamente l'interfaccia grafica.
        // "void" indica che il metodo non restituisce un valore.
        private async void BtnAccedi_Click(object? sender, EventArgs e)
        {
            // Recuperiamo il testo inserito dall'utente nella casella Email.
            // Trim() elimina gli spazi presenti all'inizio e alla fine.
            string email = txtEmail.Text.Trim();

           
            string password = txtPassword.Text;

           
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
               
                MessageBox.Show("Inserisci email e password.");

             
                return;
            }

            // Disabilitiamo temporaneamente il pulsante per evitare più richieste contemporaneamente.
            btnAccedi.Enabled = false;

            try
            {
                // Creiamo un oggetto TcpClient, che permette al Client di collegarsi al Server.
              
                using TcpClient client = new TcpClient();

              
                using CancellationTokenSource timeout =
                    new CancellationTokenSource(TimeSpan.FromSeconds(10));

              
                await client.ConnectAsync(
                    "127.0.0.1", 5000, timeout.Token);

               
                using NetworkStream stream = client.GetStream();

                using StreamReader reader = new StreamReader(stream);

           
                using StreamWriter writer = new StreamWriter(stream)
                {
                  
                    AutoFlush = true
                };

            
                Richiesta richiesta = new Richiesta
                {
                 
                    Azione = "LOGIN",

                   
                    Email = email,

                 
                    Password = password
                };

              
                string json = JsonSerializer.Serialize(richiesta);

                await writer.WriteLineAsync(json);

               
                string? jsonRisposta =
                    await reader.ReadLineAsync(timeout.Token);

           
                if (jsonRisposta == null)
                    throw new IOException("Nessuna risposta dal Server.");

        
                Risposta? risposta =
                    JsonSerializer.Deserialize<Risposta>(jsonRisposta);

              
                if (risposta == null)
                    throw new IOException("Risposta del Server non valida.");


                // Controlliamo se il Login è andato a buon fine.
                // Successo deve essere true.
                // IdUtente deve contenere un valore.
                // Token non deve essere vuoto.
                if (risposta.Successo &&
                    risposta.IdUtente.HasValue &&
                    !string.IsNullOrWhiteSpace(risposta.Token))
                {
                   
                    int idUtente = risposta.IdUtente.Value;

                    // Salviamo il token di autenticazione restituito dal Server.
                    // Il token servirà per identificare l'utente nelle richieste successive.
                    string token = risposta.Token;

                   
                    Hide();

                    try
                    {
                       
                        using DashboardForm dashboard =
                            new DashboardForm(idUtente, token);

                      
                        dashboard.ShowDialog();
                    }
                    finally
                    {
                       
                        txtPassword.Clear();

                       
                        Show();
                    }
                }

                else
                {
                   
                    MessageBox.Show(risposta.Messaggio);
                }
            }
            catch (Exception ex)
            {
               
                MessageBox.Show(
                    "Errore di connessione: " + ex.Message);
            }
            finally
            {
               
                btnAccedi.Enabled = true;
            }
        }


        
        private void BtnRegistrati_Click(object? sender, EventArgs e)
        {
            
            using RegistrazioneForm registrazione = new RegistrazioneForm();

         
            registrazione.ShowDialog(this);
        }
    }
}
