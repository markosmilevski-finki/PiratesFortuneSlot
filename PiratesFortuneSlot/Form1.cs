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
        private int bet = 10;
        private Random rnd = new Random();
        private bool inBonus = false;
        private int bonusSpins = 0;
        private int bonusMultiplier = 1;
        private int collectedTreasures = 0;
        private SoundPlayer sndSpin, sndWin, sndBonus, sndExplosion, sndBackground;
        private int dropStep = 0;
        private int[,] finalYPositions = new int[ROWS, COLS];

        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
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
            lblWin.Location = new Point(500, 600);
            lblWin.Font = new Font("Arial", 12);
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
                new { Type = SymbolType.Ruby, ImageName = "Ruby.png", Payouts = new double[] {0.25, 0.5, 5}},
                new { Type = SymbolType.Sapphire, ImageName = "Sapphire.png", Payouts = new double[] {0.25, 0.5, 5}},
                new { Type = SymbolType.Emerald, ImageName = "Emerald.png", Payouts = new double[] {0.5, 1, 8}},
                new { Type = SymbolType.RumBottle, ImageName = "RumBottle.png", Payouts = new double[] {1, 2, 10}},
                new { Type = SymbolType.Compass, ImageName = "Compass.png", Payouts = new double[] {1, 2, 12}},
                new { Type = SymbolType.Map, ImageName = "Map.png", Payouts = new double[] {2, 4, 15}},
                new { Type = SymbolType.Parrot, ImageName = "Parrot.png", Payouts = new double[] {5, 10, 25}},
                new { Type = SymbolType.PirateHat, ImageName = "PirateHat.png", Payouts = new double[] {10, 20, 50}},
                new { Type = SymbolType.Ship, ImageName = "Ship.png", Payouts = new double[] {15, 25, 100}},
                new { Type = SymbolType.Wild, ImageName = "Wild.png", Payouts = new double[] {0, 0, 0}},
                new { Type = SymbolType.Scatter, ImageName = "Scatter.png", Payouts = new double[] {0, 0, 0}},
                new { Type = SymbolType.GoldCoin, ImageName = "GoldCoin.png", Payouts = new double[] {0, 0, 0}}
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
            sndBonus = new SoundPlayer(GetEmbeddedResourceStream("Explosion.wav"));
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
            bet = (int)nudBet.Value;
            if (balance < bet) { MessageBox.Show("Insufficient balance!"); return; }

            balance -= bet;
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
            lblWin.Text = $"Win: {currentWin}";
            nudBet.Value = bet;
        }

        private void GenerateGrid()
        {
            SymbolType[] validSymbols = new SymbolType[] { SymbolType.Ruby, SymbolType.Sapphire, SymbolType.Emerald,
                                                          SymbolType.RumBottle, SymbolType.Compass, SymbolType.Map,
                                                          SymbolType.Parrot, SymbolType.PirateHat, SymbolType.Ship,
                                                          SymbolType.Wild, SymbolType.Scatter, SymbolType.GoldCoin };
            for (int row = 0; row < ROWS; row++)
            {
                for (int col = 0; col < COLS; col++)
                {
                    int rand = rnd.Next(100);
                    if (rand < 40) grid[row, col] = validSymbols[rnd.Next(0, 3)];
                    else if (rand < 70) grid[row, col] = validSymbols[rnd.Next(3, 6)];
                    else if (rand < 90) grid[row, col] = validSymbols[rnd.Next(6, 9)];
                    else if (rand < 95) grid[row, col] = SymbolType.Wild;
                    else grid[row, col] = SymbolType.Scatter;
                }
            }
            if (inBonus)
            {
                int extraWilds = rnd.Next(2, 6);
                for (int i = 0; i < extraWilds; i++)
                {
                    int r = rnd.Next(ROWS), c = rnd.Next(COLS);
                    grid[r, c] = SymbolType.Wild;
                }
                for (int row = 0; row < ROWS; row++)
                {
                    for (int col = 0; col < COLS; col++)
                    {
                        if (rnd.Next(100) < 10 && symbols.Exists(s => s.Type == SymbolType.GoldCoin))
                            grid[row, col] = SymbolType.GoldCoin;
                    }
                }
            }
        }

        private void tmrDrop_Tick(object sender, EventArgs e)
        {
            const int dropSpeed = 120;
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
                        if (cluster.Count >= 8)
                        {
                            hasWin = true;
                            totalPayout += CalculatePayout(cluster);
                            ExplodeCluster(cluster);
                        }
                    }
                }
            }

            if (inBonus && goldCoinCount > 0)
            {
                collectedTreasures += goldCoinCount;
                if (collectedTreasures >= 5)
                {
                    bonusMultiplier++;
                    bonusSpins += 2;
                    collectedTreasures = 0;
                    MessageBox.Show("Treasure collected! +1 Multiplier, +2 Spins!");
                }
            }

            if (hasWin)
            {
                sndWin?.Play();
                sndExplosion?.Play();
                currentWin += (int)(totalPayout * bet * bonusMultiplier);
                if (inBonus) bonusMultiplier++;
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
                        inBonus = false;
                        MessageBox.Show($"Bonus over! Total win: {currentWin}");
                    }
                }
                balance += currentWin;
                currentWin = 0;
                UpdateUI();
            }
        }

        private void EnterBonus(int scatters)
        {
            sndBonus?.Play();
            inBonus = true;
            bonusSpins = 10 + (scatters > 3 ? 5 * (scatters - 3) : 0);
            bonusMultiplier = 1;
            collectedTreasures = 0;
            MessageBox.Show("Ahoy! Treasure Hunt Bonus Activated!");
            btnSpin_Click(null, null);
        }

        private List<Point> GetCluster(int startRow, int startCol, bool[,] visited)
        {
            List<Point> cluster = new List<Point>();
            SymbolType type = grid[startRow, startCol];
            if (type == SymbolType.Wild) return cluster;

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
            SymbolType type = grid[cluster[0].Y, cluster[0].X];
            var sym = symbols.Find(s => s.Type == type);
            if (sym == null) return 0;
            int size = cluster.Count;
            if (size >= 12) return sym.Payouts[2];
            if (size >= 10) return sym.Payouts[1];
            return sym.Payouts[0];
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
                    if (rand < 40) grid[row, col] = (SymbolType)rnd.Next(0, 3);
                    else if (rand < 70) grid[row, col] = (SymbolType)rnd.Next(3, 6);
                    else if (rand < 90) grid[row, col] = (SymbolType)rnd.Next(6, 9);
                    else if (rand < 95) grid[row, col] = SymbolType.Wild;
                    else grid[row, col] = SymbolType.Scatter;
                    if (inBonus && rnd.Next(100) < 10 && symbols.Exists(s => s.Type == SymbolType.GoldCoin))
                        grid[row, col] = SymbolType.GoldCoin;
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