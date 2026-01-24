using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace DispatcherSimulator
{
    public class TutorialForm : Form
    {
        private int _step = 0;
        private Label _lblTitle;
        private Label _lblDescription;
        private Label _lblPageInfo;
        private Button _btnNext;
        private Button _btnPrev;
        private Button _btnMenu;

        private readonly List<(string Title, string Text, Color Accent)> _steps = new()
        {
            ("CENTRÁLA DISPEČINKU", "Vítejte v tréninkovém programu. Jako operátor linky 112 jste první linií mezi občanem v nouzi a pomocí.", Color.Gold),
            ("1. PŘÍJEM VOLÁNÍ", "Když zazvoní telefon, vlevo nahoře se objeví karta hovoru. Kliknutím na ni hovor přijmete.\n\nTip: Nečekejte příliš dlouho, jinak hovor propadne!", Color.SkyBlue),
            ("2. KOMUNIKACE", "V dialogovém okně (vpravo nahoře) veďte hovor. Máte možnost se ptát na informace", Color.LimeGreen),
            ("3. NASAZENÍ SIL", "Podle zjištěných informací vyberte jednotky vpravo dole.\n\nKlikněte na typ jednotky a poté potvrďte tlačítkem UKONČIT A POSLAT.", Color.OrangeRed),
            ("4. SKÓRE A ÚSPĚCH", "Za každou správně vyslanou pomoc získáte body.\n\nPokud pošlete špatnou jednotku, vaše celkové hodnocení klesne.", Color.White)
        };

        public TutorialForm()
        {
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.FromArgb(20, 20, 20);

            InitializeUI();
            UpdateStep();
        }

        private void InitializeUI()
        {
            // Horní panel pro titulek
            var topPanel = new Panel { Dock = DockStyle.Top, Height = 120, BackColor = Color.FromArgb(35, 35, 35) };
            _lblTitle = new Label
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 32F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            topPanel.Controls.Add(_lblTitle);

            // Spodní panel pro navigaci
            var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 120, BackColor = Color.FromArgb(35, 35, 35) };

            // Tlačítko ZPĚT DO MENU (vlevo)
            _btnMenu = new Button
            {
                Text = "🏠 MENU",
                Size = new Size(180, 60),
                Location = new Point(30, 30),
                BackColor = Color.FromArgb(150, 40, 40),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            _btnMenu.Click += (s, e) => this.Close();

            // Tlačítko PŘEDCHOZÍ (uprostřed vlevo)
            _btnPrev = new Button
            {
                Text = "< PŘEDCHOZÍ",
                Size = new Size(200, 60),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            _btnPrev.Click += (s, e) => { if (_step > 0) { _step--; UpdateStep(); } };

            // Tlačítko DALŠÍ (uprostřed vpravo)
            _btnNext = new Button
            {
                Text = "DALŠÍ >",
                Size = new Size(200, 60),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            _btnNext.Click += (s, e) => {
                if (_step < _steps.Count - 1) { _step++; UpdateStep(); }
                else { this.Close(); }
            };

            // Informace o stránce
            _lblPageInfo = new Label
            {
                Size = new Size(200, 30),
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 11F),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Layout navigace (centrování tlačítek)
            bottomPanel.Controls.Add(_btnMenu);
            bottomPanel.Controls.Add(_btnPrev);
            bottomPanel.Controls.Add(_btnNext);
            bottomPanel.Controls.Add(_lblPageInfo);

            // Hlavní text
            _lblDescription = new Label
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 18F),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(150, 50, 150, 50)
            };

            this.Controls.Add(_lblDescription);
            this.Controls.Add(topPanel);
            this.Controls.Add(bottomPanel);

            // Logika pro správné umístění prvků při startu/změně velikosti
            this.Layout += (s, e) => {
                int centerX = this.ClientSize.Width / 2;
                _btnPrev.Location = new Point(centerX - 210, 30);
                _btnNext.Location = new Point(centerX + 10, 30);
                _lblPageInfo.Location = new Point(centerX - 100, 95);
            };
        }

        private void UpdateStep()
        {
            var current = _steps[_step];
            _lblTitle.Text = current.Title;
            _lblTitle.ForeColor = current.Accent;
            _lblDescription.Text = current.Text;
            _lblPageInfo.Text = $"Krok {_step + 1} z {_steps.Count}";

            // Viditelnost tlačítka Předchozí (na první straně je schované)
            _btnPrev.Visible = (_step > 0);

            // Text tlačítka Další na poslední straně
            _btnNext.Text = (_step == _steps.Count - 1) ? "DOKONČIT" : "DALŠÍ >";
        }
    }
}