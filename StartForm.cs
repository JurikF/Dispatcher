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
            this.BackColor = Color.FromArgb(30, 30, 30); // tmavé pozadí
            this.StartPosition = FormStartPosition.CenterScreen;

            // Hlavička
            var head = new Label
            {
                Text = "Dispatcher Simulator",
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 120,
                Font = new Font("Segoe UI", 36F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(50, 50, 50)
            };
            this.Controls.Add(head);

            // Panel pro volbu obtížnosti
            var panel = new Panel
            {
                Width = 400,
                Height = 200,
                BackColor = Color.FromArgb(50, 50, 50),
                Location = new Point((this.ClientSize.Width - 400) / 2, 200)
            };
            this.Controls.Add(panel);
            panel.Anchor = AnchorStyles.Top;

            // Label obtížnosti
            var lbl = new Label
            {
                Text = "Vyber obtížnost:",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 50
            };
            panel.Controls.Add(lbl);

            // ComboBox
            var cbDifficulty = new ComboBox
            {
                Width = 200,
                Font = new Font("Segoe UI", 16F),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point((panel.Width - 200) / 2, 70)
            };
            cbDifficulty.Items.AddRange(new[] { "All", "Easy", "Hard" });
            cbDifficulty.SelectedIndex = 0;
            panel.Controls.Add(cbDifficulty);

            // Tlačítka
            var btnPlay = new Button
            {
                Text = "Hrát",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                Width = 120,
                Height = 50,
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point((panel.Width / 2) - 130, 130)
            };
            btnPlay.FlatAppearance.BorderSize = 0;
            btnPlay.Click += (s, e) =>
            {
                SelectedDifficulty = cbDifficulty.Text;
                this.Hide();
                var mainForm = new MainForm(SelectedDifficulty);
                mainForm.ShowDialog();
                this.Close();
            };
            panel.Controls.Add(btnPlay);

            var btnExit = new Button
            {
                Text = "Odejít",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                Width = 120,
                Height = 50,
                BackColor = Color.FromArgb(200, 50, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point((panel.Width / 2) + 10, 130)
            };
            btnExit.FlatAppearance.BorderSize = 0;
            btnExit.Click += (s, e) => this.Close();
            panel.Controls.Add(btnExit);
        }
    }
}
