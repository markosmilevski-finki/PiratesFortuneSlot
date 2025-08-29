using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace PiratesFortuneSlot
{
    public class UIManager
    {
        public Label lblBonusSpins;
        public Label lblMultiplier;
        public Label lblGoldCoins;
        public Label lblBiggestWin;
        public Label lblBiggestMultiplier;
        public Button btnAutoSpin;
        public Button btnGameInfo;
        public Timer tmrWinDisplay;
        public Timer tmrBaseWinDisplay;
        public Timer tmrAutoSpin;
        public Timer tmrExplode;
        public Timer tmrCascadeDrop;
        public Label lblWin;
        private readonly Form1 _form;

        public UIManager(Form1 form)
        {
            _form = form;
            lblWin = _form.lblWin;
        }

        public void InitializeUI()
        {
            lblWin.Location = new Point(450, 600);
            lblWin.Font = new Font("Arial", 24, FontStyle.Bold);
            lblWin.ForeColor = Color.FromArgb(255, 215, 0);
            lblWin.BackColor = Color.Black;
            lblWin.Size = new Size(200, 40);
            lblWin.TextAlign = ContentAlignment.MiddleCenter;
            lblWin.Visible = false;

            lblBonusSpins = new Label
            {
                Location = new Point(25, 570),
                Font = new Font("Arial", 12, FontStyle.Bold),
                Size = new Size(150, 25),
                Text = "Bonus Spins: 0",
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Visible = false
            };
            _form.Controls.Add(lblBonusSpins);

            lblMultiplier = new Label
            {
                Location = new Point(25, 540),
                Font = new Font("Arial", 12, FontStyle.Bold),
                Size = new Size(150, 25),
                Text = "Multiplier: 1x",
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Visible = false
            };
            _form.Controls.Add(lblMultiplier);

            lblGoldCoins = new Label
            {
                Location = new Point(25, 600),
                Font = new Font("Arial", 12, FontStyle.Bold),
                Size = new Size(150, 25),
                Text = "Gold Coins: 0/3",
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Visible = false
            };
            _form.Controls.Add(lblGoldCoins);

            lblBiggestWin = new Label
            {
                Location = new Point(900, 250),
                Font = new Font("Arial", 12, FontStyle.Bold),
                Size = new Size(250, 25),
                Text = "Biggest Win: 0.00",
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };
            _form.Controls.Add(lblBiggestWin);

            lblBiggestMultiplier = new Label
            {
                Location = new Point(900, 200),
                Font = new Font("Arial", 12, FontStyle.Bold),
                Size = new Size(200, 25),
                Text = "Biggest Multiplier: 1x",
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };
            _form.Controls.Add(lblBiggestMultiplier);

            btnAutoSpin = new Button
            {
                Location = new Point(1100, 600),
                Font = new Font("Arial", 12),
                Size = new Size(80, 30),
                Text = "Auto Spin"
            };
            _form.Controls.Add(btnAutoSpin);

            btnGameInfo = new Button
            {
                Location = new Point(1100, 640),
                Font = new Font("Papyrus", 12, FontStyle.Bold),
                Size = new Size(90, 30),
                Text = "Rules",
                BackColor = Color.FromArgb(184, 134, 11),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderColor = Color.Gold, BorderSize = 2 }
            };
            btnGameInfo.Click += (s, e) => new GameInfoForm().ShowDialog();
            _form.Controls.Add(btnGameInfo);

            tmrWinDisplay = new Timer
            {
                Interval = 2000
            };

            tmrBaseWinDisplay = new Timer
            {
                Interval = 2000
            };

            tmrAutoSpin = new Timer
            {
                Interval = 1500
            };

            tmrExplode = new Timer
            {
                Interval = 500
            };

            tmrCascadeDrop = new Timer
            {
                Interval = 50
            };
        }

        public void UpdateUI(double balance, double bet, int bonusSpins, int bonusMultiplier, int collectedTreasures, int goldCoinRequirement, double biggestWin, double biggestMultiplier, bool inBonus, bool isSpinComplete, bool isAutoSpinning)
        {
            _form.lblBalance.Text = $"Balance: {balance:N2}";
            _form.nudBet.Value = (decimal)bet;
            lblBonusSpins.Text = $"Bonus Spins: {bonusSpins}";
            lblMultiplier.Text = $"Multiplier: {bonusMultiplier}x";
            lblGoldCoins.Text = $"Gold Coins: {collectedTreasures}/{goldCoinRequirement}";
            lblBiggestWin.Text = $"Biggest Win: {biggestWin:N2}";
            lblBiggestMultiplier.Text = $"Biggest Multiplier: {biggestMultiplier:F2}x";
            lblBonusSpins.Visible = inBonus;
            lblMultiplier.Visible = inBonus;
            lblGoldCoins.Visible = inBonus;
            btnAutoSpin.Enabled = !inBonus;
            _form.btnSpin.Enabled = !inBonus && isSpinComplete && !isAutoSpinning;
            UpdateBackgroundImage(inBonus);
            _form.Invalidate();
        }

        public void UpdateBackgroundImage(bool inBonus)
        {
            string imageName = inBonus ? "Stormy.jpg" : "Background.jpg";
            var baseBackground = SymbolManager.GetEmbeddedImage(imageName);
            if (baseBackground == null)
            {
                return;
            }

            var composite = new Bitmap(_form.Width, _form.Height);
            using (Graphics g = Graphics.FromImage(composite))
            {
                g.DrawImage(baseBackground, new Rectangle(0, 0, _form.Width, _form.Height), new Rectangle(0, 0, baseBackground.Width, baseBackground.Height), GraphicsUnit.Pixel);

                using (Brush brush = new SolidBrush(Color.FromArgb(128, 64, 64, 64)))
                {
                    int startX = 200, startY = 50, size = 120, spacing = 130;
                    for (int row = 0; row < GridManager.ROWS; row++)
                    {
                        for (int col = 0; col < GridManager.COLS; col++)
                        {
                            int x = startX + col * spacing;
                            int y = startY + row * spacing;
                            g.FillRectangle(brush, x, y, size, size);
                        }
                    }
                }
            }

            if (_form.BackgroundImage != null)
            {
                _form.BackgroundImage.Dispose();
            }
            _form.BackgroundImage = composite;
            baseBackground.Dispose();

            _form.Invalidate();
        }
    }
}