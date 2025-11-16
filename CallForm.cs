using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace DispatcherSimulator
{
    public class CallForm : Form
    {
        private Scenario _scenario;
        public List<string> SelectedUnits { get; private set; } = new();

        public CallForm(Scenario scenario)
        {
            _scenario = scenario;
            InitializeUI();
            // Nastavení pozice nad hlavním oknem
            this.StartPosition = FormStartPosition.CenterParent; // vycentruje vůči MainForm
            this.TopMost = true;                               // zobrazí nad fullscreen oknem

        }

        private void InitializeUI()
        {
            this.Text = _scenario.Title;
            this.Width = 600;
            this.Height = 400;

            var lblText = new TextBox { Multiline = true, ReadOnly = true, Left = 20, Top = 20, Width = 540, Height = 120, Text = _scenario.Text };
            this.Controls.Add(lblText);

            var clb = new CheckedListBox { Left = 20, Top = 150, Width = 540, Height = 120 };
            clb.Items.AddRange(new object[] { "Police", "Fire", "Ambulance", "Rescue", "Hazmat" });
            this.Controls.Add(clb);

            var btnSend = new Button { Text = "Odeslat", Left = 380, Top = 310, Width = 80 };
            btnSend.Click += (s, e) => { SelectedUnits = clb.CheckedItems.Cast<string>().ToList(); this.DialogResult = DialogResult.OK; this.Close(); };
            this.Controls.Add(btnSend);

            var btnCancel = new Button { Text = "Zrušit", Left = 480, Top = 310, Width = 80 };
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            this.Controls.Add(btnCancel);
        }
    }
}
