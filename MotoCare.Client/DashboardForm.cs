
using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Net.Sockets;
using System.Text.Json;
using MotoCare.Shared;


namespace MotoCare.Client
{
   
    public partial class DashboardForm : Form
    {
       
        private readonly Color sfondo = Color.FromArgb(21, 21, 21);
        private readonly Color pannello = Color.FromArgb(36, 36, 36);
        private readonly Color arancione = Color.FromArgb(255, 102, 0);

        // ID dell'utente che ha effettuato il login.
        private readonly int idUtente;

        // Token che identifica la sessione dell'utente.
        // Viene utilizzato per comunicare con il Server.
        private readonly string token;

      
        private Label lblMoto = null!;
        private Label lblKm = null!;


      
        public DashboardForm(int idUtente, string token)
        {
           
            InitializeComponent();

          
            this.idUtente = idUtente;
            this.token = token;

           
            CreaInterfaccia();

           
            Shown += async (sender, e) => await CaricaDashboardAsync();
        }


      
        private void CreaInterfaccia()
        {
            
            Text = "MotoCare - Dashboard";

          
            ClientSize = new Size(1100, 700);

           
            MinimumSize = new Size(850, 600);

           
            StartPosition = FormStartPosition.CenterScreen;

            
            BackColor = sfondo;
            Font = new Font("Segoe UI", 10);



          
            Panel menu = new Panel
            {
               
                Dock = DockStyle.Left,

                
                Width = 230,

              
                BackColor = pannello
            };

         
            Label logo = new Label
            {
                Text = "MOTOCARE",
                ForeColor = arancione,
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                Location = new Point(20, 25),
                AutoSize = true
            };

          
            menu.Controls.Add(logo);


         
            string[] voci =
            {
                "DASHBOARD",
                "LA MIA MOTO",
                "MANUTENZIONI",
                "SCADENZE",
                "SPESE"
            };

            // Ciclo che crea automaticamente un pulsante
            // per ogni voce presente nell'array.
            for (int i = 0; i < voci.Length; i++)
            {
             
                string voce = voci[i];

               
                Button pulsante = new Button
                {
                    Text = voce,

                   
                    Location = new Point(15, 110 + i * 60),

                
                    Size = new Size(200, 45),

                
                    BackColor = i == 0 ? arancione : pannello,

                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,

                    Font = new Font("Segoe UI", 10, FontStyle.Bold),

                  
                    TextAlign = ContentAlignment.MiddleLeft
                };

                
                pulsante.FlatAppearance.BorderSize = 0;


            
                pulsante.Click += async (sender, e) =>
                {
                   
                    if (voce == "LA MIA MOTO")
                    {
                      
                        using MotoForm form = new MotoForm(token);

                       
                        if (form.ShowDialog(this) == DialogResult.OK)
                        {
                           
                            await CaricaDashboardAsync();
                        }
                    }

                    
                    else if (voce != "DASHBOARD")
                    {
                        MessageBox.Show(
                            "Sezione " + voce + " da sviluppare.");
                    }
                };

              
                menu.Controls.Add(pulsante);
            }



            Button btnEsci = new Button
            {
                Text = "ESCI",

               
                Dock = DockStyle.Bottom,

                Height = 48,
                BackColor = pannello,
                ForeColor = arancione,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(15)
            };

           
            btnEsci.FlatAppearance.BorderColor = arancione;

           
            btnEsci.Click += (sender, e) => Close();

           
            menu.Controls.Add(btnEsci);



            Panel contenuto = new Panel
            {
                
                Dock = DockStyle.Fill,

                BackColor = sfondo,

             
                AutoScroll = true,

              
                Padding = new Padding(35)
            };


            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,

              
                ColumnCount = 2,
                RowCount = 3,

              
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,

                Padding = new Padding(0),
                Margin = new Padding(0)
            };

         
            layout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 50));

            layout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 50));

           
            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 120));

            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 180));

            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 180));


         
            Label titolo = new Label
            {
                Text = "LA TUA DASHBOARD",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 24, FontStyle.Bold),

              
                Dock = DockStyle.Top,

                AutoSize = true
            };

        
            Label sottotitolo = new Label
            {
                Text = "Tutto quello che serve per gestire la tua moto.",
                ForeColor = Color.Silver,

             
                Dock = DockStyle.Bottom,

                AutoSize = true
            };

         
            Panel intestazione = new Panel
            {
                Dock = DockStyle.Fill
            };

            intestazione.Controls.Add(titolo);
            intestazione.Controls.Add(sottotitolo);

          
            layout.Controls.Add(intestazione, 0, 0);

            
            layout.SetColumnSpan(intestazione, 2);


        
           
            string[] sezioni =
            {
                "LA MIA MOTO",
                "MANUTENZIONI",
                "SCADENZE",
                "SPESE"
            };

         
            string[] descrizioni =
            {
                "Registra e gestisci la tua moto",
                "Controlla gli interventi effettuati",
                "Tieni sotto controllo le scadenze",
                "Monitora i costi della tua moto"
            };

            // Ciclo che crea le quattro schede automaticamente.
            for (int i = 0; i < sezioni.Length; i++)
            {
                
                Panel card = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = pannello,

                   
                    Margin = new Padding(10),

                  
                    Padding = new Padding(20)
                };

               
                Label nome = new Label
                {
                    Text = sezioni[i],
                    ForeColor = arancione,
                    Font = new Font("Segoe UI", 14, FontStyle.Bold),
                    Dock = DockStyle.Top,
                    Height = 45
                };

            
                Label descrizione = new Label
                {
                    Text = descrizioni[i],
                    ForeColor = Color.White,
                    Dock = DockStyle.Fill
                };

              
                card.Controls.Add(descrizione);
                card.Controls.Add(nome);


               
                if (i == 0)
                {
                   
                    lblMoto = descrizione;

                 
                    lblKm = new Label
                    {
                        Text = "",
                        ForeColor = Color.Silver,

                       
                        Dock = DockStyle.Bottom,

                        Height = 28
                    };

                  
                    card.Controls.Add(lblKm);
                }


                // CALCOLO DELLA POSIZIONE DELLA SCHEDA

                // L'operatore % restituisce il resto della divisione.
                // Permette di alternare le due colonne:
                // 0 = colonna sinistra
                // 1 = colonna destra.
                int colonna = i % 2;

                // Calcola la riga in cui inserire la scheda.
                // Le schede iniziano dalla seconda riga,
                // perché la prima è occupata dall'intestazione.
                int riga = 1 + i / 2;

                // Inserisce la scheda nella posizione calcolata.
                layout.Controls.Add(card, colonna, riga);
            }


            // Inserisce la tabella nell'area principale.
            contenuto.Controls.Add(layout);

          
            Controls.Add(contenuto);
            Controls.Add(menu);
        }



        // async permette di eseguire operazioni di rete
        // senza bloccare l'interfaccia grafica.
        // Task rappresenta un'operazione che può terminare in futuro.
        private async Task CaricaDashboardAsync()
        {
           
            lblMoto.Text = "Caricamento...";

         
            lblKm.Text = "";


           
            try
            {
            
                using TcpClient client = new TcpClient();

           
                using CancellationTokenSource timeout =
                    new CancellationTokenSource(TimeSpan.FromSeconds(10));


              
                await client.ConnectAsync(
                    "127.0.0.1", 5000, timeout.Token);


                
                using NetworkStream stream = client.GetStream();

             
                using StreamReader reader = new StreamReader(stream);

                // AutoFlush = true invia automaticamente i dati
                // senza dover richiamare manualmente Flush().
                using StreamWriter writer = new StreamWriter(stream)
                {
                    AutoFlush = true
                };


            

            
                Richiesta richiesta = new Richiesta
                {
                    // Indica al Server quale operazione deve eseguire.
                  
                    Azione = "OTTIENI_MOTO",

                  
                    Token = token
                };


          
                await writer.WriteLineAsync(
                    JsonSerializer.Serialize(richiesta));


         

               
                string? jsonRisposta =
                    await reader.ReadLineAsync(timeout.Token);

              
                if (jsonRisposta == null)
                    throw new IOException("Nessuna risposta dal Server.");


                Risposta? risposta =
                    JsonSerializer.Deserialize<Risposta>(jsonRisposta);

              
                if (risposta == null)
                    throw new IOException("Risposta non valida.");


               
                if (!risposta.Successo)
                {
                    
                    lblMoto.Text = risposta.Messaggio;

                  
                    return;
                }


                
                if (risposta.Moto == null)
                {
                    
                    lblMoto.Text = "Nessuna moto registrata";

                    return;
                }


              

              
                lblMoto.Text =
                    risposta.Moto.Marca + " " +
                    risposta.Moto.Modello;

             
                lblKm.Text =
                    risposta.Moto.Chilometraggio.ToString("N0") +
                    " km";
            }


         

         
            catch (Exception ex)
            {
               
                lblMoto.Text = "Impossibile caricare la moto.";

               
                lblKm.Text = "";

              
                MessageBox.Show(
                    "Errore caricamento Dashboard: " + ex.Message);
            }
        }
    }
}
