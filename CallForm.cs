using System;

using System.Collections.Generic;

using System.Drawing;

using System.Linq;

using System.Windows.Forms;

using static DispatcherSimulator.MainForm;



namespace DispatcherSimulator

{

    public class CallForm : Form

    {

        private Scenario _scenario;

        private TextBox _txtDisplay;

        private FlowLayoutPanel _chatPanel;

        private FlowLayoutPanel _unitSelectionPanel;

       

        // Seznam vybraných jednotek, který si pak přečte MainForm pro body

        public List<string> SelectedUnits { get; private set; } = new();



        public CallForm(Scenario scenario)

        {

            _scenario = scenario;

            InitializeUI();



            this.StartPosition = FormStartPosition.Manual;

            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;

            this.TopMost = true;



            // Pozice v pravém horním kvadrantu

            int x = Screen.PrimaryScreen.WorkingArea.Width - this.Width - 50;

            int y = 70;

            this.Location = new Point(x, y);

        }



        private void InitializeUI()

        {

            this.Text = "☎️ Aktivní hovor a nasazení sil";

            this.Width = 600;

            this.Height = 700; // Zvětšeno, aby se vešel výběr jednotek

            this.BackColor = Color.FromArgb(30, 30, 30);



            // 1. DISPLEJ HOVORU

            _txtDisplay = new TextBox

            {

                Multiline = true,

                ReadOnly = true,

                Left = 15,

                Top = 15,

                Width = 555,

                Height = 150,

                Text = $"[DISPEČINK]: Linka 112, poslouchám.\r\n[VOLAJÍCÍ]: {_scenario.Text}\r\n",

                Font = new Font("Consolas", 10F),

                BackColor = Color.Black,

                ForeColor = Color.Lime,

                BorderStyle = BorderStyle.None

            };

            this.Controls.Add(_txtDisplay);



            // 2. OTÁZKY OPERÁTORA

            var lblQuestions = new Label { Text = "DOTAZY OPERÁTORA:", ForeColor = Color.Gray, Left = 15, Top = 180, AutoSize = true };

            this.Controls.Add(lblQuestions);



            _chatPanel = new FlowLayoutPanel { Left = 15, Top = 200, Width = 570, Height = 120, BackColor = Color.Transparent };

            this.Controls.Add(_chatPanel);



            AddChoice("Jméno", _scenario.NameAnswer);

            AddChoice("Lokalita", _scenario.LocationAnswer);

            AddChoice("Zranění", _scenario.InjuryAnswer);

            AddChoice("Detaily", _scenario.DetailsAnswer);



            // 3. VÝBĚR JEDNOTEK

            var lblUnits = new Label { Text = "VÝBĚR JEDNOTEK K VYSLÁNÍ:", ForeColor = Color.Gold, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Left = 15, Top = 330, AutoSize = true };

            this.Controls.Add(lblUnits);



            _unitSelectionPanel = new FlowLayoutPanel { Left = 15, Top = 355, Width = 570, Height = 220, BackColor = Color.FromArgb(40, 40, 40), Padding = new Padding(10) };

            this.Controls.Add(_unitSelectionPanel);



            // Přidání přepínacích tlačítek (String tag musí odpovídat RequiredUnits v JSONu, např. "Police" nebo "Fire")

            AddUnitToggle("Pořádková policie", Color.FromArgb(30, 30, 200), "Police");

            AddUnitToggle("Dopravní policie", Color.FromArgb(30, 30, 200), "Police");

            AddUnitToggle("Policie - K9", Color.FromArgb(30, 30, 200), "Police");

            AddUnitToggle("Hasiči", Color.FromArgb(200, 30, 30), "Fire");

            AddUnitToggle("Hasiči - SAR", Color.FromArgb(200, 30, 30), "Fire");

            AddUnitToggle("Záchranná služba", Color.FromArgb(210, 180, 0), "Ambulance");



            // 4. TLAČÍTKO ODESLAT

            var btnEnd = new Button

            {

                Text = "UKONČIT A VYSLAT VYBRANÉ SÍLY",

                Left = 15,

                Top = 600,

                Width = 555,

                Height = 50,

                Font = new Font("Segoe UI", 12F, FontStyle.Bold),

                BackColor = Color.DarkGreen,

                ForeColor = Color.White,

                FlatStyle = FlatStyle.Flat,

                Cursor = Cursors.Hand

            };

            btnEnd.FlatAppearance.BorderSize = 0;

            btnEnd.Click += (s, e) => 
{
    btnEnd.Enabled = false; // Prevence dvojkliku
    this.DialogResult = DialogResult.OK;
    
    // Použijeme metodu Close() bez jakýchkoliv dalších efektů
    this.Close();
};

            this.Controls.Add(btnEnd);

        }



        private void AddChoice(string question, string answer)

        {

            var btn = new Button { Text = question, Width = 130, Height = 40, BackColor = Color.FromArgb(60, 60, 60), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Margin = new Padding(3) };

            btn.Click += (s, e) => {

                AppendText("OPERÁTOR", question + "?");

                AppendText("VOLAJÍCÍ", answer);

                btn.Enabled = false;

                btn.BackColor = Color.FromArgb(45, 45, 45);

            };

            _chatPanel.Controls.Add(btn);

        }



        private void AddUnitToggle(string text, Color baseColor, string unitType)

        {

            var btn = new Button

            {

                Text = text,

                Width = 170,

                Height = 50,

                BackColor = Color.FromArgb(60, 60, 60), // Výchozí šedá

                ForeColor = Color.White,

                FlatStyle = FlatStyle.Flat,

                Font = new Font("Segoe UI", 9F, FontStyle.Bold),

                Tag = false // Stav vybráno/nevybráno

            };

            btn.FlatAppearance.BorderColor = baseColor;

            btn.FlatAppearance.BorderSize = 2;



            btn.Click += (s, e) =>

            {

                bool isSelected = !(bool)btn.Tag;

                btn.Tag = isSelected;



                if (isSelected)

                {

                    btn.BackColor = baseColor;

                    SelectedUnits.Add(unitType);

                }

                else

                {

                    btn.BackColor = Color.FromArgb(60, 60, 60);

                    SelectedUnits.Remove(unitType);

                }

            };

            _unitSelectionPanel.Controls.Add(btn);

        }



        private void AppendText(string sender, string message)

        {

            _txtDisplay.AppendText(Environment.NewLine + $"[{sender.ToUpper()}]: {message}");

            _txtDisplay.SelectionStart = _txtDisplay.Text.Length;

            _txtDisplay.ScrollToCaret();

        }

    }

}