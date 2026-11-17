using System;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Text.Json;
using System.Windows.Forms;
using MotoCare.Shared;

namespace MotoCare.Client
{
  
    public partial class MotoForm : Form
    {
      
        private readonly string token;

        private Moto? motoEsistente;

        
        private readonly Color sfondo = Color.FromArgb(21, 21, 21);
        private readonly Color pannello = Color.FromArgb(36, 36, 36);
        private readonly Color arancione = Color.FromArgb(255, 102, 0);

      
        private TextBox txtMarca = null!;
        private TextBox txtModello = null!;
        private TextBox txtAnno = null!;
        private TextBox txtCilindrata = null!;
        private TextBox txtTarga = null!;
        private TextBox txtKm = null!;

      
        private Button btnSalva = null!;


       
        public MotoForm(string token)
        {
            InitializeComponent();

            
            this.token = token;

          
            CreaInterfaccia();

            
            Shown += async (sender, e) => await CaricaMotoAsync();
        }


     
        private void CreaInterfaccia()
        {
            Text = "MotoCare - La mia moto";
            ClientSize = new Size(520, 660);
            BackColor = sfondo;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Font = new Font("Segoe UI", 10);

            
            Label titolo = new Label
            {
                Text = "REGISTRA LA TUA MOTO",
                ForeColor = arancione,
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                Location = new Point(35, 25),
                AutoSize = true
            };

          
            Panel card = new Panel
            {
                BackColor = pannello,
                Location = new Point(35, 95),
                Size = new Size(450, 525)
            };

           
            txtMarca = CreaCampo(card, "MARCA", 20);
            txtModello = CreaCampo(card, "MODELLO", 95);
            txtAnno = CreaCampo(card, "ANNO", 170);
            txtCilindrata = CreaCampo(card, "CILINDRATA (CC)", 245);
            txtTarga = CreaCampo(card, "TARGA", 320);
            txtKm = CreaCampo(card, "CHILOMETRAGGIO", 395);

           
            btnSalva = new Button
            {
                Text = "SALVA MOTO",
                Location = new Point(25, 465),
                Size = new Size(400, 42),
                BackColor = arancione,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

       
            btnSalva.FlatAppearance.BorderSize = 0;

          
            btnSalva.Click += BtnSalva_Click;

           
            card.Controls.Add(btnSalva);

            
            Controls.Add(titolo);
            Controls.Add(card);
        }


       
        private TextBox CreaCampo(
            Panel contenitore, string etichetta, int y)
        {
            
            Label label = new Label
            {
                Text = etichetta,
                ForeColor = Color.LightGray,
                Location = new Point(25, y),
                AutoSize = true
            };

         
            TextBox campo = new TextBox
            {
                Location = new Point(25, y + 25),
                Size = new Size(400, 30),
                BackColor = Color.FromArgb(54, 54, 54),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

           
            contenitore.Controls.Add(label);
            contenitore.Controls.Add(campo);

           
            return campo;
        }



        
        private async Task CaricaMotoAsync()
        {
            // Disabilita il pulsante durante il caricamento
            btnSalva.Enabled = false;

            try
            {
              
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
                    MessageBox.Show(risposta.Messaggio);
                    return;
                }

               
                motoEsistente = risposta.Moto;

               
                if (motoEsistente == null)
                {
                    btnSalva.Enabled = true;
                    return;
                }

              
                txtMarca.Text = motoEsistente.Marca;
                txtModello.Text = motoEsistente.Modello;
                txtAnno.Text = motoEsistente.Anno.ToString();
                txtCilindrata.Text = motoEsistente.Cilindrata.ToString();
                txtTarga.Text = motoEsistente.Targa;
                txtKm.Text = motoEsistente.Chilometraggio.ToString();

                btnSalva.Text = "SALVA MODIFICHE";

              
                btnSalva.Enabled = true;

              
                Text = "MotoCare - Dettagli moto";
            }
            catch (Exception ex)
            {
              
                MessageBox.Show("Errore caricamento moto: " + ex.Message);
            }
        }



    
        private async void BtnSalva_Click(
            object? sender, EventArgs e)
        {
          
            if (string.IsNullOrWhiteSpace(txtMarca.Text) ||
                string.IsNullOrWhiteSpace(txtModello.Text))
            {
                MessageBox.Show("Inserisci marca e modello.");
                return;
            }

           
            if (!int.TryParse(txtAnno.Text, out int anno) ||
                anno < 1885 ||
                anno > DateTime.Now.Year + 1)
            {
                MessageBox.Show("Anno non valido.");
                return;
            }

        
            if (!int.TryParse(
                    txtCilindrata.Text, out int cilindrata) ||
                cilindrata < 0 ||
                !int.TryParse(txtKm.Text, out int km) ||
                km < 0)
            {
                MessageBox.Show(
                    "Cilindrata o chilometraggio non validi.");
                return;
            }

           
            Moto moto = new Moto
            {
                Marca = txtMarca.Text.Trim(),
                Modello = txtModello.Text.Trim(),
                Anno = anno,
                Cilindrata = cilindrata,
                Targa = txtTarga.Text.Trim(),
                Chilometraggio = km
            };

            // Disabilita il pulsante per evitare più invii contemporanei
            btnSalva.Enabled = false;

            try
            {
               
                using TcpClient client = new TcpClient();

              
                using CancellationTokenSource timeout =
                    new CancellationTokenSource(
                        TimeSpan.FromSeconds(10));

               
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
                   
                    Azione = motoEsistente == null
                        ? "REGISTRA_MOTO"
                        : "MODIFICA_MOTO",

                    Token = token,

                  
                    Moto = moto
                };

               
                await writer.WriteLineAsync(
                    JsonSerializer.Serialize(richiesta));

               
                string? jsonRisposta =
                    await reader.ReadLineAsync(timeout.Token);

               
                if (jsonRisposta == null)
                    throw new IOException(
                        "Nessuna risposta dal Server.");

              
                Risposta? risposta =
                    JsonSerializer.Deserialize<Risposta>(
                        jsonRisposta);

              
                if (risposta == null)
                    throw new IOException(
                        "Risposta non valida.");

               
                MessageBox.Show(risposta.Messaggio);

                
                if (risposta.Successo)
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
            catch (Exception ex)
            {
               
                MessageBox.Show(
                    "Errore di connessione: " + ex.Message);
            }
            finally
            {
                
                btnSalva.Enabled = true;
            }
        }
    }
}