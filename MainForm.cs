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
        private string _selectedDifficultyFilter = "Všechny obtížnosti";
        private Timer _callTimer;

        // Terminál hovoru
        private Panel _activeCallPanel;
        private TextBox _txtChatDisplay;
        private FlowLayoutPanel _questionFlowPanel;
        private FlowLayoutPanel _unitSelectionPanel; 
        private Scenario _currentActiveScenario;
        private Panel _currentActiveCard;
        private Label lblNoCall;

        // Historie
        private ListBox lstHistory;

        public MainForm(string vybranaObtiznost)
        {
            _selectedDifficultyFilter = vybranaObtiznost;

            _callTimer = new Timer
            {
                Interval = 5000 
            };

            _callTimer.Tick += (s, e) =>
            {
                GenerateRandomCall();
                if (_callTimer.Interval == 5000)
                {
                    _callTimer.Interval = 30000;
                }
            };

            InitializeUI();
            LoadScenarios();

            _callTimer.Start();
        }

        private void InitializeUI()
        {
            bool isEn = GameSettings.CurrentLanguage == "EN";

            this.Text = "Dispatcher Simulator";
            this.BackColor = Color.FromArgb(20, 20, 20);
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;

            // 1. HORNÍ PANEL (Menu)
            var topPanel = new Panel { Height = 60, Dock = DockStyle.Top, BackColor = Color.FromArgb(79, 81, 84), Padding = new Padding(15, 5, 15, 5) };
            this.Controls.Add(topPanel);

            lblScore = new Label { Text = isEn ? "Score: 0" : "Skóre: 0", ForeColor = Color.White, Font = new Font("Segoe UI", 18F, FontStyle.Bold), Dock = DockStyle.Left, AutoSize = true, TextAlign = ContentAlignment.MiddleLeft };
            topPanel.Controls.Add(lblScore);

            var btnClose = new Button { Text = "✕", Dock = DockStyle.Right, Width = 50, BackColor = Color.DarkRed, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 12F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => Application.Exit();
            
            var btnMenu = new Button { Text = isEn ? "BACK TO MENU" : "ZPĚT DO MENU", Dock = DockStyle.Right, Width = 150, BackColor = Color.FromArgb(0, 180, 0), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnMenu.FlatAppearance.BorderSize = 0;
            btnMenu.Click += (s, e) => { if (MessageBox.Show(isEn ? "Return to menu?" : "Vrátit se do menu?", "Menu", MessageBoxButtons.YesNo) == DialogResult.Yes) Application.Restart(); };

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

            // LEVÝ HORNÍ
            var p1 = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10, 70, 10, 10), BackColor = Color.FromArgb(30, 30, 30) };
            var lblCallsTitle = new Label { Text = isEn ? "INCOMING CALLS" : "PŘÍCHOZÍ HOVORY", Dock = DockStyle.Top, Height = 30, Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = Color.Gold };
            _callsFlowPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.FromArgb(40, 40, 40) };
            p1.Controls.Add(_callsFlowPanel);
            p1.Controls.Add(lblCallsTitle);
            mainLayout.Controls.Add(p1, 0, 0);

            // PRAVÝ HORNÍ
            var p2 = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(30, 30, 30), Padding = new Padding(10, 70, 10, 10) };
            lblNoCall = new Label { Text = isEn ? "TERMINAL INACTIVE\r\nSelect a call from the list" : "TERMINÁL NEAKTIVNÍ\r\nVyberte hovor ze seznamu", ForeColor = Color.White, Font = new Font("Consolas", 14F), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter };
            
            _activeCallPanel = new Panel { Dock = DockStyle.Fill, Visible = false };
            _txtChatDisplay = new TextBox { Multiline = true, ReadOnly = true, Dock = DockStyle.Top, Height = 250, BackColor = Color.Black, ForeColor = Color.White, Font = new Font("Consolas", 12F), BorderStyle = BorderStyle.None, ScrollBars = ScrollBars.Vertical};
            var lblQuestion = new Label { Text = isEn ? "QUESTIONS:" : "MOŽNÉ OTÁZKY:", Dock = DockStyle.Top, Height = 25, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.White };
            _questionFlowPanel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(0, 5, 0, 0) };
            var lblUnits = new Label { Text = isEn ? "AVAILABLE UNITS:" : "DOSTUPNÉ JEDNOTKY:", Dock = DockStyle.Top, Height = 25, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.White };
            _unitSelectionPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(35, 35, 35), Padding = new Padding(5) };
            
            var btnConfirm = new Button { Text = isEn ? "CONFIRM AND DISPATCH" : "POTVRDIT A VYSLAT", Dock = DockStyle.Bottom, Height = 50, BackColor = Color.FromArgb(50, 50, 50), ForeColor = Color.Gold, Font = new Font("Segoe UI", 12F, FontStyle.Bold), FlatStyle = FlatStyle.Flat };
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

            // LEVÝ DOLNÍ
            var p3 = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15), BackColor = Color.FromArgb(30, 30, 30) };
            var lblLusTitle = new Label { Text = isEn ? "PERSON & VEHICLE DATABASE" : "LUSTRACE OSOB A VOZIDEL", Dock = DockStyle.Top, Height = 30, Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = Color.Gold };
            var pnlInputs = new Panel { Dock = DockStyle.Top, Height = 110 };
            var lblPersonInfo = new Label { Text = isEn ? "Search person:" : "Vyhledat osobu:", ForeColor = Color.Gold, Font = new Font("Segoe UI", 12F), AutoSize = true, Location = new Point(0, 5) };
            var txtSearchName = new TextBox { PlaceholderText = isEn ? "Name and Surname" : "Jméno a Příjmení", Width = 210, Location = new Point(0, 35), Font = new Font("Segoe UI", 10F) };
            var txtSearchBirth = new TextBox { PlaceholderText = isEn ? "Birth (DD.MM.YYYY)" : "Narození (DD.MM.RRRR)", Width = 210, Location = new Point(0, 65), Font = new Font("Segoe UI", 10F) };
            txtSearchBirth.KeyPress += (s, e) => { if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.')) e.Handled = true; };
            var lblCarInfo = new Label { Text = isEn ? "Search vehicle:" : "Vyhledat vozidlo:", ForeColor = Color.Gold, Font = new Font("Segoe UI", 12F), AutoSize = true, Location = new Point(220, 5) };
            var txtSearchSPZ = new TextBox { PlaceholderText = isEn ? "License Plate" : "Zadejte SPZ vozidla", Width = 210, Location = new Point(220, 35), Font = new Font("Segoe UI", 10F), CharacterCasing = CharacterCasing.Upper };
            var btnSearch = new Button { Text = isEn ? "SEARCH" : "VYHLEDAT", Width = 400, Height = 60, Location = new Point(480, 5), BackColor = Color.FromArgb(50, 50, 50), ForeColor = Color.Gold, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            var txtLustraceResult = new TextBox { Multiline = true, ReadOnly = true, Dock = DockStyle.Fill, BackColor = Color.Black, ForeColor = Color.White, Font = new Font("Consolas", 11F), BorderStyle = BorderStyle.FixedSingle, ScrollBars = ScrollBars.Vertical };
            btnSearch.Click += (s, e) => { PerformLustrace(txtSearchName.Text, txtSearchBirth.Text, txtSearchSPZ.Text, txtLustraceResult); txtSearchName.Clear(); txtSearchBirth.Clear(); txtSearchSPZ.Clear(); };

            pnlInputs.Controls.Add(lblPersonInfo); pnlInputs.Controls.Add(txtSearchName); pnlInputs.Controls.Add(txtSearchBirth);
            pnlInputs.Controls.Add(lblCarInfo); pnlInputs.Controls.Add(txtSearchSPZ); pnlInputs.Controls.Add(btnSearch);
            p3.Controls.Add(txtLustraceResult); p3.Controls.Add(pnlInputs); p3.Controls.Add(lblLusTitle);
            txtLustraceResult.BringToFront();
            mainLayout.Controls.Add(p3, 0, 1);

            // PRAVÝ DOLNÍ
            var p4 = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10), BackColor = Color.FromArgb(30, 30, 30) };
            var lblHistTitle = new Label { Text = isEn ? "CALL HISTORY" : "HISTORIE VYŘÍZENÝCH HOVORŮ", Dock = DockStyle.Top, Height = 30, Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = Color.Gold };
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
                bool isEn = GameSettings.CurrentLanguage == "EN";
                string vyzadovano = string.Join(", ", s.RequiredUnits);
                string vyslano = s.UserSentUnits.Count > 0 ? string.Join(", ", s.UserSentUnits) : "";
                
                Form detailForm = new Form();
                detailForm.Text = (isEn ? "Call Detail - " : "Detail zásahu - ") + s.EventName;
                detailForm.Size = new Size(450, 400);
                detailForm.StartPosition = FormStartPosition.CenterParent;
                detailForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                detailForm.BackColor = Color.White;
                detailForm.TopMost = true; 

                Label lblInfo = new Label {
                    Dock = DockStyle.Fill, Padding = new Padding(20), Font = new Font("Segoe UI", 10F),
                    Text = $"{(isEn ? "EVENT" : "UDÁLOST")}: {s.EventName}\n" +
                        $"--------------------------------------------\n" +
                        $"{(isEn ? "RESULT" : "VÝSLEDEK")}: {(s.WasCorrect ? (isEn ? "SUCCESS (+10)" : "ÚSPĚCH (+10)") : (isEn ? "ERROR (-5)" : "CHYBA (-5)"))}\n\n" +
                        $"{(isEn ? "REQUIRED" : "MĚLO BÝT")}: {vyzadovano}\n" +
                        $"{(isEn ? "SENT" : "VYSLÁNO")}: {vyslano}\n\n" +
                        $"{(isEn ? "DESCRIPTION" : "POPIS")}: {s.Text}\n\n" +
                        $"{(isEn ? "CALLER" : "VOLAJÍCÍ")}: {s.NameAnswer}\n" +
                        $"{(isEn ? "LOCATION" : "ADRESA")}: {s.LocationAnswer}"
                };

                lblInfo.ForeColor = s.WasCorrect ? Color.DarkGreen : Color.DarkRed;
                detailForm.Controls.Add(lblInfo);
                detailForm.ShowDialog(this); 
            }
        }

        private void SetupUnitSelection()
        {
            bool isEn = GameSettings.CurrentLanguage == "EN";

            // CreateUnitButton(Text na tlačítku, Barva, AccessibleName/ID pro porovnání)
            _unitSelectionPanel.Controls.Add(CreateUnitButton(
                isEn ? "Patrol Police" : "Pořádková policie", 
                Color.FromArgb(30, 144, 255), 
                isEn ? "Patrol Police" : "Pořádková policie"));

            _unitSelectionPanel.Controls.Add(CreateUnitButton(
                isEn ? "Traffic Police" : "Dopravní policie", 
                Color.FromArgb(173, 255, 47), 
                isEn ? "Traffic Police" : "Dopravní policie"));

            _unitSelectionPanel.Controls.Add(CreateUnitButton(
                isEn ? "Firefighters" : "Hasiči", 
                Color.Red, 
                isEn ? "Firefighters" : "Hasiči"));

            _unitSelectionPanel.Controls.Add(CreateUnitButton(
                isEn ? "EMS" : "Záchranka", 
                Color.Gold, 
                isEn ? "EMS" : "Záchranka"));
        }

        private Panel CreateUnitButton(string text, Color activeColor, string type) {
            var p = new Panel { Width = 180, Height = 35, BackColor = Color.FromArgb(210, 210, 210), Margin = new Padding(3), Tag = false, AccessibleName = type, Cursor = Cursors.Hand };
            var lbl = new Label { Text = text, ForeColor = Color.Black, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Enabled = false, Font = new Font("Segoe UI", 8F, FontStyle.Bold) };
            p.Controls.Add(lbl);
            p.Click += (s, e) => { 
                bool sel = !(bool)p.Tag; p.Tag = sel; 
                p.BackColor = sel ? activeColor : Color.FromArgb(210, 210, 210);
                lbl.ForeColor = sel ? Color.White : Color.Black;
            };
            return p;
        }

        private void PerformLustrace(string name, string birth, string spz, TextBox output)
        {
            bool isEn = GameSettings.CurrentLanguage == "EN";
            try 
            {
                output.ForeColor = Color.White; 
                output.Clear();
                string rootPath = AppContext.BaseDirectory;

                // Lustrace Vozidla
                if (!string.IsNullOrWhiteSpace(spz)) 
                {
                    string jsonPath = Path.Combine(rootPath, "cars.json");
                    if (File.Exists(jsonPath)) 
                    {
                        var cars = JsonSerializer.Deserialize<List<Car>>(File.ReadAllText(jsonPath), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        var car = cars?.FirstOrDefault(c => c.SPZ.Equals(spz.Trim(), StringComparison.OrdinalIgnoreCase));
                        
                        if (car != null) 
                        {
                            if (car.IsStolen) output.ForeColor = Color.Red;
                            
                            string header = isEn ? "VEHICLE DATABASE RESULT:" : "VÝSLEDEK LUSTRACE VOZIDLA:";
                            string modelLbl = isEn ? "MODEL" : "MODEL";
                            string ownerLbl = isEn ? "OWNER" : "MAJITEL";
                            string statusLbl = isEn ? "STATUS" : "STAV";
                            string noteLbl = isEn ? "NOTE" : "POZNÁMKA";
                            string statusVal = car.IsStolen ? (isEn ? "STOLEN" : "KRADENÉ") : (isEn ? "Clear" : "V pořádku");

                            output.Text = $"{header}\r\n----------------------\r\n" +
                                        $"SPZ: {car.SPZ}\r\n" +
                                        $"{modelLbl}: {car.Model}\r\n" +
                                        $"{ownerLbl}: {car.Owner}\r\n" +
                                        $"{statusLbl}: {statusVal}\r\n" +
                                        $"{noteLbl}: {car.Note}";
                            return;
                        }
                    }
                }

                // Lustrace Osoby
                if (!string.IsNullOrWhiteSpace(name)) 
                {
                    string jsonPath = Path.Combine(rootPath, "people.json");
                    if (File.Exists(jsonPath)) 
                    {
                        var people = JsonSerializer.Deserialize<List<Person>>(File.ReadAllText(jsonPath), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        var person = people?.FirstOrDefault(p => (p.FirstName + " " + p.LastName).Equals(name.Trim(), StringComparison.OrdinalIgnoreCase) && p.BirthDate.Trim() == birth.Trim());
                        
                        if (person != null) 
                        {
                            if (person.IsWanted) output.ForeColor = Color.Red;
                            
                            string header = isEn ? "PERSON DATABASE RESULT:" : "VÝSLEDEK LUSTRACE OSOBY:";
                            string nameLbl = isEn ? "NAME" : "JMÉNO";
                            string bornLbl = isEn ? "BORN" : "NAROZEN";
                            string statusLbl = isEn ? "STATUS" : "STAV";
                            string noteLbl = isEn ? "NOTE" : "POZNÁMKA";
                            string statusVal = person.IsWanted ? (isEn ? "WANTED" : "PÁTRÁNÍ") : (isEn ? "Clear" : "Negativní");

                            output.Text = $"{header}\r\n----------------------\r\n" +
                                        $"{nameLbl}: {person.FirstName} {person.LastName}\r\n" +
                                        $"{bornLbl}: {person.BirthDate}\r\n" +
                                        $"{statusLbl}: {statusVal}\r\n" +
                                        $"{noteLbl}: {person.Note}";
                            return;
                        }
                    }
                }

                output.Text = isEn ? "SYSTEM: No records found." : "SYSTÉM: Žádný záznam neodpovídá parametrům.";
            }
            catch (Exception ex) 
            { 
                output.ForeColor = Color.Orange; 
                output.Text = "ERR: " + ex.Message; 
            }
        }
        
        private void GenerateRandomCall()
        {
            if (_scenarios == null || _scenarios.Count == 0) return;
            var availableScenarios = _scenarios.Where(s => _selectedDifficultyFilter == "Všechny obtížnosti" || _selectedDifficultyFilter == "All difficulties" || s.Difficulty == _selectedDifficultyFilter).ToList();
            if (availableScenarios.Count == 0) return;
            Random rnd = new Random();
            var scenario = availableScenarios[rnd.Next(availableScenarios.Count)];
            foreach (Control c in _callsFlowPanel.Controls) if (c.Tag is Scenario active && active.Id == scenario.Id) return;
            AddCallToDashboard(scenario);
        }

        private void AddCallToDashboard(Scenario scenario)
        {
            bool isEn = GameSettings.CurrentLanguage == "EN";
            var card = new Panel { Width = _callsFlowPanel.Width - 35, Height = 60, BackColor = Color.White, Margin = new Padding(5), BorderStyle = BorderStyle.FixedSingle, Cursor = Cursors.Hand, Tag = scenario };
            card.Controls.Add(new Label { Text = $"📞 {scenario.Title}", Font = new Font("Segoe UI", 10F, FontStyle.Bold), AutoSize = true, Top = 20, Left = 10, Enabled = false });
            
            card.Click += (s, e) => {
                _currentActiveScenario = scenario; _currentActiveCard = card; lblNoCall.Visible = false; _activeCallPanel.Visible = true;
                bool isPolice = scenario.Title.Contains("👮");
                string callerName = isPolice ? scenario.NameAnswer : (isEn ? "CALLER" : "VOLAJÍCÍ");

                if (isPolice) _txtChatDisplay.Text = $"[{callerName.ToUpper()}]: {scenario.Text}\r\n\r\n";
                else _txtChatDisplay.Text = $"[{(isEn ? "DISPATCH" : "DISPEČINK")}]: {(isEn ? "Emergency line, how can I help?" : "Tísňová linka, jak vám mohu pomoci?")}\r\n[{callerName}]: {scenario.Text}\r\n\r\n";
                
                _questionFlowPanel.Controls.Clear();
                if (isPolice) {
                    AddQuestionButton(isEn ? "LOCATION" : "POLOHA", new[] { isEn ? "What is your location?" : "Udejte vaši polohu." }, scenario.LocationAnswer, callerName);
                    
                    if (scenario.Text.ToLower().Contains("střelb") || scenario.Text.ToLower().Contains("shot") || 
                        scenario.Text.ToLower().Contains("napaden") || scenario.Text.ToLower().Contains("attack") || 
                        scenario.Text.ToLower().Contains("firing") || scenario.Text.ToLower().Contains("assault")) {
                        
                        AddQuestionButton(isEn ? "SITUATION" : "STAV NA MÍSTĚ", new[] { isEn ? "Status report? Injuries?" : "Jaká je situace? Jsou tam zranění?" }, scenario.DetailsAnswer, callerName);
                        AddQuestionButton(isEn ? "BACKUP" : "POSÍLÁM POSILY", new[] { isEn ? "Units dispatched." : "Posílám posily." }, isEn ? "Copy that." : "Rozumím.", callerName);
                    } 
                    else if (scenario.Text.ToLower().Contains("vozidlo") || scenario.Text.ToLower().Contains("car") || 
                        scenario.Text.ToLower().Contains("spz") || scenario.Text.ToLower().Contains("plate") || 
                        scenario.Text.ToLower().Contains("vehicle") || scenario.Text.ToLower().Contains("pursuit")) 
                    {
                        AddQuestionButton(isEn ? "PLATE" : "ŽÁDAT SPZ", new[] { isEn ? "Send the plate number." : "Udejte SPZ vozidla." }, scenario.DetailsAnswer, callerName);
                        AddQuestionButton(isEn ? "BACKUP" : "POSÍLÁM POSILY", new[] { isEn ? "Backup is on the way." : "Posílám k vám další hlídky." }, isEn ? "Copy." : "Rozumím.", callerName);

                        var b1 = CreateQuestionButton(isEn ? "STOLEN" : "ODCIZENO", 
                            new[] { isEn ? "Vehicle is reported as stolen!" : "Vozidlo je nahlášeno jako kradené!" }, 
                            isEn ? "Copy, we are engaging!" : "Rozumím, zahajujeme zákrok!", callerName);
                            
                        var b2 = CreateQuestionButton(isEn ? "NOT STOLEN" : "NEODCIZENO", 
                            new[] { isEn ? "Vehicle is not stolen." : "Vozidlo není hlášeno jako kradené." }, 
                            isEn ? "Copy, continuing patrol." : "Rozumím, pokračujeme v hlídce.", callerName);

                        b1.Click += (x, y) => { b1.Enabled = b2.Enabled = false; b2.BackColor = Color.Gray; };
                        b2.Click += (x, y) => { b2.Enabled = b1.Enabled = false; b1.BackColor = Color.Gray; };

                        _questionFlowPanel.Controls.Add(b1);
                        _questionFlowPanel.Controls.Add(b2);
                    }
                    else {
                        AddQuestionButton(isEn ? "DATA" : "ŽÁDAT ÚDAJE", new[] { isEn ? "Send ID details." : "Nadiktujte údaje." }, scenario.DetailsAnswer, callerName);
                        
                        var b1 = CreateQuestionButton(isEn ? "WANTED" : "JE HLEDANÝ", new[] { isEn ? "Person is wanted!" : "Osoba je v pátrání!" }, isEn ? "Requesting backup." : "Žádáme posilu.", callerName);
                        var b2 = CreateQuestionButton(isEn ? "CLEAN" : "NENÍ HLEDANÝ", new[] { isEn ? "Person is clean." : "Osoba je čistá." }, isEn ? "Copy, finishing." : "Díky, končíme.", callerName);
                        
                        b1.Click += (x, y) => { b1.Enabled = b2.Enabled = false; b2.BackColor = Color.Gray; };
                        b2.Click += (x, y) => { b1.Enabled = b2.Enabled = false; b1.BackColor = Color.Gray; };
                        
                        _questionFlowPanel.Controls.Add(b1);
                        _questionFlowPanel.Controls.Add(b2);
                    }
                } else {
                    AddQuestionButton(isEn ? "Name" : "Jméno", new[] { isEn ? "Who am I speaking with?" : "S kým mluvím?" }, scenario.NameAnswer, callerName);
                    AddQuestionButton(isEn ? "Location" : "Lokalita", new[] { isEn ? "Where are you?" : "Kde přesně jste?" }, scenario.LocationAnswer, callerName);
                    AddQuestionButton(isEn ? "Injuries" : "Zranění", new[] { isEn ? "Are there any injuries?" : "Jsou tam zranění?" }, scenario.InjuryAnswer, callerName);
                    AddQuestionButton(isEn ? "Details" : "Detaily", new[] { isEn ? "What is happening?" : "Co se tam děje?" }, scenario.DetailsAnswer, callerName);
                }
            };
            _callsFlowPanel.Controls.Add(card); _callsFlowPanel.Controls.SetChildIndex(card, 0);
        }

        private void AddQuestionButton(string text, string[] speech, string ans, string caller) => _questionFlowPanel.Controls.Add(CreateQuestionButton(text, speech, ans, caller));
        private Button CreateQuestionButton(string text, string[] speech, string ans, string caller) {
            bool isEn = GameSettings.CurrentLanguage == "EN";
            var b = new Button { Text = text, Width = 110, Height = 35, BackColor = Color.FromArgb(210, 210, 210), FlatStyle = FlatStyle.Flat };
            b.Click += (s, e) => {
                Random rnd = new Random(); _txtChatDisplay.AppendText($"[{(isEn ? "DISPATCH" : "DISPEČINK")}]: {speech[rnd.Next(speech.Length)]}\r\n[{caller.ToUpper()}]: {ans}\r\n\r\n");
                b.Enabled = false; b.BackColor = Color.LightGray;
            };
            return b;
        }

        private void HandleConfirmation() {
            if (_currentActiveScenario == null) return;
            bool isEn = GameSettings.CurrentLanguage == "EN";
            var sel = new List<string>();
            foreach (Control c in _unitSelectionPanel.Controls) if (c is Panel p && p.Tag != null && (bool)p.Tag) sel.Add((string)p.AccessibleName);
            _currentActiveScenario.UserSentUnits = new List<string>(sel);
            _currentActiveScenario.WasCorrect = EvaluateResponse(_currentActiveScenario, sel);
            if (_currentActiveScenario.WasCorrect) _score += 10; else _score -= 5;
            lblScore.Text = (isEn ? "Score: " : "Skóre: ") + _score;
            lstHistory.Items.Insert(0, _currentActiveScenario);
            _callsFlowPanel.Controls.Remove(_currentActiveCard);
            _activeCallPanel.Visible = false; lblNoCall.Visible = true; _currentActiveScenario = null;
            foreach (Control c in _unitSelectionPanel.Controls) if (c is Panel p) { p.Tag = false; p.BackColor = Color.FromArgb(210, 210, 210); }
        }

        private bool EvaluateResponse(Scenario s, List<string> sel) {
            if (s.RequiredUnits.Count == 0 && sel.Count == 0) return true;
            var req = new HashSet<string>(s.RequiredUnits ?? new());
            var sent = new HashSet<string>(sel);
            return req.SetEquals(sent);
        }

        private void LoadScenarios() {
            try {
                string fileName = GameSettings.CurrentLanguage == "EN" ? "scenarios_en.json" : "scenarios.json";
                string jsonPath = Path.Combine(AppContext.BaseDirectory, fileName);
                if (File.Exists(jsonPath)) {
                    _scenarios = JsonSerializer.Deserialize<List<Scenario>>(File.ReadAllText(jsonPath), new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<Scenario>();
                }
            } catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }
    }

    public class Scenario {
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
        public override string ToString() {
            bool isEn = GameSettings.CurrentLanguage == "EN";
            string res = WasCorrect ? (isEn ? "[CORRECT]" : "[Správně]") : (isEn ? "[WRONG]" : "[Chybně]");
            return $"{DateTime.Now:HH:mm} {res} - {EventName}";
        }
    }

    public class Person { public string FirstName { get; set; } = ""; public string LastName { get; set; } = ""; public string BirthDate { get; set; } = ""; public bool IsWanted { get; set; } public string Note { get; set; } = ""; }
    public class Car { public string SPZ { get; set; } = ""; public string Model { get; set; } = ""; public string Owner { get; set; } = ""; public bool IsStolen { get; set; } public string Note { get; set; } = ""; }
}