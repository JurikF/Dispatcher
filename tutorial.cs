using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.Json;
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

        // Třídy pro správné načtení struktury z tutorial.json
        private class TutorialData 
        { 
            public List<TutorialStep> CZ { get; set; } 
            public List<TutorialStep> EN { get; set; } 
        }

        private class TutorialStep 
        { 
            public int Step { get; set; }
            public string Title { get; set; } 
            public string Description { get; set; } 
            public string Accent { get; set; } 
        }

        private List<TutorialStep> _currentSteps = new();
        private string _currentLang = "CZ"; // Zatím natvrdo CZ

        public TutorialForm(string lang) // Teď přijímá "CZ" nebo "EN"
        {
            _currentLang = lang; // Tímto se nastaví jazyk před načtením dat
            
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.FromArgb(20, 20, 20);

            LoadTutorialData(); // Teď už LoadTutorialData ví, jaký jazyk načíst
            InitializeUI();
            UpdateStep();
        }

        private void LoadTutorialData()
        {
            try
            {
                string path = Path.Combine(AppContext.BaseDirectory, "tutorial.json");
                if (File.Exists(path))
                {
                    string jsonContent = File.ReadAllText(path);
                    var data = JsonSerializer.Deserialize<TutorialData>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    
                    if (data != null)
                    {
                        _currentSteps = (_currentLang == "CZ") ? data.CZ : data.EN;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Chyba při načítání tutorial.json: " + ex.Message);
                // Nouzový řádek, kdyby se něco pokazilo
                _currentSteps = new List<TutorialStep> { new TutorialStep { Title = "CHYBA", Description = "Nepodařilo se načíst data.", Accent = "Red" } };
            }
        }

        private void InitializeUI()
        {
            // Horní panel
            var topPanel = new Panel { Dock = DockStyle.Top, Height = 120, BackColor = Color.FromArgb(35, 35, 35) };
            _lblTitle = new Label { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 32F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter };
            topPanel.Controls.Add(_lblTitle);

            // Spodní panel
            var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 120, BackColor = Color.FromArgb(35, 35, 35) };

            _btnMenu = new Button
            {
                Text = "🏠 MENU",
                Size = new Size(180, 60),
                BackColor = Color.FromArgb(150, 40, 40),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            _btnMenu.Click += (s, e) => this.Close();

            _btnPrev = new Button
            {
                Text = _currentLang == "CZ" ? "< PŘEDCHOZÍ" : "< PREVIOUS",
                Size = new Size(200, 60),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            _btnPrev.Click += (s, e) => { if (_step > 0) { _step--; UpdateStep(); } };

            _btnNext = new Button
            {
                Text = _currentLang == "CZ" ? "DALŠÍ >" : "NEXT >",
                Size = new Size(200, 60),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            _btnNext.Click += (s, e) => {
                if (_step < _currentSteps.Count - 1) { _step++; UpdateStep(); }
                else { this.Close(); }
            };

            _lblPageInfo = new Label { Size = new Size(200, 30), ForeColor = Color.Gray, Font = new Font("Segoe UI", 11F), TextAlign = ContentAlignment.MiddleCenter };

            bottomPanel.Controls.AddRange(new Control[] { _btnMenu, _btnPrev, _btnNext, _lblPageInfo });

            // Hlavní text uprostřed
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

            // Centrování tlačítek
            this.Layout += (s, e) => {
                int centerX = this.ClientSize.Width / 2;
                _btnMenu.Location = new Point(30, 30);
                _btnPrev.Location = new Point(centerX - 210, 30);
                _btnNext.Location = new Point(centerX + 10, 30);
                _lblPageInfo.Location = new Point(centerX - 100, 95);
            };
        }

        private void UpdateStep()
        {
            if (_currentSteps == null || _currentSteps.Count == 0) return;

            var current = _currentSteps[_step];
            _lblTitle.Text = current.Title;
            
            // Převod barvy z textu (Accent)
            try { _lblTitle.ForeColor = Color.FromName(current.Accent ?? "White"); }
            catch { _lblTitle.ForeColor = Color.White; }

            _lblDescription.Text = current.Description;
            _lblPageInfo.Text = _currentLang == "CZ" ? $"Krok {_step + 1} z {_currentSteps.Count}" : $"Step {_step + 1} of {_currentSteps.Count}";

            _btnPrev.Visible = (_step > 0);
            _btnNext.Text = (_step == _currentSteps.Count - 1) ? (_currentLang == "CZ" ? "DOKONČIT" : "FINISH") : (_currentLang == "CZ" ? "DALŠÍ >" : "NEXT >");
        }
    }
}