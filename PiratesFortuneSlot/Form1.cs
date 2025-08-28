using System;
using System.Collections.Generic;
using System.Drawing;
using System.Media;
using System.Reflection;
using System.Windows.Forms;
using System.IO;
using NAudio.Wave;

namespace PiratesFortuneSlot
{
    public partial class Form1 : Form
    {
        private enum SymbolType
        {
            Ruby, Sapphire, Emerald, RumBottle, Compass, Map, Parrot, PirateHat, Ship, Wild, Scatter, GoldCoin, Empty
        }

        private class Symbol
        {
            public SymbolType Type { get; set; }
            public Image Image { get; set; }
            public double[] Payouts { get; set; }

            public Symbol(SymbolType type, string imageName, double[] payouts)
            {
                Type = type;
                Image = GetEmbeddedImage(imageName);
                Payouts = payouts;
            }
        }

        private List<Symbol> symbols = new List<Symbol>();
        private const int COLS = 5;
        private const int ROWS = 4;
        private SymbolType[,] grid = new SymbolType[ROWS, COLS];
        private PictureBox[,] pbGrid = new PictureBox[ROWS, COLS];
        private Point[,] gridCenters;
        private double balance = 1000.00;
        private double currentWin = 0.00;
        private double totalSpinWin = 0.00;
        private double bet = 10.00;
        private Random rnd = new Random();
        private bool inBonus = false;
        private int bonusSpins = 0;
        private int bonusMultiplier = 1;
        private double biggestMultiplier = 0.0;
        private double biggestWin = 0.00;
        private int collectedTreasures = 0;
        private int extraSpinsAdded = 0;
        private int goldCoinRequirement = 3;
        private bool isAutoSpinning = false;
        private bool isSpinComplete = true;
        private bool bonusTriggeredThisSpin = false;
        private IWavePlayer sndBackgroundPlayer;
        private WaveFileReader sndBackgroundReader;
        private List<IWavePlayer> soundPlayers = new List<IWavePlayer>();
        private List<WaveFileReader> soundReaders = new List<WaveFileReader>();
        private int[,] finalYPositions = new int[ROWS, COLS];
        private int spinCount = 0;
        private int spinsWon = 0;
        private double winPercentage = 0;
        private Label lblBonusSpins;
        private Label lblMultiplier;
        private Label lblGoldCoins;
        private Label lblBiggestWin;
        private Label lblBiggestMultiplier;
        private Label lblTotalSpins;
        private Label lblWinPercentage;
        private Button btnAutoSpin;
        private Timer tmrWinDisplay;
        private Timer tmrBaseWinDisplay;
        private Timer tmrAutoSpin;
        private Timer tmrExplode;
        private Timer tmrCascadeDrop;
        private Point lblWinInitialPos;
        private double baseWin;
        private bool closingForm = false;
        private List<Point> currentExplodingCluster;
        private bool isExploding = false;
        private bool isCascading = false;
        private int goldCoins;

        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true;

            lblWin.Location = new Point(450, 600);
            lblWin.Font = new Font("Arial", 24, FontStyle.Bold);
            lblWin.ForeColor = Color.FromArgb(255, 215, 0);
            lblWin.BackColor = Color.Black;
            lblWin.Size = new Size(200, 40);
            lblWin.TextAlign = ContentAlignment.MiddleCenter;
            lblWin.Visible = false;
            lblWinInitialPos = lblWin.Location;

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
            Controls.Add(lblBonusSpins);

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
            Controls.Add(lblMultiplier);

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
            Controls.Add(lblGoldCoins);

            lblBiggestWin = new Label
            {
                Location = new Point(900, 250),
                Font = new Font("Arial", 12, FontStyle.Bold),
                Size = new Size(150, 25),
                Text = "Biggest Win: 0.00",
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };
            Controls.Add(lblBiggestWin);

            lblBiggestMultiplier = new Label
            {
                Location = new Point(900, 200),
                Font = new Font("Arial", 12, FontStyle.Bold),
                Size = new Size(200, 25),
                Text = "Biggest Multiplier: 1x",
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };
            Controls.Add(lblBiggestMultiplier);

            lblTotalSpins = new Label
            {
                Location = new Point(900, 300),
                Font = new Font("Arial", 12, FontStyle.Bold),
                Size = new Size(200, 25),
                Text = "Total Spins: 0",
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };
            Controls.Add(lblTotalSpins);

            lblWinPercentage = new Label
            {
                Location = new Point(900, 350),
                Font = new Font("Arial", 12, FontStyle.Bold),
                Size = new Size(200, 25),
                Text = "Win Percentage: 0",
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };
            Controls.Add(lblWinPercentage);

