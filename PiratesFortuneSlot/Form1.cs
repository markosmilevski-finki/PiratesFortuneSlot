using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace PiratesFortuneSlot
{
    public partial class Form1 : Form
    {
        private enum SymbolType
        {
            Ruby, Sapphire, Emerald, RumBottle, Compass, Map, Parrot, PirateHat, Ship, Wild, Scatter, Empty
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

        private List<Symbol> symbols = new List<Symbol>
        {
            new Symbol(SymbolType.Ruby, "Ruby.png", new double[] {0.25, 0.5, 5}),
            new Symbol(SymbolType.Sapphire, "Sapphire.png", new double[] {0.25, 0.5, 5}),
            new Symbol(SymbolType.Emerald, "Emerald.png", new double[] {0.5, 1, 8}),
            new Symbol(SymbolType.RumBottle, "RumBottle.png", new double[] {1, 2, 10}),
            new Symbol(SymbolType.Compass, "Compass.png", new double[] {1, 2, 12}),
            new Symbol(SymbolType.Map, "Map.png", new double[] {2, 4, 15}),
            new Symbol(SymbolType.Parrot, "Parrot.png", new double[] {5, 10, 25}),
            new Symbol(SymbolType.PirateHat, "PirateHat.png", new double[] {10, 20, 50}),
            new Symbol(SymbolType.Ship, "Ship.png", new double[] {15, 25, 100}),
            new Symbol(SymbolType.Wild, "Wild.png", new double[] {0, 0, 0}),
            new Symbol(SymbolType.Scatter, "Scatter.png", new double[] {0, 0, 0})
        };

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

        public Form1()
        {
            InitializeComponent();
            InitializeGrid();
            LoadSounds();
            UpdateUI();
            PlayBackgroundMusic();
        }

        private void LoadSounds()
        {
            sndSpin = new SoundPlayer(GetEmbeddedResourceStream("Spin.wav"));
            sndWin = new SoundPlayer(GetEmbeddedResourceStream("Win.wav"));
            sndBonus = new SoundPlayer(GetEmbeddedResourceStream("Bonus.wav"));
            sndExplosion = new SoundPlayer(GetEmbeddedResourceStream("Explosion.wav"));
            sndBackground = new SoundPlayer(GetEmbeddedResourceStream("BackgroundMusic.wav"));
        }

        private static Stream GetEmbeddedResourceStream(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            return assembly.GetManifestResourceStream("PiratesFortuneSlot.Resources." + name);
        }

        private void PlayBackgroundMusic()
        {
            sndBackground.PlayLooping();
        }

        private static Image GetEmbeddedImage(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream("PiratesFortuneSlot.Resources." + name))
            {
                return Image.FromStream(stream);
            }
        }

        private void InitializeGrid()
        {
            int startX = 150, startY = 100, size = 100, spacing = 110;
            for (int row = 0; row < ROWS; row++)
            {
                for (int col = 0; col < COLS; col++)
                {
                    pbGrid[row, col] = new PictureBox
                    {
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

            sndSpin.Play();

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
                    pbGrid[row, col].Location = new Point(pbGrid[row, col].Left, -800);
                    pbGrid[row, col].Image = null;
                }
            }
            tmrDrop.Start();
        }

        private void UpdateUI()
        {
            lblBalance.Text = $"Balance: {balance}";
            lblWin.Text = $"Win: {currentWin}";
            nudBet.Value = bet;
        }

        private void GenerateGrid()
        {
            for (int row = 0; row < ROWS; row++)
            {
                for (int col = 0; col < COLS; col++)
                {
                    int rand = rnd.Next(100);
                    if (rand < 40) grid[row, col] = (SymbolType)rnd.Next(0, 3);
                    else if (rand < 70) grid[row, col] = (SymbolType)rnd.Next(3, 6);
                    else if (rand < 90) grid[row, col] = (SymbolType)rnd.Next(6, 9);
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
            }
        }

        private int dropStep = 0;
        private int[,] finalYPositions = new int[ROWS, COLS];

        private void tmrDrop_Tick(object sender, EventArgs e)
        {
            const int dropSpeed = 50;
            bool allDropped = true;

            for (int row = 0; row < ROWS; row++)
            {
                for (int col = 0; col < COLS; col++)
                {
                    int targetY = 100 + row * 210;
                    if (pbGrid[row, col].Top < targetY)
                    {
                        pbGrid[row, col].Location = new Point(pbGrid[row, col].Left, pbGrid[row, col].Top + dropSpeed);
                        allDropped = false;
                    }
                    else
                    {
                        pbGrid[row, col].Location = new Point(pbGrid[row, col].Left, targetY);
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
        }

        private void CheckWinsAndCascades()
        {
            bool hasWin = false;
            double totalPayout = 0;
            int scatterCount = 0;

            bool[,] visited = new bool[ROWS, COLS];
            for (int row = 0; row < ROWS; row++)
            {
                for (int col = 0; col < COLS; col++)
                {
                    if (grid[row, col] == SymbolType.Scatter) scatterCount++;
                    if (!visited[row, col] && grid[row, col] != SymbolType.Empty && grid[row, col] != SymbolType.Scatter)
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

            if (hasWin)
            {
                sndWin.Play();
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
            sndExplosion.Play();
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
        }

        private void tmrCascade_Tick(object sender, EventArgs e)
        {
            tmrCascade.Stop();
            CascadeSymbols();
            UpdateGridDisplay();
            CheckWinsAndCascades();
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
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}