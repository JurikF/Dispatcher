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
        // --- GLOBÁLNÍ PROMĚNNÉ ---
        private FlowLayoutPanel _callsFlowPanel;
        private List<Scenario> _scenarios = new();
        private int _score = 0;
        private Label lblScore;
        string _selectedDifficultyFilter = "Všechny obtížnosti";
        private System.Windows.Forms.Timer _callTimer;

        // Terminál hovoru (Vpravo nahoře)
        private Panel _activeCallPanel;
        private TextBox _txtChatDisplay;
        private FlowLayoutPanel _questionFlowPanel;
        private FlowLayoutPanel _unitSelectionPanel; // Přesunuto sem
        private Scenario _currentActiveScenario;
        private Panel _currentActiveCard;
        private Label lblNoCall;

        // Manuál (Vlevo dole)
        private TextBox txtManual;
        private Label lblPageInfo;
        private string[] manualPages;
        private int currentPage = 0;

        // Historie (Vpravo dole)
        private ListBox lstHistory;

        public MainForm(string vybranaObtiznost)
    {

        // Přiřadíme hodnotu z parametru do naší proměnné v class
        _selectedDifficultyFilter = vybranaObtiznost;

        // Inicializace timeru (pokud ho nemáš z Designeru)
        _callTimer = new System.Windows.Forms.Timer();
        _callTimer.Interval = 5000; // 5 sekund
        _callTimer.Tick += (s, e) => GenerateRandomCall();

        InitializeUI();
        LoadScenarios();
        
        _callTimer.Start();
    }

        private void InitializeUI()
        {
            this.Text = "Dispatcher Simulator";
            this.BackColor = Color.FromArgb(20, 20, 20);
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;

            // 1. HORNÍ PANEL (Menu)
            var topPanel = new Panel { Height = 60, Dock = DockStyle.Top, BackColor = Color.FromArgb(79, 81, 84), Padding = new Padding(15, 5, 15, 5) };
            this.Controls.Add(topPanel);

            lblScore = new Label { Text = "Skóre: 0", ForeColor = Color.White, Font = new Font("Segoe UI", 18F, FontStyle.Bold), Dock = DockStyle.Left, AutoSize = true, TextAlign = ContentAlignment.MiddleLeft };
            topPanel.Controls.Add(lblScore);

            var btnClose = new Button { Text = "✕", Dock = DockStyle.Right, Width = 50, BackColor = Color.DarkRed, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 12F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => Application.Exit();
            
            var btnMenu = new Button { Text = "ZPĚT DO MENU", Dock = DockStyle.Right, Width = 150, BackColor = Color.FromArgb(0, 180, 0), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnMenu.FlatAppearance.BorderSize = 0;
            btnMenu.Click += (s, e) => { if (MessageBox.Show("Vrátit se do menu?", "Menu", MessageBoxButtons.YesNo) == DialogResult.Yes) Application.Restart(); };

            topPanel.Controls.Add(btnMenu);
            topPanel.Controls.Add(new Panel { Dock = DockStyle.Right, Width = 10 }); 
            topPanel.Controls.Add(btnClose);

            // 2. HLAVNÍ LAYOUT
            var mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2, CellBorderStyle = TableLayoutPanelCellBorderStyle.Single };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F)); // Horní část trochu větší pro terminál
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            this.Controls.Add(mainLayout);

            // --- KVADRANTY ---

            // LEVÝ HORNÍ - Seznam hovorů
            var p1 = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10, 70, 10, 10), BackColor = Color.FromArgb(30, 30, 30) };
            var lblCallsTitle = new Label { Text = "PŘÍCHOZÍ HOVORY", Dock = DockStyle.Top, Height = 35, Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = Color.White };
            _callsFlowPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.FromArgb(40, 40, 40) };
            p1.Controls.Add(_callsFlowPanel);
            p1.Controls.Add(lblCallsTitle);
            mainLayout.Controls.Add(p1, 0, 0);

            // PRAVÝ HORNÍ - KOMPLETNÍ TERMINÁL (Chat + Jednotky + Potvrdit)
            var p2 = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(30, 30, 30), Padding = new Padding(10, 70, 10, 10) };
            lblNoCall = new Label { Text = "TERMINÁL NEAKTIVNÍ\r\nVyberte hovor ze seznamu", ForeColor = Color.White, Font = new Font("Consolas", 14F), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter };
            
            _activeCallPanel = new Panel { Dock = DockStyle.Fill, Visible = false };
            
            _txtChatDisplay = new TextBox { Multiline = true, ReadOnly = true, Dock = DockStyle.Top, Height = 250, BackColor = Color.Black, ForeColor = Color.Lime, Font = new Font("Consolas", 12F), BorderStyle = BorderStyle.None, ScrollBars = ScrollBars.Vertical};
            var lblQuestion = new Label { Text = "OTÁZKY PRO VOLAJÍCÍHO:", Dock = DockStyle.Top, Height = 25, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.White };
            _questionFlowPanel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(0, 5, 0, 0) };
            var lblUnits = new Label { Text = "DOSTUPNÉ JEDNOTKY:", Dock = DockStyle.Top, Height = 25, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.White };
            _unitSelectionPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(35, 35, 35), Padding = new Padding(5) };
            
            var btnConfirm = new Button { Text = "POTVRDIT A VYSLAT", Dock = DockStyle.Bottom, Height = 50, BackColor = Color.DarkGreen, ForeColor = Color.White, Font = new Font("Segoe UI", 12F, FontStyle.Bold), FlatStyle = FlatStyle.Flat };
            btnConfirm.Click += (s, e) => HandleConfirmation();

            _activeCallPanel.Controls.Add(_unitSelectionPanel);
            _activeCallPanel.Controls.Add(btnConfirm);
            _activeCallPanel.Controls.Add(lblUnits);
            _activeCallPanel.Controls.Add(_questionFlowPanel);
            _activeCallPanel.Controls.Add(lblQuestion);
            _activeCallPanel.Controls.Add(_txtChatDisplay);
            
            p2.Controls.Add(lblNoCall);
            p2.Controls.Add(_activeCallPanel);
            mainLayout.Controls.Add(p2, 1, 0);

            // LEVÝ DOLNÍ - Manuál
            var p3 = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10), BackColor = Color.FromArgb(30, 30, 30) };
            var lblMan = new Label { Text = "MANUÁL", Dock = DockStyle.Top, Height = 35, Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = Color.White };
            manualPages = new[] {
                "OVLÁDÁNÍ\r\n\r\n1. Vyber hovor vlevo.\r\n2. Získej info otázkami.\r\n3. Zvol jednotky a potvrď.",
                "JEDNOTKY\r\n\r\nPOLICIE: Zločin, doprava.\r\nHASIČI: Oheň, nehody.\r\nZÁCHRANKA: Zdraví."
            };
            txtManual = new TextBox { Multiline = true, ReadOnly = true, Dock = DockStyle.Fill, BackColor = Color.FromArgb(40, 40, 40), ForeColor = Color.White, Font = new Font("Segoe UI", 15F), BorderStyle = BorderStyle.None, Text = manualPages[0] };
            var nav = new Panel { Dock = DockStyle.Bottom, Height = 30 };
            var bP = new Button { Text = "<", Dock = DockStyle.Left, Width = 40, BackColor = Color.FromArgb(70, 70, 70), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            var bN = new Button { Text = ">", Dock = DockStyle.Right, Width = 40, BackColor = Color.FromArgb(70, 70, 70), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            lblPageInfo = new Label { Text = "1 / 2", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.White };
            bN.Click += (s, e) => { if (currentPage < 1) { currentPage++; txtManual.Text = manualPages[currentPage]; lblPageInfo.Text = $"{currentPage + 1} / 2"; } };
            bP.Click += (s, e) => { if (currentPage > 0) { currentPage--; txtManual.Text = manualPages[currentPage]; lblPageInfo.Text = $"{currentPage + 1} / 2"; } };
            p3.Controls.Add(lblMan);
            nav.Controls.Add(lblPageInfo); 
            nav.Controls.Add(bP); 
            nav.Controls.Add(bN);
            p3.Controls.Add(nav);
            p3.Controls.Add(txtManual);
            txtManual.BringToFront();
            mainLayout.Controls.Add(p3, 0, 1);

            // PRAVÝ DOLNÍ - HISTORIE HOVORŮ (TADY JE ZMĚNA)
            var p4 = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10), BackColor = Color.FromArgb(30, 30, 30) };
            var lblHistTitle = new Label { Text = "HISTORIE VYŘÍZENÝCH HOVORŮ", Dock = DockStyle.Top, Height = 35, Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = Color.White };
            lstHistory = new ListBox { Dock = DockStyle.Fill, BackColor = Color.FromArgb(40, 40, 40), ForeColor = Color.LightGray, Font = new Font("Consolas", 15F), BorderStyle = BorderStyle.None };
            p4.Controls.Add(lstHistory);
            p4.Controls.Add(lblHistTitle);
            lstHistory.DoubleClick += LstHistory_DoubleClick;
            mainLayout.Controls.Add(p4, 1, 1);

            // Příprava tlačítek jednotek (budou se zobrazovat v terminálu)
            SetupUnitSelection();
        }

       private void LstHistory_DoubleClick(object sender, EventArgs e)
        {
            if (lstHistory.SelectedItem is Scenario s)
            {
                string vyzadovano = string.Join(", ", s.RequiredUnits);
                string vyslano = s.UserSentUnits.Count > 0 ? string.Join(", ", s.UserSentUnits) : "Nic";
                
                Form detailForm = new Form();
                detailForm.Text = "Detail zásahu - " + s.EventName;
                detailForm.Size = new Size(450, 400);
                detailForm.StartPosition = FormStartPosition.CenterParent;
                detailForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                detailForm.BackColor = Color.White;
                detailForm.MaximizeBox = false;
                detailForm.MinimizeBox = false;

                // KLÍČOVÁ OPRAVA PRO TOPMOST:
                // Nastavíme oknu stejnou prioritu jako má hlavní hra
                detailForm.TopMost = true; 

                Label lblInfo = new Label {
                    Dock = DockStyle.Fill,
                    Padding = new Padding(20),
                    Font = new Font("Segoe UI", 10F),
                    Text = $"UDÁLOST: {s.EventName}\n" +
                        $"--------------------------------------------\n" +
                        $"VÝSLEDEK: {(s.WasCorrect ? "ÚSPĚCH (+10)" : "CHYBA (-5)")}\n\n" +
                        $"MĚLO BÝT POSLÁNO: {vyzadovano}\n" +
                        $"VY JSTE POSLALI: {vyslano}\n\n" +
                        $"POPIS: {s.Text}\n\n" +
                        $"VOLAJÍCÍ: {s.NameAnswer}\n" +
                        $"ADRESA: {s.LocationAnswer}"
                };

                if (!s.WasCorrect) lblInfo.ForeColor = Color.DarkRed;
                else lblInfo.ForeColor = Color.DarkGreen;

                detailForm.Controls.Add(lblInfo);

                // Zobrazíme okno a předáme mu 'this' (hlavní hru) jako vlastníka
                detailForm.ShowDialog(this); 
            }
        }

        private void SetupUnitSelection()
        {
            _unitSelectionPanel.Controls.Add(CreateUnitButton("Pořádková policie", Color.Blue, "Pořádková policie"));
            _unitSelectionPanel.Controls.Add(CreateUnitButton("Dopravní policie", Color.Blue, "Dopravní policie"));
            _unitSelectionPanel.Controls.Add(CreateUnitButton("Hasiči", Color.Red, "Hasiči"));
            _unitSelectionPanel.Controls.Add(CreateUnitButton("Záchranka", Color.Gold, "Záchranka"));
        }

        private Panel CreateUnitButton(string text, Color activeColor, string type) {
            var p = new Panel { Width = 180, Height = 35, BackColor = Color.FromArgb(60, 60, 60), Margin = new Padding(3), Tag = false, AccessibleName = type, Cursor = Cursors.Hand };
            p.Controls.Add(new Label { Text = text, ForeColor = Color.White, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Enabled = false, Font = new Font("Segoe UI", 8F, FontStyle.Bold) });
            p.Click += (s, e) => { bool sel = !(bool)p.Tag; p.Tag = sel; p.BackColor = sel ? activeColor : Color.FromArgb(60, 60, 60); };
            return p;
        }

        private void GenerateRandomCall()
        {
            if (_scenarios == null || _scenarios.Count == 0) return;

            // FILTR: Pokud je vybráno "Všechny", vezmi celý seznam. 
            // Jinak vyfiltruj jen ty, které odpovídají zvolené obtížnosti.
            var availableScenarios = _scenarios
                .Where(s => _selectedDifficultyFilter == "Všechny obtížnosti" || s.Difficulty == _selectedDifficultyFilter)
                .ToList();

            // Kontrola, zda pro danou obtížnost vůbec existují nějaké hovory
            if (availableScenarios.Count == 0) return;

            Random rnd = new Random();
            var scenario = availableScenarios[rnd.Next(availableScenarios.Count)];

            // Kontrola, aby stejný hovor nebyl aktivní dvakrát
            foreach (Control c in _callsFlowPanel.Controls)
                if (c.Tag is Scenario active && active.Id == scenario.Id) return;

            AddCallToDashboard(scenario);
        }

        private void AddCallToDashboard(Scenario scenario)
        {
            var card = new Panel { Width = _callsFlowPanel.Width - 35, Height = 60, BackColor = Color.White, Margin = new Padding(5), BorderStyle = BorderStyle.FixedSingle, Cursor = Cursors.Hand };
            card.Controls.Add(new Label { Text = $"📞 {scenario.Title}", Font = new Font("Segoe UI", 10F, FontStyle.Bold), AutoSize = true, Top = 20, Left = 10, Enabled = false });
            card.Click += (s, e) => {
                _currentActiveScenario = scenario; _currentActiveCard = card;
                lblNoCall.Visible = false; _activeCallPanel.Visible = true;
                _txtChatDisplay.Text = $"[DISPEČINK]: Tísňová linka, jak vám mohu pomoci?\r\n" + $"[VOLAJÍCÍ]: {scenario.Text}\r\n";  
                _questionFlowPanel.Controls.Clear();
                _questionFlowPanel.Controls.Clear();

            // JMÉNO
            AddQuestionButton("Jméno", new[] { 
                "Můžete mi uvést vaše jméno?", 
                "S kým mluvím, prosím?", 
                "Jak se jmenujete?" 
            }, scenario.NameAnswer);

            // LOKALITA
            AddQuestionButton("Lokalita", new[] { 
                "Kde přesně se nacházíte?", 
                "Jaká je vaše přesná adresa?", 
                "Můžete mi popsat, kde se místo události nachází?" 
            }, scenario.LocationAnswer);

            // ZRANĚNÍ
            AddQuestionButton("Zranění", new[] { 
                "Je tam někdo zraněný?", 
                "Jsou na místě nějaké oběti nebo zranění?", 
                "Potřebuje někdo okamžitou lékařskou pomoc?" 
            }, scenario.InjuryAnswer);

            // DETAILY
            AddQuestionButton("Detaily", new[] { 
                "Můžete mi k tomu říct víc podrobností?", 
                "Co přesně se na místě děje?", 
                "Popište mi situaci podrobněji." 
            }, scenario.DetailsAnswer);

            };
            _callsFlowPanel.Controls.Add(card);
            _callsFlowPanel.Controls.SetChildIndex(card, 0);
        }

        private void AddQuestionButton(string buttonText, string[] speechOptions, string answerText) 
{
    var b = new Button 
    { 
        Text = buttonText, 
        Width = 100, 
        Height = 35, 
        BackColor = Color.FromArgb(210, 210, 210),
        ForeColor = Color.Black, 
        FlatStyle = FlatStyle.Flat 
    };

    b.Click += (s, e) => { 
        // Náhodný výběr jedné z variant
        Random rnd = new Random();
        string vybranaVeta = speechOptions[rnd.Next(speechOptions.Length)];

        _txtChatDisplay.AppendText($"[DISPEČINK]: {vybranaVeta}\r\n");
        _txtChatDisplay.AppendText($"[VOLAJÍCÍ]: {answerText}\r\n\r\n"); 
        
        b.Enabled = false; 
        b.BackColor = Color.LightGray; 
    };

    _questionFlowPanel.Controls.Add(b);
}

        private void HandleConfirmation() {
            if (_currentActiveScenario == null) return;
            
            var sel = new List<string>();
            foreach (Control c in _unitSelectionPanel.Controls) 
                if (c is Panel p && p.Tag != null && (bool)p.Tag) sel.Add((string)p.AccessibleName);

            // Uložíme data do objektu
            _currentActiveScenario.UserSentUnits = new List<string>(sel);
            _currentActiveScenario.WasCorrect = EvaluateResponse(_currentActiveScenario, sel);

            if (_currentActiveScenario.WasCorrect) _score += 10; else _score -= 5;
            lblScore.Text = $"Skóre: {_score}";

            // Vložíme CELÝ OBJEKT do historie
            lstHistory.Items.Insert(0, _currentActiveScenario);

            // Reset herní plochy
            _callsFlowPanel.Controls.Remove(_currentActiveCard);
            _activeCallPanel.Visible = false; 
            lblNoCall.Visible = true; 
            _currentActiveScenario = null;
            foreach (Control c in _unitSelectionPanel.Controls) if (c is Panel p) { p.Tag = false; p.BackColor = Color.FromArgb(210, 210, 210); }
        }

        private bool EvaluateResponse(Scenario s, List<string> sel) {
            var req = new HashSet<string>(s.RequiredUnits ?? new());
            return req.SetEquals(new HashSet<string>(sel));
        }

        private void LoadScenarios()
{
    try
    {
        // Cesta k souboru (předpokládá se, že je u .exe souboru)
        string path = Path.Combine(Application.StartupPath, "C:\\Users\\Filip\\Desktop\\Dispatcher\\scenarios.json");

        if (File.Exists(path))
        {
            string jsonString = File.ReadAllText(path);
            
            // Nastavení pro dekódování (velká/malá písmena v JSONu nebudou vadit)
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            
            _scenarios = JsonSerializer.Deserialize<List<Scenario>>(jsonString, options) ?? new List<Scenario>();
            
            // Jen pro kontrolu do konzole/debugu
            Console.WriteLine($"Načteno {_scenarios.Count} scénářů ze souboru.");
        }
        else
        {
            // Pokud soubor neexistuje, vytvoříme aspoň jeden nouzový hovor
            _scenarios = new List<Scenario> { 
                new Scenario { Title = "CHYBA", Text = "Soubor scenarios.json nebyl nalezen!", RequiredUnits = new List<string>() } 
            };
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show("Chyba při načítání scénářů: " + ex.Message);
        _scenarios = new List<Scenario>();
    }
}
    }

    public class Scenario
    {
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string EventName { get; set; } = "";
    public string Text { get; set; } = "";
    public string Difficulty { get; set; } = "";
    public List<string> RequiredUnits { get; set; } = new();
    public string NameAnswer { get; set; } = "";
    public string LocationAnswer { get; set; } = "";
    public string InjuryAnswer { get; set; } = "";
    public string DetailsAnswer { get; set; } = "";
    public List<string> UserSentUnits { get; set; } = new();
    public bool WasCorrect { get; set; }
    public override string ToString() => $"{DateTime.Now:HH:mm} {(WasCorrect ? "[Správně]" : "[Chybně]")} - {EventName}";
    }
}
