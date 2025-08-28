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
        private int balance = 1000;
        private int currentWin = 0;
        private int totalSpinWin = 0;
        private int bet = 10;
        private Random rnd = new Random();
        private bool inBonus = false;
        private int bonusSpins = 0;
        private int bonusMultiplier = 1;
        private double biggestMultiplier = 1.0;
        private int biggestWin = 0;
        private int collectedTreasures = 0;
        private int extraSpinsAdded = 0;
        private int goldCoinRequirement = 3;
        private bool isAutoSpinning = false;
        private bool isSpinComplete = true;
        private IWavePlayer sndBackgroundPlayer;
        private WaveFileReader sndBackgroundReader;
        private List<IWavePlayer> soundPlayers = new List<IWavePlayer>();
        private List<WaveFileReader> soundReaders = new List<WaveFileReader>();
        private int[,] finalYPositions = new int[ROWS, COLS];
        private int spinCount = 0;
        private Label lblBonusSpins;
        private Label lblMultiplier;
        private Label lblGoldCoins;
        private Label lblBiggestWin;
        private Label lblBiggestMultiplier;
        private Button btnAutoSpin;
        private Timer tmrWinDisplay;
        private Timer tmrBaseWinDisplay;
        private Timer tmrAutoSpin;
        private Point lblWinInitialPos;
        private int baseWin;

        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true;

            lblWin.Location = new Point(640 - 100, 360);
            lblWin.Font = new Font("Arial", 24, FontStyle.Bold);
            lblWin.ForeColor = Color.FromArgb(255, 215, 0);
            lblWin.BackColor = Color.Transparent;
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
                Text = "Biggest Win: 0",
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
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Size = new Size(1280, 720);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackgroundImageLayout = ImageLayout.Stretch;
            this.BackgroundImage = GetEmbeddedImage("Background1.jpg");
            lblBalance.Location = new Point(25, 630);
            lblBalance.Font = new Font("Arial", 12, FontStyle.Bold);
            lblBalance.ForeColor = Color.White;
            lblBalance.BackColor = Color.Transparent;
            nudBet.Location = new Point(850, 600);
            nudBet.Font = new Font("Arial", 12);
            nudBet.Size = new Size(80, 25);
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

            InitializeGrid();
            GenerateGrid();
            UpdateGridDisplay();
            UpdateUI();
            PlayBackgroundMusic("BackgroundMusic.wav");
        }

        private void InitializeSymbols()
        {
            var symbolData = new[]
            {
                new { Type = SymbolType.Ruby, ImageName = "Ruby.png", Payouts = new double[] { 11480.76, 16231.94, 22961.52, 32463.88, 45923.04, 64927.76, 91846.08, 129855.52, 183692.16, 259711.04, 367384.32, 519422.08, 734768.64, 1038844.16, 1469537.28, 2077768.32 } },
                new { Type = SymbolType.Sapphire, ImageName = "Sapphire.png", Payouts = new double[] { 138.83, 196.21, 277.66, 392.42, 555.32, 784.84, 1110.64, 1569.68, 2221.28, 3139.36, 4442.56, 6278.72, 8885.12, 12557.44, 17770.24, 25114.88 } },
                new { Type = SymbolType.Emerald, ImageName = "Emerald.png", Payouts = new double[] { 20.67, 29.22, 41.34, 58.44, 82.68, 116.88, 165.36, 233.76, 330.72, 467.52, 661.44, 935.04, 1322.88, 1870.08, 2645.76, 3740.16 } },
                new { Type = SymbolType.RumBottle, ImageName = "RumBottle.png", Payouts = new double[] { 0.10, 0.14, 0.20, 0.28, 0.40, 0.57, 0.80, 1.13, 1.60, 2.26, 3.20, 4.53, 6.40, 9.05, 12.80, 18.10 } },
                new { Type = SymbolType.Compass, ImageName = "Compass.png", Payouts = new double[] { 0.65, 0.92, 1.30, 1.84, 2.60, 3.67, 5.20, 7.35, 10.40, 14.70, 20.80, 29.40, 41.60, 58.80, 83.20, 117.60 } },
                new { Type = SymbolType.Map, ImageName = "Map.png", Payouts = new double[] { 1.72, 2.43, 3.44, 4.86, 6.88, 9.72, 13.76, 19.44, 27.52, 38.88, 55.04, 77.76, 110.08, 155.52, 220.16, 311.04 } },
                new { Type = SymbolType.Parrot, ImageName = "Parrot.png", Payouts = new double[] { 0.24, 0.34, 0.48, 0.68, 0.96, 1.36, 1.92, 2.71, 3.84, 5.43, 7.68, 10.85, 15.36, 21.71, 30.72, 43.42 } },
                new { Type = SymbolType.PirateHat, ImageName = "PirateHat.png", Payouts = new double[] { 0.15, 0.21, 0.30, 0.42, 0.60, 0.85, 1.20, 1.70, 2.40, 3.39, 4.80, 6.79, 9.60, 13.58, 19.20, 27.15 } },
                new { Type = SymbolType.Ship, ImageName = "Ship.png", Payouts = new double[] { 0.40, 0.57, 0.80, 1.13, 1.60, 2.26, 3.20, 4.53, 6.40, 9.05, 12.80, 18.10, 25.60, 36.20, 51.20, 72.40 } },
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
                    if (sndBackgroundReader != null)
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
            string imageName = inBonus ? "Stormy.jpg" : "Background1.jpg";
            var newBackground = GetEmbeddedImage(imageName);
            if (newBackground != null)
            {
                if (this.BackgroundImage != null)
                {
                    this.BackgroundImage.Dispose();
                }
                this.BackgroundImage = newBackground;
            }
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
            int startX = 200, startY = 50, size = 120, spacing = 130;
            for (int row = 0; row < ROWS; row++)
            {
                for (int col = 0; col < COLS; col++)
                {
                    pbGrid[row, col] = new PictureBox
                    {
                        Name = $"pb{row}{col}",
                        Location = new Point(startX + col * spacing, startY + row * spacing),
                        Size = new Size(size, size),
                        SizeMode = PictureBoxSizeMode.Zoom
                    };
                    Controls.Add(pbGrid[row, col]);
                }
            }
        }

        private void btnSpin_Click(object sender, EventArgs e)
        {
            if (!inBonus)
            {
                bet = (int)nudBet.Value;
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
                totalSpinWin = 0;
            }
            currentWin = 0;
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
                if (balance < (int)nudBet.Value)
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
            if (!isAutoSpinning || inBonus || balance < (int)nudBet.Value || !isSpinComplete)
            {
                if (!isAutoSpinning || inBonus || balance < (int)nudBet.Value)
                {
                    isAutoSpinning = false;
                    btnAutoSpin.Text = "Auto Spin";
                    tmrAutoSpin.Stop();
                    UpdateUI();
                    if (balance < (int)nudBet.Value)
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
                    pbGrid[row, col].Location = new Point(pbGrid[row, col].Left, -480);
                    pbGrid[row, col].Image = null;
                    finalYPositions[row, col] = 50 + row * 130;
                }
            }
        }

        private void UpdateUI()
        {
            lblBalance.Text = $"Balance: {balance}";
            nudBet.Value = bet;
            lblBonusSpins.Text = $"Bonus Spins: {bonusSpins}";
            lblMultiplier.Text = $"Multiplier: {bonusMultiplier}x";
            lblGoldCoins.Text = $"Gold Coins: {collectedTreasures}/{goldCoinRequirement}";
            lblBiggestWin.Text = $"Biggest Win: {biggestWin}";
            lblBiggestMultiplier.Text = $"Biggest Multiplier: {biggestMultiplier:F2}x";
            lblBonusSpins.Visible = inBonus;
            lblMultiplier.Visible = inBonus;
            lblGoldCoins.Visible = inBonus;
            btnAutoSpin.Enabled = !inBonus;
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

        private void UpdateGridDisplay()
        {
            for (int row = 0; row < ROWS; row++)
            {
                for (int col = 0; col < COLS; col++)
                {
                    pbGrid[row, col].Image = null;
                    var sym = symbols.Find(s => s.Type == grid[row, col]);
                    pbGrid[row, col].Image = sym?.Image;
                }
            }
            this.Invalidate();
        }

        private void CheckWinsAndCascades()
        {
            bool hasWin = false;
            double totalPayout = 0;
            int scatterCount = 0;
            int goldCoinCount = 0;

            bool[,] visited = new bool[ROWS, COLS];
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
                            double payout = CalculatePayout(cluster);
                            totalPayout += payout;
                            ExplodeCluster(cluster);
                        }
                    }
                }
            }

            if (inBonus && goldCoinCount > 0)
            {
                collectedTreasures += goldCoinCount;
                if (collectedTreasures >= goldCoinRequirement && extraSpinsAdded < 10)
                {
                    bonusMultiplier++;
                    bonusSpins += 1;
                    extraSpinsAdded += 1;
                    collectedTreasures = 0;
                    goldCoinRequirement = Math.Min(goldCoinRequirement + 1, 10);
                    biggestMultiplier = Math.Max(biggestMultiplier, bonusMultiplier);
                    MessageBox.Show($"Treasure collected! +1 Multiplier, +1 Spin! Next: {goldCoinRequirement} coins");
                    UpdateUI();
                }
            }

            if (inBonus && scatterCount >= 3)
            {
                bonusSpins += 10;
                PlaySound("BonusPirate.wav");
                MessageBox.Show("Bonus Retriggered! +10 Spins!");
            }

            if (hasWin)
            {
                PlaySound("Explosion.wav");
                baseWin = (int)(totalPayout * bet * bonusMultiplier);
                currentWin = baseWin;
                if (!inBonus && goldCoinCount >= 2)
                {
                    currentWin *= goldCoinCount;
                    lblWin.Text = $"Base Pay: {baseWin} x{goldCoinCount}";
                    lblWin.Visible = true;
                    tmrBaseWinDisplay.Start();
                }
                else
                {
                    lblWin.Text = $"Win: {currentWin}";
                    lblWin.Visible = true;
                    tmrWinDisplay.Start();
                }
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
                tmrCascade.Start();
            }
            else
            {
                if (scatterCount >= 3 && !inBonus)
                {
                    EnterBonus(scatterCount);
                }
                else if (inBonus)
                {
                    bonusSpins--;
                    if (bonusSpins > 0)
                    {
                        btnSpin_Click(null, null);
                    }
                    else
                    {
                        PlaySound("PirateTalk.wav");
                        inBonus = false;
                        extraSpinsAdded = 0;
                        goldCoinRequirement = 3;
                        collectedTreasures = 0;
                        isAutoSpinning = false;
                        btnAutoSpin.Text = "Auto Spin";
                        tmrAutoSpin.Stop();
                        balance += totalSpinWin;
                        MessageBox.Show($"Bonus over! Total win: {totalSpinWin}");
                        totalSpinWin = 0;
                        lblWin.Visible = false;
                        isSpinComplete = true;
                        PlayBackgroundMusic("BackgroundMusic.wav");
                        UpdateBackgroundImage();
                        UpdateUI();
                    }
                }
                else
                {
                    balance += totalSpinWin;
                    totalSpinWin = 0;
                    lblWin.Visible = false;
                    isSpinComplete = true;
                    UpdateUI();
                }
            }
        }

        private void tmrBaseWinDisplay_Tick(object sender, EventArgs e)
        {
            tmrBaseWinDisplay.Stop();
            lblWin.Text = $"Win: {totalSpinWin}";
            tmrWinDisplay.Start();
        }

        private void tmrWinDisplay_Tick(object sender, EventArgs e)
        {
            tmrWinDisplay.Stop();
            lblWin.Visible = false;
        }

        private void EnterBonus(int scatters)
        {
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
            }
            PlaySound("Explosion.wav");
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
                    if (inBonus)
                    {
                        int rand = rnd.Next(1000);
                        if (rand < 17) grid[row, col] = SymbolType.GoldCoin;
                        else if (rand < 37) grid[row, col] = SymbolType.Ruby;
                        else if (rand < 77) grid[row, col] = SymbolType.Sapphire;
                        else if (rand < 137) grid[row, col] = SymbolType.Emerald;
                        else if (rand < 327) grid[row, col] = SymbolType.RumBottle;
                        else if (rand < 427) grid[row, col] = SymbolType.Compass;
                        else if (rand < 507) grid[row, col] = SymbolType.Map;
                        else if (rand < 647) grid[row, col] = SymbolType.Parrot;
                        else if (rand < 807) grid[row, col] = SymbolType.PirateHat;
                        else if (rand < 927) grid[row, col] = SymbolType.Ship;
                        else if (rand < 957) grid[row, col] = SymbolType.Wild;
                        else if (rand < 987) grid[row, col] = SymbolType.Scatter;
                        else grid[row, col] = SymbolType.GoldCoin;
                    }
                    else
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
        }

        private void tmrCascade_Tick(object sender, EventArgs e)
        {
            tmrCascade.Stop();
            CascadeSymbols();
            UpdateGridDisplay();
            CheckWinsAndCascades();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
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
        }
    }
}