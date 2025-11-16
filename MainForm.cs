using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using System.Drawing;

namespace DispatcherSimulator
{
    public partial class MainForm : Form
    {
        private const int Blue = 0;

        private List<Scenario> _scenarios = new();
        private int _score = 0;
        private Queue<Scenario> _currentQueue = new();

        private string _startingDifficulty;

        // Ovládací prvky pro design
        private Label lblScore;
        private ListBox listCalls;
        private Label lblScenario;
        private CheckBox chkPolice, chkFire, chkAmbulance;
        private FlowLayoutPanel panelUnits;

        public MainForm(string difficulty)
        {
            _startingDifficulty = difficulty;
            InitializeUI();
            LoadScenarios();

            // fullscreen
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;
        }

        private void InitializeUI()
        {
            this.Text = "Dispatcher Simulator";
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;

            // Horní panel
            var topPanel = new Panel
            {
                Height = 50,
                Dock = DockStyle.Top,
                BackColor = Color.DarkBlue
            };
            this.Controls.Add(topPanel);

            var lblTitle = new Label
            {
                Text = "Operační středisko 112",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                AutoSize = true,
                Left = 20,
                Top = 10
            };
            topPanel.Controls.Add(lblTitle);

            var lblScore = new Label
            {
                Text = "Skóre: 0",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                AutoSize = true,
                Top = 15,
                Left = this.ClientSize.Width - 250,
                Name = "lblScore",
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            topPanel.Controls.Add(lblScore);

            var btnMenu = new Button
            {
                Text = "Menu",
                ForeColor = Color.White,
                BackColor = Color.DarkGreen,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Width = 80,
                Height = 35,
                Left = this.ClientSize.Width - 160,
                Top = 8,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnMenu.Click += (s, e) =>
            {
                this.Hide(); var startForm = new StartForm();
                startForm.FormClosed += (s2, e2) => this.Close(); 
                startForm.Show();
            };
            topPanel.Controls.Add(btnMenu);

            var btnClose = new Button
            {
                Text = "X",
                ForeColor = Color.White,
                BackColor = Color.DarkRed,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Width = 40,
                Height = 35,
                Left = this.ClientSize.Width - 70,
                Top = 8,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnClose.Click += (s, e) => this.Close();
            topPanel.Controls.Add(btnClose);

            // Hlavní 2x2 layout
            var mainLayout4 = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single // pro vizuální rozdělení
            };

            // Sloupce a řádky 50 %
            mainLayout4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            mainLayout4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            mainLayout4.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            mainLayout4.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            this.Controls.Add(mainLayout4);

            // ===================== KVADRANTY =====================

            // Levý horní – scénář
            var panelTopLeft = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            lblScenario = new Label
            {
                Text = "Zde se zobrazí scénář...",
                Font = new Font("Segoe UI", 16F),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopLeft
            };
            panelTopLeft.Controls.Add(lblScenario);
            mainLayout4.Controls.Add(panelTopLeft, 0, 0);

            // Levý dolní – staré scénáře
            var panelBottomLeft = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            var lstOldScenarios = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 12F)
            };
            // Zatím prázdné, později se naplní starými scénáři
            panelBottomLeft.Controls.Add(lstOldScenarios);
            mainLayout4.Controls.Add(panelBottomLeft, 0, 1);

            // Pravý horní – kvadrant
            var panelTopRight = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };
            mainLayout4.Controls.Add(panelTopRight, 1, 0);

            // Centrální panel pro textová pole
            var centerPanel = new Panel
            {
                Width = 440,
                Height = 250,
                Anchor = AnchorStyles.None
            };
            panelTopRight.Controls.Add(centerPanel);

            // FlowLayoutPanel uvnitř centerPanel
            var infoPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true
            };
            centerPanel.Controls.Add(infoPanel);