            btnAutoSpin = new Button
            {
                Location = new Point(1100, 600),
                Font = new Font("Arial", 12),
                Size = new Size(80, 30),
                Text = "Auto Spin"
            };
            btnAutoSpin.Click += btnAutoSpin_Click;
            Controls.Add(btnAutoSpin);

            tmrWinDisplay = new Timer
            {
                Interval = 2000
            };
            tmrWinDisplay.Tick += tmrWinDisplay_Tick;

            tmrBaseWinDisplay = new Timer
            {
                Interval = 2000
            };
            tmrBaseWinDisplay.Tick += tmrBaseWinDisplay_Tick;

            tmrAutoSpin = new Timer
            {
                Interval = 1500
            };
            tmrAutoSpin.Tick += tmrAutoSpin_Tick;

            tmrExplode = new Timer
            {
                Interval = 500
            };
            tmrExplode.Tick += tmrExplode_Tick;

            tmrCascadeDrop = new Timer
            {
                Interval = 50
            };
            tmrCascadeDrop.Tick += tmrCascadeDrop_Tick;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Size = new Size(1280, 720);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackgroundImageLayout = ImageLayout.Stretch;

            UpdateBackgroundImage();

            lblBalance.Location = new Point(25, 630);
            lblBalance.Font = new Font("Arial", 12, FontStyle.Bold);
            lblBalance.ForeColor = Color.White;
            lblBalance.BackColor = Color.Transparent;
            nudBet.Location = new Point(850, 600);
            nudBet.Font = new Font("Arial", 12);
            nudBet.Size = new Size(80, 25);
            nudBet.DecimalPlaces = 2;
            nudBet.Minimum = 0.10m;
            nudBet.Maximum = 100.00m;
            nudBet.Value = 10.00m;
            nudBet.Increment = 0.10m;
            btnSpin.Location = new Point(1000, 600);
            btnSpin.Font = new Font("Arial", 12);
            btnSpin.Size = new Size(80, 30);

            tmrDrop.Interval = 50;
            tmrCascade.Interval = 500;

            LoadSounds();
            InitializeSymbols();
            if (symbols.Count == 0)
            {
                MessageBox.Show("No symbols loaded. Check resource files.");
                Close();
                return;
            }

            gridCenters = GetGridCenterCoordinates();
            InitializeGrid();
            GenerateGrid();
            UpdateGridDisplay();
            UpdateUI();
            PlayBackgroundMusic("BackgroundMusic.wav");
        }

        private Point[,] GetGridCenterCoordinates()
        {
            Point[,] centers = new Point[ROWS, COLS];
            int startX = 200, startY = 50, spacing = 130, size = 100;

            for (int row = 0; row < ROWS; row++)
            {
                for (int col = 0; col < COLS; col++)
                {
                    int centerX = startX + col * spacing + size / 2;
                    int centerY = startY + row * spacing + size / 2;
                    centers[row, col] = new Point(centerX, centerY);
                }
                if (row > 0)
                {
                    startY -= 10;
                }
                if (row == 2) startX += 5;
            }
            return centers;
        }

