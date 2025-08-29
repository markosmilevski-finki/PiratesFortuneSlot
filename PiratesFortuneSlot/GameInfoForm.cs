using System;
using System.Drawing;
using System.Windows.Forms;

namespace PiratesFortuneSlot
{
    public partial class GameInfoForm : Form
    {
        private readonly SymbolManager _symbolManager;

        public GameInfoForm()
        {
            _symbolManager = new SymbolManager();
            _symbolManager.InitializeSymbols();
            InitializeRulesInfoUI();
        }

        private void InitializeRulesInfoUI()
        {
            this.Text = "Pirates Fortune Slot - Rules & Info";
            this.Size = new Size(650, 600);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.DoubleBuffered = true;

            var bgImage = SymbolManager.GetEmbeddedImage("Background.jpg");
            if (bgImage != null)
            {
                this.BackgroundImage = bgImage;
                this.BackgroundImageLayout = ImageLayout.Stretch;
            }
            else
            {
                this.BackColor = Color.FromArgb(0, 105, 148);
            }

            Panel contentPanel = new Panel
            {
                Location = new Point(20, 20),
                Size = new Size(610, 560),
                BackColor = Color.FromArgb(200, 245, 245, 220),
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(contentPanel);

            Label lblTitle = new Label
            {
                Text = "Pirates Fortune Slot Rules & Info",
                Font = new Font("Papyrus", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(184, 134, 11),
                Location = new Point(10, 10),
                Size = new Size(590, 40),
                TextAlign = ContentAlignment.MiddleCenter
            };
            contentPanel.Controls.Add(lblTitle);

            Panel symbolsPanel = new Panel
            {
                Location = new Point(10, 60),
                Size = new Size(590, 180),
                BackColor = Color.Transparent
            };
            contentPanel.Controls.Add(symbolsPanel);
            CreateSpecialSymbolsSection(symbolsPanel);

            Panel divider = new Panel
            {
                Location = new Point(10, 250),
                Size = new Size(590, 2),
                BackColor = Color.FromArgb(184, 134, 11)
            };
            contentPanel.Controls.Add(divider);

            Panel mechanicsPanel = new Panel
            {
                Location = new Point(10, 260),
                Size = new Size(590, 240),
                BackColor = Color.Transparent
            };
            contentPanel.Controls.Add(mechanicsPanel);
            CreateGameMechanicsSection(mechanicsPanel);

            Button btnClose = new Button
            {
                Text = "Close",
                Font = new Font("Papyrus", 12, FontStyle.Bold),
                Size = new Size(100, 30),
                Location = new Point(490, 500),
                BackColor = Color.FromArgb(184, 134, 11),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderColor = Color.Gold, BorderSize = 2 }
            };
            btnClose.Click += (s, e) => this.Close();
            contentPanel.Controls.Add(btnClose);

            btnClose.MouseEnter += (s, e) => btnClose.BackColor = Color.FromArgb(218, 165, 32);
            btnClose.MouseLeave += (s, e) => btnClose.BackColor = Color.FromArgb(184, 134, 11);
        }

        private void CreateSpecialSymbolsSection(Panel panel)
        {
            Label lblSpecialHeader = new Label
            {
                Text = "Special Symbols",
                Font = new Font("Papyrus", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(184, 134, 11),
                Location = new Point(0, 0),
                Size = new Size(590, 30),
                TextAlign = ContentAlignment.MiddleLeft
            };
            panel.Controls.Add(lblSpecialHeader);

            string[] specialDescriptions = new[]
            {
                "Wild: Substitutes for any symbol except Scatter and Gold Coin in winning clusters.",
                "Scatter: 3 or more trigger the Treasure Hunt Bonus: 3 Scatters = 10 Bonus Spins, +5 spins per additional Scatter.",
                "Gold Coin: In Bonus mode, collect Gold Coins to reach requirement (starts at 5, doubles up to 10): grants +2 Bonus Spins and +1 Multiplier (up to 10 extra triggers)."
            };
            string[] symbolImages = new[] { "Wild.png", "Scatter.png", "GoldCoin.png" };

            for (int i = 0; i < specialDescriptions.Length; i++)
            {
                int y = 30 + i * 50;

                PictureBox pbSymbol = new PictureBox
                {
                    Image = SymbolManager.GetEmbeddedImage(symbolImages[i]),
                    Size = new Size(40, 40),
                    Location = new Point(0, y),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    BackColor = Color.Transparent
                };
                if (pbSymbol.Image == null)
                {
                    pbSymbol.BackColor = Color.Gray;
                }
                panel.Controls.Add(pbSymbol);

                Label lblDesc = new Label
                {
                    Text = specialDescriptions[i],
                    Font = new Font("Arial", 10),
                    ForeColor = Color.Black,
                    Location = new Point(50, y + 5),
                    Size = new Size(540, 40),
                    TextAlign = ContentAlignment.MiddleLeft,
                    AutoSize = false
                };
                panel.Controls.Add(lblDesc);
            }
        }

        private void CreateGameMechanicsSection(Panel panel)
        {
            Label lblMechanicsHeader = new Label
            {
                Text = "Game Mechanics",
                Font = new Font("Papyrus", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(184, 134, 11),
                Location = new Point(0, 0),
                Size = new Size(590, 30),
                TextAlign = ContentAlignment.MiddleLeft
            };
            panel.Controls.Add(lblMechanicsHeader);

            string[] mechanics = new[]
            {
                "Cluster Wins: Minimum of 4 identical symbols (or Wilds) connected horizontally or vertically.",
                "Cascading Reels: Winning clusters explode, new symbols drop in, allowing more wins.",
                "Bonus Mode: Triggered by 3+ Scatters, includes more Wilds and Gold Coin collection.",
                "Bonus Spins: Each spin consumes one Bonus Spin, ends when no spins remain.",
                "Payout: Symbol Payout × Bet × Bonus Multiplier (in Bonus mode)."
            };

            for (int i = 0; i < mechanics.Length; i++)
            {
                Label lblMechanic = new Label
                {
                    Text = mechanics[i],
                    Font = new Font("Arial", 10),
                    ForeColor = Color.Black,
                    Location = new Point(0, 30 + i * 25),
                    Size = new Size(590, 25),
                    TextAlign = ContentAlignment.MiddleLeft
                };
                panel.Controls.Add(lblMechanic);
            }
        }
    }
}