            // Přidání textových polí
            void AddTextField(string labelText, string placeholder)
            {
                var lbl = new Label
                {
                    Text = labelText,
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                    AutoSize = true,
                    Margin = new Padding(0, 10, 0, 0)
                };
                infoPanel.Controls.Add(lbl);

                var txt = new TextBox
                {
                    Font = new Font("Segoe UI", 12F),
                    Width = 420,
                    Height = 35,
                    PlaceholderText = placeholder,
                    Margin = new Padding(0, 5, 0, 0)
                };
                infoPanel.Controls.Add(txt);
            }

            AddTextField("Jméno volajícího:", "Zadejte jméno...");
            AddTextField("Adresa události:", "Zadejte adresu...");

            // Poznámka operátora
            {
                var lbl = new Label
                {
                    Text = "Poznámka operátora:",
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                    AutoSize = true,
                    Margin = new Padding(0, 10, 0, 0)
                };
                infoPanel.Controls.Add(lbl);

                var txt = new TextBox
                {
                    Font = new Font("Segoe UI", 12F),
                    Width = 420,
                    Height = 120,
                    Multiline = true,
                    ScrollBars = ScrollBars.Vertical,
                    PlaceholderText = "Zde můžete napsat poznámku...",
                    Margin = new Padding(0, 5, 0, 0)
                };
                infoPanel.Controls.Add(txt);
            }

            // Funkce pro vycentrování centerPanel uvnitř panelTopRight
            void CenterPanel()
            {
                centerPanel.Left = (panelTopRight.ClientSize.Width - centerPanel.Width) / 2;
                centerPanel.Top = (panelTopRight.ClientSize.Height - centerPanel.Height) / 2;
            }
            CenterPanel();
            panelTopRight.Resize += (s, e) => CenterPanel();





