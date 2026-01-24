using System;
using System.Drawing;
using System.Windows.Forms;

namespace DispatcherSimulator
{
    public class StartForm : Form
    {
        
        public string SelectedDifficulty { get; private set; } = "All";

        public StartForm()
        {
            this.Text = "Dispatcher Simulator - Menu";
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.StartPosition = FormStartPosition.CenterScreen;

            // --- HLAVIČKA ---
            var head = new Label
            {
                Text = "Dispatcher Simulator",
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 120, // Trochu jsem ji zúžil, aby tlačítka mohla výš
                Font = new Font("Segoe UI", 42F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(40, 40, 40)
            };
            this.Controls.Add(head);

            // Pomocná proměnná pro pozici Y pod hlavičkou
            int topPadding = 370; 

            // --- PANEL PRO HLAVNÍ MENU ---
            var mainMenuPanel = new FlowLayoutPanel
            {
                Width = 400,
                Height = 500,
                // Vycentrování na šířku, výška nastavena na topPadding
                Location = new Point((Screen.PrimaryScreen.Bounds.Width - 400) / 2, topPadding),
                FlowDirection = FlowDirection.TopDown,
                BackColor = Color.Transparent,
            };
            this.Controls.Add(mainMenuPanel);

            // --- PANEL PRO VÝBĚR OBTÍŽNOSTI (skrytý) ---
            var difficultyPanel = new Panel
            {
                Width = 400,
                Height = 300,
                BackColor = Color.FromArgb(50, 50, 50),
                Location = new Point((Screen.PrimaryScreen.Bounds.Width - 400) / 2, topPadding),
                Visible = false
            };
            this.Controls.Add(difficultyPanel);

            // --- FUNKCE PRO TVORBU MENU TLAČÍTEK ---
            Button CreateMenuButton(string text, Color color)
            {
                var btn = new Button
                {
                    Text = text,
                    Size = new Size(380, 80), // Trochu vyšší tlačítka pro lepší klikání
                    Margin = new Padding(10, 15, 10, 15),
                    Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                    BackColor = color,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand
                };
                btn.FlatAppearance.BorderSize = 0;
                return btn;
            }

            // 1. Tlačítko: HRÁT
            var btnPlayMenu = CreateMenuButton("HRÁT", Color.FromArgb(0, 120, 215));
            btnPlayMenu.Click += (s, e) => {
                mainMenuPanel.Visible = false;
                difficultyPanel.Visible = true;
            };

            // 2. Tlačítko: TUTORIÁL
            var btnTutorial = CreateMenuButton("TUTORIÁL", Color.FromArgb(70, 70, 70));
            
btnTutorial.Click += (s, e) => 
{
    this.Hide(); // Skryjeme menu
    var tutorial = new TutorialForm();
    
    // Po zavření tutoriálu se vrátíme zpět do menu
    tutorial.FormClosed += (s2, e2) => this.Show();
    
    tutorial.Show(); 
};

            // 3. Tlačítko: JAZYK
            var btnLanguage = CreateMenuButton("JAZYK", Color.FromArgb(70, 70, 70));
            btnLanguage.Click += (s, e) => MessageBox.Show("Změna jazyka bude dostupná brzy.");

            // Tlačítko pro ukončení
            var btnExitApp = CreateMenuButton("ODEJÍT", Color.FromArgb(180, 40, 40));
            btnExitApp.Click += (s, e) => this.Close();

            mainMenuPanel.Controls.AddRange(new Control[] { btnPlayMenu, btnTutorial, btnLanguage, btnExitApp });

            // --- OBSAH PANELU OBTÍŽNOSTI ---
            var lblDiff = new Label
            {
                Text = "Vyber obtížnost:",
                Font = new Font("Segoe UI", 22F, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 80
            };
            difficultyPanel.Controls.Add(lblDiff);

            var cbDifficulty = new ComboBox
            {
                Width = 300,
                Font = new Font("Segoe UI", 20F),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(50, 90)
            };
            cbDifficulty.Items.AddRange(new[] { "Lehká", "Střední", "Těžká", "Všechny obtížnosti" });
            cbDifficulty.SelectedIndex = 0;
            difficultyPanel.Controls.Add(cbDifficulty);
            var btnStartGame = new Button
            {
                Text = "START",
                Size = new Size(160, 70),
                Location = new Point(30, 200),
                BackColor = Color.FromArgb(0, 180, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold)
            };
            btnStartGame.Click += (s, e) =>
            {
                // Zjistíme, co hráč vybral (např. z ComboBoxu na startovní ploše)
                string obtiznost = cbDifficulty.SelectedItem.ToString();

                // Vytvoříme hlavní hru a POŠLEME jí tu obtížnost
                MainForm hra = new MainForm(obtiznost);
                
                this.Hide(); // Schováme startovní menu
                hra.ShowDialog(); // Spustíme hru
                this.Close(); // Po zavření hry zavřeme i start menu
            };
            difficultyPanel.Controls.Add(btnStartGame);

            var btnBack = new Button
            {
                Text = "ZPĚT",
                Size = new Size(160, 70),
                Location = new Point(210, 200),
                BackColor = Color.FromArgb(100, 100, 100),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold)
            };
            btnBack.Click += (s, e) => {
                difficultyPanel.Visible = false;
                mainMenuPanel.Visible = true;
            };
            difficultyPanel.Controls.Add(btnBack);
        }
    }
}