        private void InitializeSymbols()
        {
            var symbolData = new[]
            {
                new { Type = SymbolType.Ruby, ImageName = "Ruby.png", Payouts = new double[] { 22.40, 44.80, 89.60, 179.20, 358.40, 716.80, 1433.60, 2867.20, 5734.40, 11468.80, 22937.60, 45875.20, 91750.40, 183500.80, 367001.60, 734003.20, 1468006.40 } },
                new { Type = SymbolType.Sapphire, ImageName = "Sapphire.png", Payouts = new double[] { 11.20, 22.40, 44.80, 89.60, 179.20, 358.40, 716.80, 1433.60, 2867.20, 5734.40, 11468.80, 22937.60, 45875.20, 91750.40, 183500.80, 367001.60, 734003.20 } },
                new { Type = SymbolType.Emerald, ImageName = "Emerald.png", Payouts = new double[] { 5.60, 11.20, 22.40, 44.80, 89.60, 179.20, 358.40, 716.80, 1433.60, 2867.20, 5734.40, 11468.80, 22937.60, 45875.20, 91750.40, 183500.80, 367001.60 } },
                new { Type = SymbolType.RumBottle, ImageName = "RumBottle.png", Payouts = new double[] { 0.05, 0.10, 0.20, 0.40, 0.80, 1.60, 3.20, 6.40, 12.80, 25.60, 51.20, 102.40, 204.80, 409.60, 819.20, 1638.40, 3276.80 } },
                new { Type = SymbolType.Compass, ImageName = "Compass.png", Payouts = new double[] { 1.20, 2.40, 4.80, 9.60, 19.20, 38.40, 76.80, 153.60, 307.20, 614.40, 1228.80, 2457.60, 4915.20, 9830.40, 19660.80, 39321.60, 78643.20 } },
                new { Type = SymbolType.Map, ImageName = "Map.png", Payouts = new double[] { 2.80, 5.60, 11.20, 22.40, 44.80, 89.60, 179.20, 358.40, 716.80, 1433.60, 2867.20, 5734.40, 11468.80, 22937.60, 45875.20, 91750.40, 183500.80 } },
                new { Type = SymbolType.Parrot, ImageName = "Parrot.png", Payouts = new double[] { 0.30, 0.60, 1.20, 2.40, 4.80, 9.60, 19.20, 38.40, 76.80, 153.60, 307.20, 614.40, 1228.80, 2457.60, 4915.20, 9830.40, 19660.80 } },
                new { Type = SymbolType.PirateHat, ImageName = "PirateHat.png", Payouts = new double[] { 0.15, 0.30, 0.60, 1.20, 2.40, 4.80, 9.60, 19.20, 38.40, 76.80, 153.60, 307.20, 614.40, 1228.80, 2457.60, 4915.20, 9830.40 } },
                new { Type = SymbolType.Ship, ImageName = "Ship.png", Payouts = new double[] { 0.60, 1.20, 2.40, 4.80, 9.60, 19.20, 38.40, 76.80, 153.60, 307.20, 614.40, 1228.80, 2457.60, 4915.20, 9830.40, 19660.80, 39321.60 } },
                new { Type = SymbolType.Wild, ImageName = "Wild.png", Payouts = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } },
                new { Type = SymbolType.Scatter, ImageName = "Scatter.png", Payouts = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } },
                new { Type = SymbolType.GoldCoin, ImageName = "GoldCoin.png", Payouts = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } }
            };

            foreach (var data in symbolData)
            {
                var image = GetEmbeddedImage(data.ImageName);
                if (image != null)
                {
                    symbols.Add(new Symbol(data.Type, data.ImageName, data.Payouts));
                }
                else
                {
                    MessageBox.Show($"Failed to load image: {data.ImageName}");
                }
            }
        }

        private void LoadSounds()
        {
        }

        private void PlaySound(string resourceName)
        {
            try
            {
                var stream = GetEmbeddedResourceStream(resourceName);
                if (stream == null) return;

                var reader = new WaveFileReader(stream);
                var player = new WaveOutEvent();
                player.Init(reader);
                player.Play();

                soundPlayers.Add(player);
                soundReaders.Add(reader);

                player.PlaybackStopped += (s, e) =>
                {
                    soundPlayers.Remove(player);
                    soundReaders.Remove(reader);
                    player.Dispose();
                    reader.Dispose();
                };
            }
            catch (Exception)
            {
            }
        }

        private void PlayBackgroundMusic(string resourceName)
        {
            try
            {
                if (sndBackgroundPlayer != null)
                {
                    sndBackgroundPlayer.Stop();
                    sndBackgroundPlayer.Dispose();
                    sndBackgroundReader?.Dispose();
                    sndBackgroundPlayer = null;
                    sndBackgroundReader = null;
                }

                var stream = GetEmbeddedResourceStream(resourceName);
                if (stream == null) return;

                sndBackgroundReader = new WaveFileReader(stream);
                var loopStream = new NAudio.Wave.WaveChannel32(sndBackgroundReader) { PadWithZeroes = false };
                sndBackgroundPlayer = new WaveOutEvent();
                sndBackgroundPlayer.Init(loopStream);
                sndBackgroundPlayer.PlaybackStopped += (s, e) =>
                {
                    if (sndBackgroundReader != null && !closingForm)
                    {
                        sndBackgroundReader.Position = 0;
                        sndBackgroundPlayer.Play();
                    }
                };
                sndBackgroundPlayer.Play();
            }
            catch (Exception)
            {
            }
        }

        private void UpdateBackgroundImage()
        {
            string imageName = inBonus ? "Stormy.jpg" : "Background.jpg";
            var baseBackground = GetEmbeddedImage(imageName);
            if (baseBackground == null)
            {
                return;
            }

            var composite = new Bitmap(this.Width, this.Height);
            using (Graphics g = Graphics.FromImage(composite))
            {
                g.DrawImage(baseBackground, new Rectangle(0, 0, this.Width, this.Height), new Rectangle(0, 0, baseBackground.Width, baseBackground.Height), GraphicsUnit.Pixel);

                using (Brush brush = new SolidBrush(Color.FromArgb(128, 64, 64, 64)))
                {
                    int startX = 200, startY = 50, size = 120, spacing = 130;
                    for (int row = 0; row < ROWS; row++)
                    {
                        for (int col = 0; col < COLS; col++)
                        {
                            int x = startX + col * spacing;
                            int y = startY + row * spacing;
                            g.FillRectangle(brush, x, y, size, size);
                        }
                    }
                }
            }

            if (this.BackgroundImage != null)
            {
                this.BackgroundImage.Dispose();
            }
            this.BackgroundImage = composite;
            baseBackground.Dispose();

            this.Invalidate();
        }

        private static Stream GetEmbeddedResourceStream(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = "PiratesFortuneSlot.Resources." + name;
            var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                MessageBox.Show($"Sound resource not found: {resourceName}");
            }
            return stream;
        }

        private static Image GetEmbeddedImage(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = "PiratesFortuneSlot.Resources." + name;
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    MessageBox.Show($"Image resource not found: {resourceName}");
                    return null;
                }
                return Image.FromStream(stream);
            }
        }

        private void InitializeGrid()
        {
            int size = 100;
            for (int row = 0; row < ROWS; row++)
            {
                for (int col = 0; col < COLS; col++)
                {
                    pbGrid[row, col] = new PictureBox
                    {
                        Name = $"pb{row}{col}",
                        Location = new Point(gridCenters[row, col].X - size / 2, gridCenters[row, col].Y - size / 2),
                        Size = new Size(size, size),
                        SizeMode = PictureBoxSizeMode.Zoom,
                        BackColor = Color.Transparent,
                        Padding = new Padding(0),
                        Margin = new Padding(0),
                        BorderStyle = BorderStyle.None
                    };

                    Controls.Add(pbGrid[row, col]);
                }
            }
        }

        private void btnSpin_Click(object sender, EventArgs e)
        {
            if (!inBonus && !isSpinComplete)
            {
                return;
            }

            if (!inBonus)
            {
                bet = (double)nudBet.Value;
                if (balance < bet)
                {
                    MessageBox.Show("Insufficient balance!");
                    isAutoSpinning = false;
                    btnAutoSpin.Text = "Auto Spin";
                    tmrAutoSpin.Stop();
                    UpdateUI();
                    return;
                }
                balance -= bet;
                totalSpinWin = 0.00;
                bonusTriggeredThisSpin = false;
            }
            currentWin = 0.00;
            if (!inBonus) bonusMultiplier = 1;
            isSpinComplete = false;
            spinCount++;

            PlaySound("Spin.wav");
            GenerateGrid();
            AnimateDrop();
            tmrDrop.Start();
            UpdateUI();
        }

        private void btnAutoSpin_Click(object sender, EventArgs e)
        {
            if (inBonus)
            {
                MessageBox.Show("Auto-spin disabled during bonus game!");
                return;
            }
            isAutoSpinning = !isAutoSpinning;
            btnAutoSpin.Text = isAutoSpinning ? "Stop Auto" : "Auto Spin";
            if (isAutoSpinning)
            {
                if (balance < (double)nudBet.Value)
                {
                    MessageBox.Show("Insufficient balance to start auto-spin!");
                    isAutoSpinning = false;
                    btnAutoSpin.Text = "Auto Spin";
                    return;
                }
                tmrAutoSpin.Start();
                if (isSpinComplete)
                    btnSpin_Click(sender, e);
            }
            else
            {
                tmrAutoSpin.Stop();
            }
            UpdateUI();
        }

        private void tmrAutoSpin_Tick(object sender, EventArgs e)
        {
            if (!isAutoSpinning || inBonus || balance < (double)nudBet.Value || !isSpinComplete)
            {
                if (!isAutoSpinning || inBonus || balance < (double)nudBet.Value)
                {
                    isAutoSpinning = false;
                    btnAutoSpin.Text = "Auto Spin";
                    tmrAutoSpin.Stop();
                    UpdateUI();
                    if (balance < (double)nudBet.Value)
                    {
                        MessageBox.Show("Insufficient balance to continue auto-spin!");
                    }
                }
                return;
            }
            btnSpin_Click(sender, e);
        }

        private void AnimateDrop()
        {
            for (int row = 0; row < ROWS; row++)
            {
                for (int col = 0; col < COLS; col++)
                {
                    pbGrid[row, col].Location = new Point(gridCenters[row, col].X - pbGrid[row, col].Width / 2, -480);
                    pbGrid[row, col].Image = null;
                    finalYPositions[row, col] = gridCenters[row, col].Y - pbGrid[row, col].Height / 2;
                }
            }
        }

        private void AnimateCascadeDrop()
        {
            for (int col = 0; col < COLS; col++)
            {
                int writeRow = ROWS - 1;
                List<int> emptyRows = new List<int>();
                for (int readRow = ROWS - 1; readRow >= 0; readRow--)
                {
                    if (grid[readRow, col] != SymbolType.Empty)
                    {
                        if (writeRow != readRow)
                        {
                            pbGrid[writeRow, col].Location = new Point(
                                gridCenters[writeRow, col].X - pbGrid[writeRow, col].Width / 2,
                                gridCenters[readRow, col].Y - pbGrid[readRow, col].Height / 2
                            );
                            pbGrid[writeRow, col].Image = pbGrid[readRow, col].Image;
                        }
                        writeRow--;
                    }
                }
                for (int row = 0; row <= writeRow; row++)
                {
                    emptyRows.Add(row);
                    pbGrid[row, col].Location = new Point(
                        gridCenters[row, col].X - pbGrid[row, col].Width / 2,
                        -480
                    );
                    pbGrid[row, col].Image = null;
                    finalYPositions[row, col] = gridCenters[row, col].Y - pbGrid[row, col].Height / 2;
                }
            }
        }

        private void UpdateUI()
        {
            lblBalance.Text = $"Balance: {balance:F2}";
            nudBet.Value = (decimal)bet;
            lblBonusSpins.Text = $"Bonus Spins: {bonusSpins}";
            lblMultiplier.Text = $"Multiplier: {bonusMultiplier}x";
            lblGoldCoins.Text = $"Gold Coins: {collectedTreasures}/{goldCoinRequirement}";
            lblBiggestWin.Text = $"Biggest Win: {biggestWin:F2}";
            lblBiggestMultiplier.Text = $"Biggest Multiplier: {biggestMultiplier:F2}x";
            lblTotalSpins.Text = $"Total Spins: {spinCount}";
            winPercentage = spinCount > 0 ? (double)spinsWon / spinCount * 100 : 0;
            lblWinPercentage.Text = $"Win Percentage: {winPercentage:F2}%";
            lblBonusSpins.Visible = inBonus;
            lblMultiplier.Visible = inBonus;
            lblGoldCoins.Visible = inBonus;
            btnAutoSpin.Enabled = !inBonus;
            btnSpin.Enabled = !inBonus && isSpinComplete && !isAutoSpinning;
            UpdateBackgroundImage();
            this.Invalidate();
        }

        private void GenerateGrid()
        {
            for (int row = 0; row < ROWS; row++)
            {
                for (int col = 0; col < COLS; col++)
                {
                    int rand = rnd.Next(100);
                    if (rand < 2) grid[row, col] = SymbolType.Ruby;
                    else if (rand < 6) grid[row, col] = SymbolType.Sapphire;
                    else if (rand < 12) grid[row, col] = SymbolType.Emerald;
                    else if (rand < 31) grid[row, col] = SymbolType.RumBottle;
                    else if (rand < 41) grid[row, col] = SymbolType.Compass;
                    else if (rand < 49) grid[row, col] = SymbolType.Map;
                    else if (rand < 63) grid[row, col] = SymbolType.Parrot;
                    else if (rand < 79) grid[row, col] = SymbolType.PirateHat;
                    else if (rand < 91) grid[row, col] = SymbolType.Ship;
                    else if (rand < 94) grid[row, col] = SymbolType.Wild;
                    else if (rand < 97) grid[row, col] = SymbolType.Scatter;
                    else grid[row, col] = SymbolType.GoldCoin;
                }
            }
            if (inBonus)
            {
                int extraWilds = rnd.Next(0, 5);
                for (int i = 0; i < extraWilds; i++)
                {
                    int r = rnd.Next(ROWS), c = rnd.Next(COLS);
                    grid[r, c] = SymbolType.Wild;
                }
            }
        }

        private void tmrDrop_Tick(object sender, EventArgs e)
        {
            const int dropSpeed = 60;
            bool allDropped = true;

            for (int row = 0; row < ROWS; row++)
            {
                for (int col = 0; col < COLS; col++)
                {
                    int targetY = finalYPositions[row, col];
                    if (pbGrid[row, col].Top < targetY)
                    {
                        int newTop = pbGrid[row, col].Top + dropSpeed;
                        if (newTop > targetY) newTop = targetY;
                        pbGrid[row, col].Location = new Point(pbGrid[row, col].Left, newTop);
                        if (newTop == targetY)
                        {
                            var sym = symbols.Find(s => s.Type == grid[row, col]);
                            pbGrid[row, col].Image = sym?.Image;
                            if (grid[row, col] == SymbolType.GoldCoin)
                            {
                                PlaySound("GoldCoin.wav");
                            }
                            else if (grid[row, col] == SymbolType.Scatter)
                            {
                                PlaySound("Win.wav");
                            }
                        }
                        allDropped = false;
                    }
                }
            }

            if (allDropped)
            {
                tmrDrop.Stop();
                UpdateGridDisplay();
                CheckWinsAndCascades();
            }
            this.Invalidate();
        }

        private void tmrCascadeDrop_Tick(object sender, EventArgs e)
        {
            const int dropSpeed = 60;
            bool allDropped = true;

            for (int row = 0; row < ROWS; row++)
            {
                for (int col = 0; col < COLS; col++)
                {
                    int targetY = finalYPositions[row, col];
                    if (pbGrid[row, col].Top < targetY)
                    {
                        int newTop = pbGrid[row, col].Top + dropSpeed;
                        if (newTop > targetY) newTop = targetY;
                        pbGrid[row, col].Location = new Point(pbGrid[row, col].Left, newTop);
                        if (newTop == targetY)
                        {
                            var sym = symbols.Find(s => s.Type == grid[row, col]);
                            pbGrid[row, col].Image = sym?.Image;
                            if (grid[row, col] == SymbolType.GoldCoin)
                            {
                                PlaySound("GoldCoin.wav");
                            }
                            else if (grid[row, col] == SymbolType.Scatter)
                            {
                                PlaySound("Win.wav");
                            }
                        }
                        allDropped = false;
                    }
                }
            }

            if (allDropped)
            {
                tmrCascadeDrop.Stop();
                isCascading = false;
                UpdateGridDisplay();
                CheckWinsAndCascades();
            }
            this.Invalidate();
        }

        private void UpdateGridDisplay()
        {
            for (int row = 0; row < ROWS; row++)
            {
                for (int col = 0; col < COLS; col++)
                {
                    var sym = symbols.Find(s => s.Type == grid[row, col]);
                    pbGrid[row, col].Image = sym?.Image;
                    pbGrid[row, col].Location = new Point(
                        gridCenters[row, col].X - pbGrid[row, col].Width / 2,
                        gridCenters[row, col].Y - pbGrid[row, col].Height / 2
                    );
                }
            }
            this.Invalidate();
        }

        private void CheckWinsAndCascades()
        {
            if (isExploding || isCascading) return;

            bool hasWin = false;
            double totalPayout = 0;
            int scatterCount = 0;
            int goldCoinCount = 0;

            bool[,] visited = new bool[ROWS, COLS];
            List<List<Point>> winningClusters = new List<List<Point>>();

            for (int row = 0; row < ROWS; row++)
            {
                for (int col = 0; col < COLS; col++)
                {
                    if (grid[row, col] == SymbolType.Scatter) scatterCount++;
                    if (grid[row, col] == SymbolType.GoldCoin) goldCoinCount++;
                    if (!visited[row, col] && grid[row, col] != SymbolType.Empty && grid[row, col] != SymbolType.Scatter && grid[row, col] != SymbolType.GoldCoin)
                    {
                        List<Point> cluster = GetCluster(row, col, visited);
                        if (cluster.Count >= 4)
                        {
                            hasWin = true;
                            winningClusters.Add(cluster);
                            double payout = CalculatePayout(cluster);
                            totalPayout += payout;
                        }
                    }
                }
            }

            if (inBonus && goldCoinCount > 0)
            {
                collectedTreasures += goldCoinCount;
                if (collectedTreasures >= goldCoinRequirement && extraSpinsAdded < 11)
                {
                    bonusMultiplier++;
                    bonusSpins += 2;
                    extraSpinsAdded += 1;
                    collectedTreasures = 0;
                    goldCoinRequirement = Math.Min(goldCoinRequirement + 1, 10);
                    biggestMultiplier = Math.Max(biggestMultiplier, bonusMultiplier);
                    MessageBox.Show($"Coins collected! +1 Multiplier, +1 Spin! Next: Collect {goldCoinRequirement} coins");
                    UpdateUI();
                }
            }

            if (hasWin)
            {
                spinsWon++;
                PlaySound("Win.wav");
                baseWin = totalPayout * bet * bonusMultiplier;
                currentWin = baseWin;
                goldCoins = goldCoinCount;
                totalSpinWin += currentWin;
                biggestWin = Math.Max(biggestWin, currentWin);
                if (inBonus)
                {
                    biggestMultiplier = Math.Max(biggestMultiplier, bonusMultiplier);
                }
                else if (goldCoinCount >= 2)
                {
                    biggestMultiplier = Math.Max(biggestMultiplier, goldCoinCount);
                }
                UpdateUI();

                if (winningClusters.Count > 0)
                {
                    currentExplodingCluster = winningClusters[0];
                    isExploding = true;
                    tmrExplode.Start();
                }
            }
            else
            {
                if (!inBonus && scatterCount >= 3 && !bonusTriggeredThisSpin)
                {
                    bonusTriggeredThisSpin = true;
                    EnterBonus(scatterCount);
                }
                else if (inBonus && scatterCount >= 3 && !bonusTriggeredThisSpin)
                {
                    bonusTriggeredThisSpin = true;
                    bonusSpins += 10;
                    PlaySound("BonusPirate.wav");
                    MessageBox.Show("Bonus Retriggered! +10 Spins!");
                    UpdateUI();
                }
                else if (inBonus)
                {
                    bonusSpins--;
                    if (bonusSpins > 0)
                    {
                        bool needsCascade = false;
                        for (int row = 0; row < ROWS; row++)
                        {
                            for (int col = 0; col < COLS; col++)
                            {
                                if (grid[row, col] == SymbolType.Empty)
                                {
                                    needsCascade = true;
                                    break;
                                }
                            }
                            if (needsCascade) break;
                        }
                        if (needsCascade)
                        {
                            CascadeSymbols();
                            AnimateCascadeDrop();
                            isCascading = true;
                            tmrCascadeDrop.Start();
                        }
                        else
                        {
                            btnSpin_Click(null, null);
                        }
                    }
                    else
                    {
                        PlaySound("Win.wav");
                        PlaySound("PirateTalk.wav");
                        inBonus = false;
                        extraSpinsAdded = 0;
                        goldCoinRequirement = 3;
                        collectedTreasures = 0;
                        isAutoSpinning = false;
                        btnAutoSpin.Text = "Auto Spin";
                        tmrAutoSpin.Stop();
                        balance += totalSpinWin;
                        MessageBox.Show($"Bonus over! Total win: {totalSpinWin:F2}");
                        totalSpinWin = 0;
                        lblWin.Visible = false;
                        isSpinComplete = true;
                        bonusTriggeredThisSpin = false;
                        PlayBackgroundMusic("BackgroundMusic.wav");
                        UpdateBackgroundImage();
                        UpdateUI();
                    }
                }
                else
                {
                    bool needsCascade = false;
                    for (int row = 0; row < ROWS; row++)
                    {
                        for (int col = 0; col < COLS; col++)
                        {
                            if (grid[row, col] == SymbolType.Empty)
                            {
                                needsCascade = true;
                                break;
                            }
                        }
                        if (needsCascade) break;
                    }
                    if (needsCascade)
                    {
                        CascadeSymbols();
                        AnimateCascadeDrop();
                        isCascading = true;
                        tmrCascadeDrop.Start();
                    }
                    else
                    {
                        balance += totalSpinWin;
                        totalSpinWin = 0;
                        lblWin.Visible = false;
                        isSpinComplete = true;
                        bonusTriggeredThisSpin = false;
                        UpdateUI();
                    }
                }
            }
        }

        private void tmrExplode_Tick(object sender, EventArgs e)
        {
            tmrExplode.Stop();
            if (currentExplodingCluster != null)
            {
                foreach (var p in currentExplodingCluster)
                {
                    pbGrid[p.Y, p.X].BackColor = Color.FromArgb(255, 255, 100, 100);
                }
                this.Invalidate();
                if (!inBonus && goldCoins >= 2)
                {
                    currentWin *= goldCoins;
                    lblWin.Location = new Point(350, 550);
                    lblWin.Text = $"Base Pay: {baseWin:F2} x{goldCoins} \n Gold Coin Multiplier";
                    lblWin.Visible = true;
                    tmrBaseWinDisplay.Start();
                }
                else
                {
                    lblWin.Location = new Point(450, 600);
                    lblWin.Text = $"Win: {currentWin:F2}";
                    lblWin.Visible = true;
                    tmrWinDisplay.Start();
                }
                PlaySound("Explosion.wav");

                var tmrClear = new Timer { Interval = 300 };
                tmrClear.Tick += (s, ev) =>
                {
                    tmrClear.Stop();
                    ExplodeCluster(currentExplodingCluster);
                    currentExplodingCluster = null;
                    isExploding = false;
                    CascadeSymbols();
                    AnimateCascadeDrop();
                    isCascading = true;
                    tmrCascadeDrop.Start();
                    tmrClear.Dispose();
                };
                tmrClear.Start();
            }
            else
            {
                isExploding = false;
                CascadeSymbols();
                AnimateCascadeDrop();
                isCascading = true;
                tmrCascadeDrop.Start();
            }
        }

        private void tmrBaseWinDisplay_Tick(object sender, EventArgs e)
        {
            tmrBaseWinDisplay.Stop();
            lblWin.Text = $"Win: {totalSpinWin:F2}";
            tmrWinDisplay.Start();
        }

        private void tmrWinDisplay_Tick(object sender, EventArgs e)
        {
            tmrWinDisplay.Stop();
            lblWin.Visible = false;
        }

        private void EnterBonus(int scatters)
        {
            PlaySound("Win.wav");
            PlaySound("BonusPirate.wav");
            inBonus = true;
            bonusSpins = 10 + (scatters > 3 ? 5 * (scatters - 3) : 0);
            bonusMultiplier = 1;
            collectedTreasures = 0;
            extraSpinsAdded = 0;
            goldCoinRequirement = 3;
            isAutoSpinning = false;
            btnAutoSpin.Text = "Auto Spin";
            tmrAutoSpin.Stop();
            PlayBackgroundMusic("Stormy.wav");
            UpdateBackgroundImage();
            MessageBox.Show("Ahoy! Treasure Hunt Bonus Activated!");
            btnSpin_Click(null, null);
        }

        private List<Point> GetCluster(int startRow, int startCol, bool[,] visited)
        {
            List<Point> cluster = new List<Point>();
            SymbolType type = grid[startRow, startCol];
            if (type == SymbolType.Wild || type == SymbolType.Empty || type == SymbolType.Scatter || type == SymbolType.GoldCoin)
                return cluster;

            Stack<Point> stack = new Stack<Point>();
            stack.Push(new Point(startCol, startRow));
            visited[startRow, startCol] = true;

            while (stack.Count > 0)
            {
                Point p = stack.Pop();
                cluster.Add(p);

                int[] dx = { -1, 1, 0, 0 };
                int[] dy = { 0, 0, -1, 1 };
                for (int i = 0; i < 4; i++)
                {
                    int nx = p.X + dx[i], ny = p.Y + dy[i];
                    if (nx >= 0 && nx < COLS && ny >= 0 && ny < ROWS && !visited[ny, nx] &&
                        (grid[ny, nx] == type || grid[ny, nx] == SymbolType.Wild))
                    {
                        visited[ny, nx] = true;
                        stack.Push(new Point(nx, ny));
                    }
                }
            }
            return cluster;
        }

        private double CalculatePayout(List<Point> cluster)
        {
            if (cluster.Count == 0) return 0;
            SymbolType type = grid[cluster[0].Y, cluster[0].X];
            var sym = symbols.Find(s => s.Type == type);
            if (sym == null)
            {
                return 0;
            }
            int size = cluster.Count;
            double payout = 0;
            if (size >= 4 && size <= 20)
            {
                payout = sym.Payouts[size - 4];
            }
            return payout;
        }

        private void ExplodeCluster(List<Point> cluster)
        {
            foreach (var p in cluster)
            {
                grid[p.Y, p.X] = SymbolType.Empty;
                pbGrid[p.Y, p.X].Image = null;
                pbGrid[p.Y, p.X].BackColor = Color.Transparent;
            }
        }

        private void CascadeSymbols()
        {
            for (int col = 0; col < COLS; col++)
            {
                int writeRow = ROWS - 1;
                for (int readRow = ROWS - 1; readRow >= 0; readRow--)
                {
                    if (grid[readRow, col] != SymbolType.Empty)
                    {
                        grid[writeRow, col] = grid[readRow, col];
                        if (writeRow != readRow) grid[readRow, col] = SymbolType.Empty;
                        writeRow--;
                    }
                }
                for (int row = 0; row <= writeRow; row++)
                {
                    int rand = rnd.Next(100);
                    if (rand < 2) grid[row, col] = SymbolType.Ruby;
                    else if (rand < 6) grid[row, col] = SymbolType.Sapphire;
                    else if (rand < 12) grid[row, col] = SymbolType.Emerald;
                    else if (rand < 31) grid[row, col] = SymbolType.RumBottle;
                    else if (rand < 41) grid[row, col] = SymbolType.Compass;
                    else if (rand < 49) grid[row, col] = SymbolType.Map;
                    else if (rand < 63) grid[row, col] = SymbolType.Parrot;
                    else if (rand < 79) grid[row, col] = SymbolType.PirateHat;
                    else if (rand < 91) grid[row, col] = SymbolType.Ship;
                    else if (rand < 94) grid[row, col] = SymbolType.Wild;
                    else if (rand < 97) grid[row, col] = SymbolType.Scatter;
                    else grid[row, col] = SymbolType.GoldCoin;
                }
            }
        }

        private void tmrCascade_Tick(object sender, EventArgs e)
        {
            tmrCascade.Stop();
            CascadeSymbols();
            AnimateCascadeDrop();
            isCascading = true;
            tmrCascadeDrop.Start();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            closingForm = true;
            if (sndBackgroundPlayer != null)
            {
                sndBackgroundPlayer.Stop();
                sndBackgroundPlayer.Dispose();
                sndBackgroundReader?.Dispose();
            }
            foreach (var player in soundPlayers)
            {
                player.Stop();
                player.Dispose();
            }
            foreach (var reader in soundReaders)
            {
                reader.Dispose();
            }
            if (this.BackgroundImage != null)
            {
                this.BackgroundImage.Dispose();
            }
        }
    }
}