            // Pravý dolní – tlačítka
            var panelBottomRight = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                Padding = new Padding(20),
                AutoScroll = true
            };

            // Label pro zobrazení vybraných jednotek
           // Label pro zobrazení vybraných jednotek
            var lblSelected = new Label
            {
                Text = "Vybráno: nic",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                AutoSize = true,
                ForeColor = Color.Black,                     // černý text
                BackColor = Color.FromArgb(240, 240, 240),   // světle šedé pozadí
                Padding = new Padding(10),
                Margin = new Padding(0, 15, 0, 0)
            };
            panelBottomRight.Controls.Add(lblSelected);



            // Tlačítka jednotek a potvrdit
           Panel CreateToggleButton(string text, Color baseColor, Color selectedColor)
            {
                bool selected = false;

                var panel = new Panel
                {
                    Width = 250,
                    Height = 60,
                    BackColor = baseColor,
                    Margin = new Padding(0, 0, 0, 15),
                    BorderStyle = BorderStyle.FixedSingle,
                    Cursor = Cursors.Hand
                };

                var lbl = new Label
                {
                    Text = text,
                    Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill,
                    Cursor = Cursors.Hand
                };
                panel.Controls.Add(lbl);

                void UpdateSelectedLabel()
                {
                    var selectedTexts = panelBottomRight.Controls
                        .OfType<Panel>()
                        .Where(p => p.Tag != null && (bool)p.Tag)
                        .Select(p => p.Controls.OfType<Label>().First().Text)
                        .ToList();

                    lblSelected.Text = selectedTexts.Count > 0
                        ? "Vybráno: " + string.Join(", ", selectedTexts)
                        : "Vybráno: nic";
                }

                void ToggleSelection()
                {
                    selected = !selected;
                    panel.Tag = selected;

                    if (selected)
                    {
                        panel.BackColor = selectedColor;
                        panel.BorderStyle = BorderStyle.Fixed3D;
                        lbl.Font = new Font(lbl.Font, FontStyle.Bold | FontStyle.Underline);
                    }
                    else
                    {
                        panel.BackColor = baseColor;
                        panel.BorderStyle = BorderStyle.FixedSingle;
                        lbl.Font = new Font(lbl.Font, FontStyle.Bold);
                    }

                    UpdateSelectedLabel();
                }

                panel.Click += (s, e) => ToggleSelection();
                lbl.Click += (s, e) => ToggleSelection();

                return panel;
            }



            // Přidání tlačítek do pravého dolního kvadrantu
            panelBottomRight.Controls.Add(CreateToggleButton("Policie", Color.FromArgb(30, 30, 200), Color.FromArgb(0, 0, 255)));
            panelBottomRight.Controls.Add(CreateToggleButton("Hasiči", Color.FromArgb(200, 30, 30), Color.FromArgb(255, 0, 0)));
            panelBottomRight.Controls.Add(CreateToggleButton("Záchranná služba", Color.FromArgb(250, 210, 10), Color.FromArgb(255, 240, 0)));

            // Přidáme label nakonec
            panelBottomRight.Controls.Add(lblSelected);



            // Potvrdit
            var btnAction = new Button
            {
                Text = "Potvrdit",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                Width = 200,
                Height = 50,
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 10, 0, 0)
            };
            btnAction.FlatAppearance.BorderSize = 0;
            btnAction.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 150, 245);
            panelBottomRight.Controls.Add(btnAction);


            mainLayout4.Controls.Add(panelBottomRight, 1, 1);
        }






        private void UpdateScoreLabel()
        {
            lblScore.Text = $"Skóre: {_score}";
        }

        private void StartGameModeOneRandom(string difficulty)
        {
            var pool = FilterByDifficulty(difficulty);
            if (!pool.Any()) { MessageBox.Show("Žádné scénáře pro zvolenou obtížnost."); return; }

            var rnd = new Random();
            var s = pool[rnd.Next(pool.Count)];
            _currentQueue = new Queue<Scenario>(new[] { s });
            PlayNextInQueue();
        }

        private void StartGameModeX(string difficulty, int x)
        {
            var pool = FilterByDifficulty(difficulty);
            if (!pool.Any()) { MessageBox.Show("Žádné scénáře pro zvolenou obtížnost."); return; }

            var rnd = new Random();
            var selected = new List<Scenario>();
            for (int i = 0; i < x; i++) selected.Add(pool[rnd.Next(pool.Count)]);

            _currentQueue = new Queue<Scenario>(selected);
            _score = 0;
            UpdateScoreLabel();
            PlayNextInQueue();
        }

        private void PlayNextInQueue()
        {
            if (_currentQueue.Count == 0)
            {
                MessageBox.Show($"Konec série. Skóre: {_score}");
                return;
            }

            var s = _currentQueue.Dequeue();
            lblScenario.Text = $"{s.Title}\n\n{s.Text}";
            chkPolice.Checked = chkFire.Checked = chkAmbulance.Checked = false;
        }

        private bool EvaluateResponse(Scenario s, List<string> selectedUnits)
        {
            var required = s.RequiredUnits ?? new List<string>();
            return required.All(r => selectedUnits.Contains(r)) && selectedUnits.All(u => required.Contains(u));
        }

        private List<Scenario> FilterByDifficulty(string difficulty)
        {
            if (difficulty == "All") return _scenarios;
            return _scenarios.Where(s => s.Difficulty.Equals(difficulty, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        private void LoadScenarios()
        {
            try
            {
                var path = Path.Combine(AppContext.BaseDirectory, "scenarios.json");
                if (!File.Exists(path))
                {
                    _scenarios = SampleScenarios();
                    return;
                }

                var json = File.ReadAllText(path);
                _scenarios = JsonSerializer.Deserialize<List<Scenario>>(json) ?? new List<Scenario>();
            }
            catch
            {
                _scenarios = SampleScenarios();
            }
        }

        private List<Scenario> SampleScenarios()
        {
            return new List<Scenario>
            {
                new Scenario{Id=1, Title="Požár v bytě", Text="Volající hlásí silný požár v panelovém domě. Vidí plameny a dým.", Difficulty="Easy", RequiredUnits=new List<string>{"Fire"}}, 
                new Scenario{Id=2, Title="Dopravní nehoda", Text="Kolize dvou aut, jeden zraněný bez vědomí.", Difficulty="Easy", RequiredUnits=new List<string>{"Ambulance","Police"}}
            };
        }
    }

    public class Scenario
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Text { get; set; } = "";
        public string Difficulty { get; set; } = "Easy";
        public List<string>? RequiredUnits { get; set; }
    }
}
