using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Text.Json;
using System.IO;
using MotoCare.Shared;

namespace MotoCare.Client
{
   
    public partial class RegistrazioneForm : Form
    {
       
        private readonly Color backgroundColor = Color.FromArgb(21, 21, 21);
        private readonly Color panelColor = Color.FromArgb(36, 36, 36);
        private readonly Color fieldColor = Color.FromArgb(54, 54, 54);
        private readonly Color orangeColor = Color.FromArgb(255, 102, 0);
        private readonly Color secondaryTextColor = Color.FromArgb(176, 176, 176);

       
        private Panel pnlHeader = null!;
        private Panel pnlCard = null!;
        private Label lblBrandMoto = null!;
        private Label lblBrandCare = null!;
        private Label lblEyebrow = null!;
        private Label lblTitle = null!;
        private Label lblSubtitle = null!;
        private Label lblNome = null!;
        private Label lblCognome = null!;
        private Label lblEmail = null!;
        private Label lblPassword = null!;
        private Label lblConfermaPassword = null!;
        private Label lblFooter = null!;

       
        private TextBox txtNome = null!;
        private TextBox txtCognome = null!;
        private TextBox txtEmail = null!;
        private TextBox txtPassword = null!;
        private TextBox txtConfermaPassword = null!;
        private Button btnRegistrati = null!;
        private Button btnIndietro = null!;

       
        public RegistrazioneForm()
        {
            
            InitializeComponent();

         
            ConfigureInterface();
        }

       
        private void ConfigureInterface()
        {
           
            BackColor = backgroundColor;
            ClientSize = new Size(500, 680);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "MotoCare - Registrazione";

            
            pnlHeader = new Panel
            {
                BackColor = backgroundColor,
                Dock = DockStyle.Top,
                Height = 78
            };

          
            lblBrandMoto = CreateBrandLabel("MOTO", Color.White, 190, 6);
            lblBrandCare = CreateBrandLabel("CARE", orangeColor, 224, 36);

            
            pnlHeader.Controls.Add(lblBrandMoto);
            pnlHeader.Controls.Add(lblBrandCare);

           
            pnlCard = new Panel
            {
                BackColor = panelColor,
                Dock = DockStyle.Fill,
                Padding = new Padding(42, 24, 42, 20)
            };

           
            lblEyebrow = CreateLabel("MOTOCARE / ACCOUNT", 8.5F, FontStyle.Bold, orangeColor, 42, 24);
            lblTitle = CreateLabel("CREA IL TUO ACCOUNT", 18F, FontStyle.Bold, Color.White, 39, 43);
            lblSubtitle = CreateLabel("Entra nella tua prossima avventura.", 9F, FontStyle.Regular, secondaryTextColor, 42, 77);

           
            lblNome = CreateFieldLabel("NOME", 42, 112);
            lblCognome = CreateFieldLabel("COGNOME", 42, 178);
            lblEmail = CreateFieldLabel("EMAIL", 42, 244);
            lblPassword = CreateFieldLabel("PASSWORD", 42, 310);
            lblConfermaPassword = CreateFieldLabel("CONFERMA PASSWORD", 42, 376);

          
            txtNome = CreateTextBox(42, 130);
            txtCognome = CreateTextBox(42, 196);
            txtEmail = CreateTextBox(42, 262);
            txtPassword = CreateTextBox(42, 328);
            txtConfermaPassword = CreateTextBox(42, 394);

          
            txtPassword.UseSystemPasswordChar = true;
            txtConfermaPassword.UseSystemPasswordChar = true;

          
            btnRegistrati = CreatePrimaryButton("REGISTRATI", 42, 452);

           
            btnIndietro = CreateSecondaryButton("TORNA AL LOGIN", 42, 505);

           
            btnRegistrati.Click += BtnRegistrati_Click;
            btnIndietro.Click += BtnIndietro_Click;

            
            lblFooter = CreateLabel("Gestisci la tua moto. Vivi ogni chilometro.", 8F, FontStyle.Regular,
                Color.FromArgb(120, 120, 120), 42, 545);

           
            pnlCard.Controls.AddRange(new Control[]
            {
                lblEyebrow, lblTitle, lblSubtitle,
                lblNome, lblCognome, lblEmail, lblPassword, lblConfermaPassword,
                txtNome, txtCognome, txtEmail, txtPassword, txtConfermaPassword,
                btnRegistrati, btnIndietro, lblFooter
            });

       
            Controls.Add(pnlCard);
            Controls.Add(pnlHeader);
        }

       
        private Label CreateBrandLabel(string text, Color color, int x, int y)
        {
            return new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 17F, FontStyle.Bold),
                ForeColor = color,
                Location = new Point(x, y),
                Text = text
            };
        }

       
        private Label CreateLabel(string text, float size, FontStyle style, Color color, int x, int y)
        {
            return new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", size, style),
                ForeColor = color,
                Location = new Point(x, y),
                Text = text
            };
        }

      
        private Label CreateFieldLabel(string text, int x, int y)
        {
            return CreateLabel(text, 8F, FontStyle.Bold, secondaryTextColor, x, y);
        }

        private TextBox CreateTextBox(int x, int y)
        {
            return new TextBox
            {
                BackColor = fieldColor,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.White,
                Location = new Point(x, y),
                Size = new Size(416, 28),
                Margin = new Padding(0)
            };
        }

       
        private Button CreatePrimaryButton(string text, int x, int y)
        {
            return new Button
            {
                BackColor = orangeColor,
                FlatAppearance = { BorderSize = 0 },
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(x, y),
                Size = new Size(416, 40),
                Text = text,
                UseVisualStyleBackColor = false
            };
        }

       
        private Button CreateSecondaryButton(string text, int x, int y)
        {
            return new Button
            {
                BackColor = panelColor,
                FlatAppearance = { BorderColor = orangeColor, BorderSize = 1 },
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = orangeColor,
                Location = new Point(x, y),
                Size = new Size(416, 36),
                Text = text,
                UseVisualStyleBackColor = false
            };
        }


       
        private async void BtnRegistrati_Click(object? sender, EventArgs e)
        {
           
            string nome = txtNome.Text.Trim();
            string cognome = txtCognome.Text.Trim();
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text;

            
            if (string.IsNullOrWhiteSpace(nome) ||
                string.IsNullOrWhiteSpace(cognome) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Compila tutti i campi.");
                return;
            }

           
            if (!email.Contains('@') || !email.Contains('.'))
            {
                MessageBox.Show("Inserisci un'email valida.");
                return;
            }

            
            if (password.Length < 8)
            {
                MessageBox.Show("La password deve avere almeno 8 caratteri.");
                return;
            }

          
            if (password != txtConfermaPassword.Text)
            {
                MessageBox.Show("Le password non coincidono.");
                return;
            }

          
            btnRegistrati.Enabled = false;

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
                    Azione = "REGISTRA",
                    Nome = nome,
                    Cognome = cognome,
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

              
                MessageBox.Show(risposta.Messaggio);

               
                if (risposta.Successo)
                {
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
               
                btnRegistrati.Enabled = true;
            }
        }


       
        private void BtnIndietro_Click(object? sender, EventArgs e)
        {
            
            Close();
        }
    }
}