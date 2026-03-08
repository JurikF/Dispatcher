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
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F)); 
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            this.Controls.Add(mainLayout);

            // --- KVADRANTY ---

            // LEVÝ HORNÍ - Seznam hovorů
            var p1 = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10, 70, 10, 10), BackColor = Color.FromArgb(30, 30, 30) };
            var lblCallsTitle = new Label { Text = "PŘÍCHOZÍ HOVORY", Dock = DockStyle.Top, Height = 30, Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = Color.Gold };
            _callsFlowPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.FromArgb(40, 40, 40) };
            p1.Controls.Add(_callsFlowPanel);
            p1.Controls.Add(lblCallsTitle);
            mainLayout.Controls.Add(p1, 0, 0);

            // PRAVÝ HORNÍ - KOMPLETNÍ TERMINÁL (Chat + Jednotky + Potvrdit)
            var p2 = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(30, 30, 30), Padding = new Padding(10, 70, 10, 10) };
            lblNoCall = new Label { Text = "TERMINÁL NEAKTIVNÍ\r\nVyberte hovor ze seznamu", ForeColor = Color.White, Font = new Font("Consolas", 14F), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter };
            
            _activeCallPanel = new Panel { Dock = DockStyle.Fill, Visible = false };

            _txtChatDisplay = new TextBox { Multiline = true, ReadOnly = true, Dock = DockStyle.Top, Height = 250, BackColor = Color.Black, ForeColor = Color.White, Font = new Font("Consolas", 12F), BorderStyle = BorderStyle.None, ScrollBars = ScrollBars.Vertical};
            var lblQuestion = new Label { Text = "MOŽNÉ OTÁZKY:", Dock = DockStyle.Top, Height = 25, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.White };
            _questionFlowPanel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(0, 5, 0, 0) };
            var lblUnits = new Label { Text = "DOSTUPNÉ JEDNOTKY:", Dock = DockStyle.Top, Height = 25, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.White };
            _unitSelectionPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(35, 35, 35), Padding = new Padding(5) };
            
            var btnConfirm = new Button { Text = "POTVRDIT A VYSLAT", Dock = DockStyle.Bottom, Height = 50, BackColor = Color.FromArgb(50, 50, 50), ForeColor = Color.Gold, Font = new Font("Segoe UI", 12F, FontStyle.Bold), FlatStyle = FlatStyle.Flat };
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

            // LEVÝ DOLNÍ - LUSTRACE (Osoba i Auto)
            var p3 = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15), BackColor = Color.FromArgb(30, 30, 30) };

            var lblLusTitle = new Label { 
                Text = "LUSTRACE OSOB A VOZIDEL", 
                Dock = DockStyle.Top, Height = 30, 
                Font = new Font("Segoe UI", 12F, FontStyle.Bold), 
                ForeColor = Color.Gold 
            };

            // Kontejner pro vstupy
            var pnlInputs = new Panel { Dock = DockStyle.Top, Height = 110 };

            // --- LEVÝ SLOUPEC (Osoba) ---
            var lblPersonInfo = new Label { 
                Text = "Vyhledat osobu:", 
                ForeColor = Color.Gold, 
                Font = new Font("Segoe UI", 12F), 
                AutoSize = true, 
                Location = new Point(0, 5),
            };

            var txtSearchName = new TextBox { 
                PlaceholderText = "Jméno a Příjmení", 
                Width = 210, Location = new Point(0, 35), 
                Font = new Font("Segoe UI", 10F) 
            };

            var txtSearchBirth = new TextBox { 
                PlaceholderText = "Narození (DD.MM.RRRR)",
                Width = 210, Location = new Point(0, 65), 
                Font = new Font("Segoe UI", 10F) 
            };

            txtSearchBirth.KeyPress += (s, e) => {
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
                {
                    e.Handled = true;
                }
            };

            var lblCarInfo = new Label { 
                Text = "Vyhledat vozidlo:", 
                ForeColor = Color.Gold, 
                Font = new Font("Segoe UI", 12F), 
                AutoSize = true, 
                Location = new Point(220, 5),
            };

            var txtSearchSPZ = new TextBox { 
                PlaceholderText = "Zadejte SPZ vozidla",  
                Width = 210, Location = new Point(220, 35), 
                Font = new Font("Segoe UI", 10F), 
                CharacterCasing = CharacterCasing.Upper,
            };

            // Tlačítko vyhledat
            var btnSearch = new Button { 
                Text = "VYHLEDAT", 
                Width = 400, Height = 60, 
                Location = new Point(480, 5), 
                BackColor = Color.FromArgb(50, 50, 50), 
                ForeColor = Color.Gold, 
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };

            // Výsledkové pole
            var txtLustraceResult = new TextBox { 
                Multiline = true, ReadOnly = true, Dock = DockStyle.Fill, 
                BackColor = Color.Black, ForeColor = Color.White, 
                Font = new Font("Consolas", 11F), BorderStyle = BorderStyle.FixedSingle,
                ScrollBars = ScrollBars.Vertical
            };

            btnSearch.Click += (s, e) => {
                PerformLustrace(txtSearchName.Text, txtSearchBirth.Text, txtSearchSPZ.Text, txtLustraceResult);

                txtSearchName.Clear();
                txtSearchBirth.Clear();
                txtSearchSPZ.Clear();
            };

            pnlInputs.Controls.Add(lblPersonInfo);
            pnlInputs.Controls.Add(txtSearchName);
            pnlInputs.Controls.Add(txtSearchBirth);
            pnlInputs.Controls.Add(lblCarInfo);
            pnlInputs.Controls.Add(txtSearchSPZ);
            pnlInputs.Controls.Add(btnSearch);

            p3.Controls.Add(txtLustraceResult);
            p3.Controls.Add(pnlInputs);
            p3.Controls.Add(lblLusTitle);
            txtLustraceResult.BringToFront();

            mainLayout.Controls.Add(p3, 0, 1);

            // PRAVÝ DOLNÍ - HISTORIE HOVORŮ
            var p4 = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10), BackColor = Color.FromArgb(30, 30, 30) };
            var lblHistTitle = new Label { Text = "HISTORIE VYŘÍZENÝCH HOVORŮ", Dock = DockStyle.Top, Height = 30, Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = Color.Gold };
            lstHistory = new ListBox { Dock = DockStyle.Fill, BackColor = Color.FromArgb(40, 40, 40), ForeColor = Color.LightGray, Font = new Font("Consolas", 15F), BorderStyle = BorderStyle.None };
            p4.Controls.Add(lstHistory);
            p4.Controls.Add(lblHistTitle);
            lstHistory.DoubleClick += LstHistory_DoubleClick;
            mainLayout.Controls.Add(p4, 1, 1);

            SetupUnitSelection();
        }

       private void LstHistory_DoubleClick(object sender, EventArgs e)
        {
            if (lstHistory.SelectedItem is Scenario s)
            {
                string vyzadovano = string.Join(", ", s.RequiredUnits);
                string vyslano = s.UserSentUnits.Count > 0 ? string.Join(", ", s.UserSentUnits) : "";
                
                Form detailForm = new Form();
                detailForm.Text = "Detail zásahu - " + s.EventName;
                detailForm.Size = new Size(450, 400);
                detailForm.StartPosition = FormStartPosition.CenterParent;
                detailForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                detailForm.BackColor = Color.White;
                detailForm.MaximizeBox = false;
                detailForm.MinimizeBox = false;
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
            _unitSelectionPanel.Controls.Add(CreateUnitButton("Pořádková policie", Color.FromArgb(30, 144, 255), "Pořádková policie"));
            _unitSelectionPanel.Controls.Add(CreateUnitButton("Dopravní policie", Color.FromArgb(173, 255, 47), "Dopravní policie"));
            _unitSelectionPanel.Controls.Add(CreateUnitButton("Hasiči", Color.Red, "Hasiči"));
            _unitSelectionPanel.Controls.Add(CreateUnitButton("Záchranka", Color.Gold, "Záchranka"));
        }

        private Panel CreateUnitButton(string text, Color activeColor, string type) {
            var p = new Panel { 
                Width = 180, 
                Height = 35, 
                BackColor = Color.FromArgb(210, 210, 210), 
                Margin = new Padding(3), 
                Tag = false, 
                AccessibleName = type, 
                Cursor = Cursors.Hand 
            };

            var lbl = new Label { 
                Text = text, 
                ForeColor = Color.Black,
                Dock = DockStyle.Fill, 
                TextAlign = ContentAlignment.MiddleCenter, 
                Enabled = false, 
                Font = new Font("Segoe UI", 8F, FontStyle.Bold) 
            };
            p.Controls.Add(lbl);

            p.Click += (s, e) => { 
                bool sel = !(bool)p.Tag; 
                p.Tag = sel; 
                
                if (sel) {
                    p.BackColor = activeColor;
                    lbl.ForeColor = Color.White;
                } else {
                    p.BackColor = Color.FromArgb(210, 210, 210);
                    lbl.ForeColor = Color.Black;
                }
            };
            return p;
        }

        private void PerformLustrace(string name, string birth, string spz, TextBox output)
        {
            try
            {
                output.ForeColor = Color.White;
                output.Clear();

                // 1. PŘEDNOST MÁ SPZ (pokud není prázdná)
                if (!string.IsNullOrWhiteSpace(spz))
                {
                    string rootPath = AppContext.BaseDirectory;
                    string jsonPath = Path.Combine(rootPath, "cars.json");
                    if (File.Exists(jsonPath))
                    {
                        var cars = JsonSerializer.Deserialize<List<Car>>(File.ReadAllText(jsonPath), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        var car = cars?.FirstOrDefault(c => c.SPZ.Equals(spz.Trim(), StringComparison.OrdinalIgnoreCase));

                        if (car != null)
                        {
                            if (car.IsStolen) output.ForeColor = Color.Red;
                            output.Text = $"VÝSLEDEK LUSTRACE VOZIDLA:\r\n" +
                                        $"--------------------------------------\r\n" +
                                        $"SPZ:      {car.SPZ.ToUpper()}\r\n" +
                                        $"MODEL:    {car.Model}\r\n" +
                                        $"MAJITEL:  {car.Owner}\r\n" +
                                        $"STAV:     {(car.IsStolen ? "!!! VOZIDLO JE KRADENÉ !!!" : "V pořádku (není hlášeno)")}\r\n" +
                                        $"ZÁZNAM:   {car.Note}";
                            return;
                        }
                    }
                }

                
                if (!string.IsNullOrWhiteSpace(name))
                {
                    string rootPath = AppContext.BaseDirectory;
                    string jsonPath = Path.Combine(rootPath, "people.json");
                    if (File.Exists(jsonPath))
                    {
                        var people = JsonSerializer.Deserialize<List<Person>>(File.ReadAllText(jsonPath), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        var person = people?.FirstOrDefault(p => 
                            (p.FirstName + " " + p.LastName).Equals(name.Trim(), StringComparison.OrdinalIgnoreCase) && 
                            p.BirthDate.Trim() == birth.Trim());

                        if (person != null)
                        {
                            if (person.IsWanted) output.ForeColor = Color.Red;
                            output.Text = $"VÝSLEDEK LUSTRACE OSOBY:\r\n" +
                                        $"--------------------------------------\r\n" +
                                        $"OSOBA:    {person.FirstName.ToUpper()} {person.LastName.ToUpper()}\r\n" +
                                        $"NAR.:     {person.BirthDate}\r\n" +
                                        $"STAV:     {(person.IsWanted ? "!!! OSOBA V PÁTRÁNÍ !!!" : "Negativní")}\r\n" +
                                        $"ZÁZNAM:   {person.Note}";
                            return;
                        }
                    }
                }

                output.Text = "SYSTÉM: Žádný záznam neodpovídá zadaným parametrům.";
            }
            catch (Exception ex)
            {
                output.ForeColor = Color.Orange;
                output.Text = "CHYBA DATABÁZE: " + ex.Message;
            }
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
            _currentActiveScenario = scenario; 
            _currentActiveCard = card;
            lblNoCall.Visible = false; 
            _activeCallPanel.Visible = true;
            bool isPoliceCall = scenario.Title.Contains("👮");
            string callerName = scenario.Title.Contains("👮") ? scenario.NameAnswer : "VOLAJÍCÍ";
            
            if (isPoliceCall)
            {
                // U policie začíná rovnou hlášení hlídky (bez tvého úvodu)
                _txtChatDisplay.Text = $"[{callerName.ToUpper()}]: {scenario.Text}\r\n\r\n";
            }
            else
            {
                // U civilistů zůstává standardní pozdrav operátora
                _txtChatDisplay.Text = $"[DISPEČINK]: Tísňová linka, jak vám mohu pomoci?\r\n" + 
                                       $"[VOLAJÍCÍ]: {scenario.Text}\r\n\r\n";
            }
            
            _questionFlowPanel.Controls.Clear();

            // ROZCESTNÍK: Je to policie, nebo civilista?
            if (scenario.Title.Contains("👮"))
            {   
                AddQuestionButton("POLOHA", new[] { 
                "Udejte vaši přesnou polohu.", 
                "Kde se právě nacházíte?", 
                "Vaše GPS nebo ulice?"
            }, scenario.LocationAnswer, callerName);

                // 1. SITUACE: STŘELBA / NAPADENÍ
                if (scenario.Text.Contains("střelb") || scenario.Text.Contains("napaden"))
                {
                    AddQuestionButton("STAV NA MÍSTĚ", new[] { "Jaká je situace na místě? Jsou tam zranění?" }, scenario.DetailsAnswer, callerName);
                    AddQuestionButton("POSÍLÁM POSILY", new[] { "Posílám posily." }, "Rozumím vesmíre.", callerName);
                }
                // 2. SITUACE: PRONÁSLEDOVÁNÍ VOZIDLA
                else if (scenario.Text.Contains("pronásledujeme") || scenario.Text.Contains("vozidlo"))
                {
                    Button btnOdcizeno = null;
                    Button btnNeniOdcizeno = null;

                    AddQuestionButton("ŽÁDAT SPZ", new[] { "Nadiktujte SPZ a popis vozidla." }, scenario.DetailsAnswer, callerName);
                    AddQuestionButton("POSÍLÁM POSILY", new[] { "Rozumím, posílám k vám další hlídky." }, "Rozumím.", callerName);
                    btnOdcizeno = CreateQuestionButton("ODCIZENO", new[] { "Vozidlo je nahlášeno jako kradené!" }, "Potvrzuji, vozidlo je kradené!", callerName);
                    btnNeniOdcizeno = CreateQuestionButton("NEODCIZENO", new[] { "Vozidlo není nahlášeno jako kradené!" }, "Potvrzuji, vozidlo není kradené!", callerName);

                    btnOdcizeno.Click += (s, e) => {
                    btnOdcizeno.Enabled = false;
                    btnNeniOdcizeno.Enabled = false;    
                    btnNeniOdcizeno.BackColor = Color.Gray;
                };

                btnNeniOdcizeno.Click += (s, e) => {
                    btnNeniOdcizeno.Enabled = false;
                    btnOdcizeno.Enabled = false;
                    btnOdcizeno.BackColor = Color.Gray;
                };

                _questionFlowPanel.Controls.Add(btnOdcizeno);
                _questionFlowPanel.Controls.Add(btnNeniOdcizeno);
                }
                // 3. SITUACE: STANDARDNÍ LUSTRACE (původní)
                else
                {
                    Button btnJeHledany = null;
                    Button btnNeniHledany = null;

                    AddQuestionButton("ŽÁDAT ÚDAJE", new[] { "Nadiktujte mi jméno a datum narození." }, scenario.DetailsAnswer, callerName);
                    btnJeHledany = CreateQuestionButton("JE HLEDANÝ", new[] { "Osoba je v pátrání!" }, "Rozumím, žádáme posilu na naši pozici.", callerName);
                    btnNeniHledany = CreateQuestionButton("NENÍ HLEDANÝ", new[] { "Osoba je čistá." }, "Díky, končíme.", callerName);

                // Přidáme logiku pro zamknutí
                btnJeHledany.Click += (s, e) => {
                    btnJeHledany.Enabled = false;
                    btnNeniHledany.Enabled = false;
                    btnNeniHledany.BackColor = Color.Gray;
                };

                btnNeniHledany.Click += (s, e) => {
                    btnNeniHledany.Enabled = false;
                    btnJeHledany.Enabled = false;
                    btnJeHledany.BackColor = Color.Gray;
                };

                _questionFlowPanel.Controls.Add(btnJeHledany);
                _questionFlowPanel.Controls.Add(btnNeniHledany);
                }
            }
            else
            {
                // --- STANDARDNÍ TLAČÍTKA PRO CIVILISTY ---
                AddQuestionButton("Jméno", new[] { "S kým mluvím prosím?", "Vaše jméno?", "Jak se jmenujete?" }, scenario.NameAnswer, callerName);
                AddQuestionButton("Lokalita", new[] { "Kde přesně jste?", "Udejte polohu." }, scenario.LocationAnswer, callerName);
                AddQuestionButton("Zranění", new[] { "Jsou tam zranění?", "Je někdo zraněný?" }, scenario.InjuryAnswer, callerName);
                AddQuestionButton("Detaily", new[] { "Co se tam děje?", "Popište mi situaci." }, scenario.DetailsAnswer, callerName);
            }
        };
            _callsFlowPanel.Controls.Add(card);
            _callsFlowPanel.Controls.SetChildIndex(card, 0);
        }

        private void AddQuestionButton(string buttonText, string[] speechOptions, string answerText, string callerName) 
{
    var b = new Button 
    { 
        Text = buttonText, 
        Width = 110, 
        Height = 35, 
        BackColor = Color.FromArgb(210, 210, 210),
        ForeColor = Color.Black, 
        FlatStyle = FlatStyle.Flat,
        TextAlign = ContentAlignment.MiddleCenter
    };

    b.Click += (s, e) => { 
        // Náhodný výběr jedné z variant
        Random rnd = new Random();
        string vybranaVeta = speechOptions[rnd.Next(speechOptions.Length)];

        _txtChatDisplay.AppendText($"[DISPEČINK]: {vybranaVeta}\r\n");
        _txtChatDisplay.AppendText($"[{callerName.ToUpper()}]: {answerText}\r\n\r\n"); 
        
        b.Enabled = false; 
        b.BackColor = Color.LightGray;
    };

    _questionFlowPanel.Controls.Add(b);
}

        // Tato metoda funguje stejně jako AddQuestionButton, ale vrací vytvořené tlačítko
        private Button CreateQuestionButton(string buttonText, string[] speechOptions, string answerText, string callerName)
        {
            var b = new Button
            {
                Text = buttonText,
                Width = 110,
                Height = 35,
                BackColor = Color.FromArgb(210, 210, 210),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleCenter
            };

            b.Click += (s, e) => {
                Random rnd = new Random();
                string vybranaVeta = speechOptions[rnd.Next(speechOptions.Length)];
                
                // 1. Zpráva od dispečera
                _txtChatDisplay.AppendText($"[DISPEČINK]: {vybranaVeta}\r\n");
                
                // 2. Zpráva od jednotky (použije se callerName předaný z AddCallToDashboard)
                // Přidali jsme .ToUpper() pro autentičnost
                _txtChatDisplay.AppendText($"[{callerName.ToUpper()}]: {answerText}\r\n\r\n");
                
                b.Enabled = false;
                b.BackColor = Color.LightGray;
            };

            return b;
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
            // Pokud je osoba v pátrání (toto by šlo vylepšit detekcí v kódu, 
            // ale pro teď to necháme na s.RequiredUnits v JSONu)
            
            // Pokud v JSONu nejsou žádné vyžadované jednotky, a hráč žádné neposlal, je to OK.
            if (s.RequiredUnits.Count == 0 && sel.Count == 0) return true;

            var req = new HashSet<string>(s.RequiredUnits ?? new());
            var sent = new HashSet<string>(sel);
            
            return req.SetEquals(sent);
        }

        private void LoadScenarios()
{
    try
    {
        // Cesta k souboru (předpokládá se, že je u .exe souboru)
        string rootPath = AppContext.BaseDirectory;
        string jsonPath = Path.Combine(rootPath, "scenarios.json");

        if (File.Exists(jsonPath))
        {
            string jsonString = File.ReadAllText(jsonPath);
            
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

    public class Person
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string BirthDate { get; set; } = "";
        public bool IsWanted { get; set; }
        public string Note { get; set; } = "";
    }

    public class Car 
    {
        public string SPZ { get; set; } = "";
        public string Model { get; set; } = "";
        public string Owner { get; set; } = "";
        public bool IsStolen { get; set; }
        public string Note { get; set; } = "";
    }
}
