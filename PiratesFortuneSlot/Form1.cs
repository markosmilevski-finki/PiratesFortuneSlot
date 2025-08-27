using System;
using System.Collections.Generic;
using System.Drawing;
using System.Media;
using System.Reflection;
using System.Windows.Forms;
using System.IO;

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
        private int collectedTreasures = 0;
        private int extraSpinsAdded = 0;
        private SoundPlayer sndSpin, sndWin, sndBonus, sndExplosion, sndBackground;
        private int dropStep = 0;
        private int[,] finalYPositions = new int[ROWS, COLS];
        private Label lblBonusSpins;
        private Label lblMultiplier;
        private Timer tmrWinDisplay;
        private Point lblWinInitialPos;

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
                Location = new Point(500, 630),
                Font = new Font("Arial", 12),
                Size = new Size(150, 25),
                Text = "Bonus Spins: 0",
                Visible = false
            };
            Controls.Add(lblBonusSpins);

            lblMultiplier = new Label
            {
                Location = new Point(500, 660),
                Font = new Font("Arial", 12),
                Size = new Size(150, 25),
                Text = "Multiplier: 1x",
                Visible = false
            };
            Controls.Add(lblMultiplier);

            tmrWinDisplay = new Timer
            {
                Interval = 2000
            };
            tmrWinDisplay.Tick += tmrWinDisplay_Tick;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Size = new Size(1280, 720);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackgroundImageLayout = ImageLayout.Stretch;

            lblBalance.Location = new Point(150, 600);
            lblBalance.Font = new Font("Arial", 12);
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
            PlayBackgroundMusic();
        }

        private void InitializeSymbols()
        {
            var symbolData = new[]
            {
                new { Type = SymbolType.Ruby, ImageName = "Ruby.png", Payouts = new double[] {50, 75, 100, 150, 200, 250, 300, 350, 400, 450, 500, 600, 700, 800, 900, 1000}},
                new { Type = SymbolType.Sapphire, ImageName = "Sapphire.png", Payouts = new double[] {45, 67.5, 90, 135, 180, 225, 270, 315, 360, 405, 450, 540, 630, 720, 810, 900}},
                new { Type = SymbolType.Emerald, ImageName = "Emerald.png", Payouts = new double[] {40, 60, 80, 120, 160, 200, 240, 280, 320, 360, 400, 480, 560, 640, 720, 800}},
                new { Type = SymbolType.RumBottle, ImageName = "RumBottle.png", Payouts = new double[] {10, 15, 20, 30, 40, 50, 60, 70, 80, 90, 100, 120, 140, 160, 180, 200}},
                new { Type = SymbolType.Compass, ImageName = "Compass.png", Payouts = new double[] {8, 12, 16, 24, 32, 40, 48, 56, 64, 72, 80, 96, 112, 128, 144, 160}},
                new { Type = SymbolType.Map, ImageName = "Map.png", Payouts = new double[] {6, 9, 12, 18, 24, 30, 36, 42, 48, 54, 60, 72, 84, 96, 108, 120}},
                new { Type = SymbolType.Parrot, ImageName = "Parrot.png", Payouts = new double[] {4, 6, 8, 12, 16, 20, 24, 28, 32, 36, 40, 48, 56, 64, 72, 80}},
                new { Type = SymbolType.PirateHat, ImageName = "PirateHat.png", Payouts = new double[] {3, 4.5, 6, 9, 12, 15, 18, 21, 24, 27, 30, 36, 42, 48, 54, 60}},
                new { Type = SymbolType.Ship, ImageName = "Ship.png", Payouts = new double[] {2, 3, 4, 6, 8, 10, 12, 14, 16, 18, 20, 24, 28, 32, 36, 40}},
                new { Type = SymbolType.Wild, ImageName = "Wild.png", Payouts = new double[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}},
                new { Type = SymbolType.Scatter, ImageName = "Scatter.png", Payouts = new double[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}},
                new { Type = SymbolType.GoldCoin, ImageName = "GoldCoin.png", Payouts = new double[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}}
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
            sndSpin = new SoundPlayer(GetEmbeddedResourceStream("Spin.wav"));
            sndWin = new SoundPlayer(GetEmbeddedResourceStream("Win.wav"));
            sndBonus = new SoundPlayer(GetEmbeddedResourceStream("Win.wav"));
            sndExplosion = new SoundPlayer(GetEmbeddedResourceStream("Explosion.wav"));
            sndBackground = new SoundPlayer(GetEmbeddedResourceStream("BackgroundMusic.wav"));
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

        private void PlayBackgroundMusic()
        {
            if (sndBackground != null)
                sndBackground.PlayLooping();
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
                if (balance < bet) { MessageBox.Show("Insufficient balance!"); return; }
                balance -= bet;
                totalSpinWin = 0;
            }
            currentWin = 0;
            if (!inBonus) bonusMultiplier = 1;

            sndSpin?.Play();
            GenerateGrid();
            AnimateDrop();
            tmrDrop.Start();
            UpdateUI();
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
            lblWin.Text = $"Win: {totalSpinWin}";
            lblWin.Location = lblWinInitialPos;
            nudBet.Value = bet;
            lblBonusSpins.Text = $"Bonus Spins: {bonusSpins}";
            lblMultiplier.Text = $"Multiplier: {bonusMultiplier}x";
            lblBonusSpins.Visible = inBonus;
            lblMultiplier.Visible = inBonus;
            this.Invalidate();
        }

        private void GenerateGrid()
        {
            double[] probabilities = { 2.8, 4.2, 5.7, 7.1, 8.5, 9.9, 11.4, 12.8, 14.2, 2.0, 2.0, 2.0 };
            SymbolType[] validSymbols = new SymbolType[] { SymbolType.Ruby, SymbolType.Sapphire, SymbolType.Emerald,
                                                          SymbolType.RumBottle, SymbolType.Compass, SymbolType.Map,
                                                          SymbolType.Parrot, SymbolType.PirateHat, SymbolType.Ship,
                                                          SymbolType.Wild, SymbolType.Scatter, SymbolType.GoldCoin };

            for (int row = 0; row < ROWS; row++)
            {
                for (int col = 0; col < COLS; col++)
                {
                    int rand = rnd.Next(100);
                    double cumProb = 0;
                    for (int i = 0; i < probabilities.Length; i++)
                    {
                        cumProb += probabilities[i];
                        if (rand < cumProb)
                        {
                            grid[row, col] = validSymbols[i];
                            break;
                        }
                    }
                }
            }
            if (inBonus)
            {
                int extraWilds = rnd.Next(3, 7);
                for (int i = 0; i < extraWilds; i++)
                {
                    int r = rnd.Next(ROWS), c = rnd.Next(COLS);
                    grid[r, c] = SymbolType.Wild;
                }
                for (int row = 0; row < ROWS; row++)
                {
                    for (int col = 0; col < COLS; col++)
                    {
                        if (rnd.Next(100) < 1)
                            grid[row, col] = SymbolType.GoldCoin;
                    }
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
                        }
                        allDropped = false;
                    }
                }
            }

            if (allDropped)
            {
                tmrDrop.Stop();
                dropStep = 0;
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
                        if (cluster.Count >= 5)
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
                if (collectedTreasures >= 5 && extraSpinsAdded < 5)
                {
                    bonusMultiplier++;
                    bonusSpins += 2;
                    extraSpinsAdded += 2;
                    MessageBox.Show("Treasure collected! +1 Multiplier, +2 Spins!");
                }
            }

            if (hasWin)
            {
                sndWin?.Play();
                sndExplosion?.Play();
                currentWin = (int)(totalPayout * bet * bonusMultiplier);
                totalSpinWin += currentWin;
                lblWin.Text = $"Win: {totalSpinWin}";
                lblWin.Visible = true;
                tmrWinDisplay.Start();
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
                        sndWin.Play();
                        inBonus = false;
                        extraSpinsAdded = 0;
                        balance += totalSpinWin;
                        MessageBox.Show($"Bonus over! Total win: {totalSpinWin}");
                        totalSpinWin = 0;
                        lblWin.Visible = false;
                        UpdateUI();
                    }
                }
                else
                {
                    balance += totalSpinWin;
                    totalSpinWin = 0;
                    lblWin.Visible = false;
                    UpdateUI();
                }
            }
        }

        private void tmrWinDisplay_Tick(object sender, EventArgs e)
        {
            tmrWinDisplay.Stop();
            lblWin.Visible = false;
        }

        private void EnterBonus(int scatters)
        {
            sndBonus?.Play();
            inBonus = true;
            bonusSpins = 10 + (scatters > 3 ? 5 * (scatters - 3) : 0);
            bonusMultiplier = 1;
            collectedTreasures = 0;
            extraSpinsAdded = 0;
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
            if (size < 5) return 0;
            int index = Math.Min(size - 5, 15);
            double payout = sym.Payouts[index];
            return payout;
        }

        private void ExplodeCluster(List<Point> cluster)
        {
            foreach (var p in cluster)
            {
                grid[p.Y, p.X] = SymbolType.Empty;
                pbGrid[p.Y, p.X].Image = null;
            }
            sndExplosion?.Play();
        }

        private void CascadeSymbols()
        {
            for (int col = 0; col < COLS; col++)
            {
                List<SymbolType> nonEmptySymbols = new List<SymbolType>();
                for (int row = 0; row < ROWS; row++)
                {
                    if (grid[row, col] != SymbolType.Empty)
                    {
                        nonEmptySymbols.Add(grid[row, col]);
                    }
                }

                for (int row = ROWS - 1; row >= 0; row--)
                {
                    if (nonEmptySymbols.Count > 0)
                    {
                        grid[row, col] = nonEmptySymbols[nonEmptySymbols.Count - 1];
                        nonEmptySymbols.RemoveAt(nonEmptySymbols.Count - 1);
                    }
                    else
                    {
                        grid[row, col] = SymbolType.Empty;
                    }
                }
            }

            double[] probabilities = { 2.8, 4.2, 5.7, 7.1, 8.5, 9.9, 11.4, 12.8, 14.2, 2.0, 2.0, 2.0 };
            SymbolType[] validSymbols = { SymbolType.Ruby, SymbolType.Sapphire, SymbolType.Emerald,
                                         SymbolType.RumBottle, SymbolType.Compass, SymbolType.Map,
                                         SymbolType.Parrot, SymbolType.PirateHat, SymbolType.Ship,
                                         SymbolType.Wild, SymbolType.Scatter, SymbolType.GoldCoin };

            for (int row = 0; row < ROWS; row++)
            {
                for (int col = 0; col < COLS; col++)
                {
                    if (grid[row, col] == SymbolType.Empty)
                    {
                        int rand = rnd.Next(100);
                        double cumProb = 0;
                        for (int i = 0; i < probabilities.Length; i++)
                        {
                            cumProb += probabilities[i];
                            if (rand < cumProb)
                            {
                                grid[row, col] = validSymbols[i];
                                break;
                            }
                        }
                        if (inBonus && rnd.Next(100) < 1)
                            grid[row, col] = SymbolType.GoldCoin;
                    }
                }
            }

            for (int row = 0; row < ROWS; row++)
            {
                for (int col = 0; col < COLS; col++)
                {
                    var sym = symbols.Find(s => s.Type == grid[row, col]);
                    pbGrid[row, col].Image = sym?.Image ?? null;
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
    }
}