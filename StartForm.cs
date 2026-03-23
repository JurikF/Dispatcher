using System;
using System.Drawing;
using System.Windows.Forms;

namespace DispatcherSimulator
{
    public class StartForm : Form
    {
        private FlowLayoutPanel mainMenuPanel;
        private Panel difficultyPanel;
        private Panel languagePanel;
        private Label head;
        private Label lblDiff;
        private Button btnPlayMenu, btnTutorial, btnLanguage, btnExitApp, btnBack, btnStartGame;
        private ComboBox cbDifficulty;

        public StartForm()
        {
            this.Text = "Dispatcher Simulator - Menu";
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.StartPosition = FormStartPosition.CenterScreen;

            InitializeUI();
            UpdateLanguage();
        }

        private void InitializeUI()
        {
            this.Controls.Clear();

            head = new Label
            {
                Text = "Dispatcher Simulator",
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 120,
                Font = new Font("Segoe UI", 42F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(40, 40, 40)
            };
            this.Controls.Add(head);

            int topPadding = 370;
            int centerX = (Screen.PrimaryScreen.Bounds.Width - 400) / 2;

            mainMenuPanel = new FlowLayoutPanel { Width = 400, Height = 500, Location = new Point(centerX, topPadding), FlowDirection = FlowDirection.TopDown, BackColor = Color.Transparent };
            this.Controls.Add(mainMenuPanel);

            difficultyPanel = new Panel { Width = 400, Height = 300, BackColor = Color.FromArgb(50, 50, 50), Location = new Point(centerX, topPadding), Visible = false };
            this.Controls.Add(difficultyPanel);

            languagePanel = new Panel { Width = 400, Height = 300, BackColor = Color.FromArgb(50, 50, 50), Location = new Point(centerX, topPadding), Visible = false };
            this.Controls.Add(languagePanel);

            btnPlayMenu = CreateMenuButton("", Color.FromArgb(0, 120, 215));
            btnPlayMenu.Click += (s, e) => { mainMenuPanel.Visible = false; difficultyPanel.Visible = true; };

            btnTutorial = CreateMenuButton("", Color.FromArgb(70, 70, 70));
            btnTutorial.Click += (s, e) => { this.Hide(); var tutorial = new TutorialForm(GameSettings.CurrentLanguage); tutorial.FormClosed += (s2, e2) => this.Show(); tutorial.Show(); };

            btnLanguage = CreateMenuButton("", Color.FromArgb(70, 70, 70));
            btnLanguage.Click += (s, e) => { mainMenuPanel.Visible = false; languagePanel.Visible = true; };

            btnExitApp = CreateMenuButton("", Color.FromArgb(180, 40, 40));
            btnExitApp.Click += (s, e) => this.Close();

            mainMenuPanel.Controls.AddRange(new Control[] { btnPlayMenu, btnTutorial, btnLanguage, btnExitApp });

            lblDiff = new Label { Font = new Font("Segoe UI", 22F, FontStyle.Bold), ForeColor = Color.White, TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.Top, Height = 80 };
            difficultyPanel.Controls.Add(lblDiff);

            cbDifficulty = new ComboBox { Width = 300, Font = new Font("Segoe UI", 20F), DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(50, 90) };
            difficultyPanel.Controls.Add(cbDifficulty);

            btnStartGame = new Button { Size = new Size(160, 70), Location = new Point(30, 200), BackColor = Color.FromArgb(0, 180, 0), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 16F, FontStyle.Bold) };
            btnStartGame.Click += (s, e) => { string obt = cbDifficulty.SelectedItem.ToString(); MainForm hra = new MainForm(obt); this.Hide(); hra.ShowDialog(); this.Close(); };
            difficultyPanel.Controls.Add(btnStartGame);

            btnBack = new Button { Size = new Size(160, 70), Location = new Point(210, 200), BackColor = Color.FromArgb(100, 100, 100), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 16F, FontStyle.Bold) };
            btnBack.Click += (s, e) => { difficultyPanel.Visible = false; mainMenuPanel.Visible = true; };
            difficultyPanel.Controls.Add(btnBack);

            var btnCZ = new Button { Text = "ČEŠTINA", Size = new Size(340, 70), Location = new Point(30, 30), BackColor = Color.FromArgb(0, 120, 215), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 16F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnCZ.Click += (s, e) => { GameSettings.CurrentLanguage = "CZ"; UpdateLanguage(); languagePanel.Visible = false; mainMenuPanel.Visible = true; };
            
            var btnEN = new Button { Text = "ENGLISH", Size = new Size(340, 70), Location = new Point(30, 110), BackColor = Color.FromArgb(0, 120, 215), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 16F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnEN.Click += (s, e) => { GameSettings.CurrentLanguage = "EN"; UpdateLanguage(); languagePanel.Visible = false; mainMenuPanel.Visible = true; };

            languagePanel.Controls.Add(btnCZ);
            languagePanel.Controls.Add(btnEN);
        }

        private void UpdateLanguage()
        {
            bool isEn = GameSettings.CurrentLanguage == "EN";
            
            btnPlayMenu.Text = isEn ? "PLAY" : "HRÁT";
            btnTutorial.Text = isEn ? "TUTORIAL" : "TUTORIÁL";
            btnLanguage.Text = isEn ? "LANGUAGE" : "JAZYK";
            btnExitApp.Text = isEn ? "EXIT" : "ODEJÍT";
            
            lblDiff.Text = isEn ? "Select Difficulty:" : "Vyber obtížnost:";
            btnStartGame.Text = "START";
            btnBack.Text = isEn ? "BACK" : "ZPĚT";

            cbDifficulty.Items.Clear();
            if (isEn) cbDifficulty.Items.AddRange(new[] { "Easy", "Medium", "Hard", "All Difficulties" });
            else cbDifficulty.Items.AddRange(new[] { "Lehká", "Střední", "Těžká", "Všechny obtížnosti" });
            cbDifficulty.SelectedIndex = 0;
        }

        private Button CreateMenuButton(string text, Color color)
        {
            var btn = new Button { Text = text, Size = new Size(380, 80), Margin = new Padding(10, 15, 10, 15), Font = new Font("Segoe UI", 20F, FontStyle.Bold), BackColor = color, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }
    }
}