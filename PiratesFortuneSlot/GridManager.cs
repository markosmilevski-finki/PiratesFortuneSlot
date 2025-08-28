using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PiratesFortuneSlot
{
    public class GridManager
    {
        public const int COLS = 5;
        public const int ROWS = 4;
        public SymbolType[,] grid = new SymbolType[ROWS, COLS];
        public PictureBox[,] pbGrid = new PictureBox[ROWS, COLS];
        public Point[,] gridCenters;
        public int[,] finalYPositions = new int[ROWS, COLS];
        private readonly Form _form;
        private readonly SymbolManager _symbolManager;

        public GridManager(Form form, SymbolManager symbolManager)
        {
            _form = form;
            _symbolManager = symbolManager;
            gridCenters = GetGridCenterCoordinates();
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

        public void InitializeGrid()
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

                    _form.Controls.Add(pbGrid[row, col]);
                }
            }
        }

        public void GenerateGrid(bool inBonus)
        {
            Random rnd = new Random();
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

        public void UpdateGridDisplay()
        {
            for (int row = 0; row < ROWS; row++)
            {
                for (int col = 0; col < COLS; col++)
                {
                    var sym = _symbolManager.Symbols.Find(s => s.Type == grid[row, col]);
                    pbGrid[row, col].Image = sym?.Image;
                    pbGrid[row, col].Location = new Point(
                        gridCenters[row, col].X - pbGrid[row, col].Width / 2,
                        gridCenters[row, col].Y - pbGrid[row, col].Height / 2
                    );
                }
            }
            _form.Invalidate();
        }

        public void AnimateDrop()
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

        public void AnimateCascadeDrop()
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

        public List<Point> GetCluster(int startRow, int startCol, bool[,] visited, List<Symbol> symbols)
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

        public double CalculatePayout(List<Point> cluster, List<Symbol> symbols)
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

        public void ExplodeCluster(List<Point> cluster)
        {
            foreach (var p in cluster)
            {
                grid[p.Y, p.X] = SymbolType.Empty;
                pbGrid[p.Y, p.X].Image = null;
                pbGrid[p.Y, p.X].BackColor = Color.Transparent;
            }
        }

        public void CascadeSymbols(Random rnd, bool inBonus)
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
    }
}