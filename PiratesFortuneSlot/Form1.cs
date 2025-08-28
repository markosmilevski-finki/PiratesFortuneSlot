using System;
using System.Collections.Generic;
using System.Drawing;
using System.Media;
using System.Windows.Forms;
using System.IO;
using NAudio.Wave;

namespace PiratesFortuneSlot
{
    public partial class Form1 : Form
    {
        private readonly SymbolManager _symbolManager;
        private readonly GridManager _gridManager;
        private readonly UIManager _uiManager;
        private readonly AudioManager _audioManager;

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
        private int goldCoinRequirement = 5;
        private bool isAutoSpinning = false;
        private bool isSpinComplete = true;
        private bool bonusTriggeredThisSpin = false;
        private bool goldCoinsCountedThisSpin = false;
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

            _symbolManager = new SymbolManager();
            _gridManager = new GridManager(this, _symbolManager);
            _uiManager = new UIManager(this);
            _audioManager = new AudioManager();

            _uiManager.InitializeUI();
            _uiManager.btnAutoSpin.Click += btnAutoSpin_Click;
            _uiManager.tmrWinDisplay.Tick += tmrWinDisplay_Tick;
            _uiManager.tmrBaseWinDisplay.Tick += tmrBaseWinDisplay_Tick;
            _uiManager.tmrAutoSpin.Tick += tmrAutoSpin_Tick;
            _uiManager.tmrExplode.Tick += tmrExplode_Tick;
            _uiManager.tmrCascadeDrop.Tick += tmrCascadeDrop_Tick;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Size = new Size(1280, 720);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackgroundImageLayout = ImageLayout.Stretch;

            _uiManager.UpdateBackgroundImage(inBonus);

            lblBalance.Location = new Point(25, 630);
            lblBalance.Font = new Font("Arial", 12, FontStyle.Bold);
            lblBalance.ForeColor = Color.White;
            lblBalance.BackColor = Color.Transparent;
            nudBet.Location = new Point(850, 600);
            nudBet.Font = new Font("Arial", 12);
            nudBet.Size = new Size(80, 25);
            nudBet.DecimalPlaces = 2;
            nudBet.Minimum = 0.10m;
            nudBet.Maximum = 1000.00m;
            nudBet.Value = 10.00m;
            nudBet.Increment = 0.10m;
            btnSpin.Location = new Point(1000, 600);
            btnSpin.Font = new Font("Arial", 12);
            btnSpin.Size = new Size(80, 30);

            tmrDrop.Interval = 50;
            tmrCascade.Interval = 500;

            _audioManager.LoadSounds();
            _symbolManager.InitializeSymbols();
            if (_symbolManager.Symbols.Count == 0)
            {
                MessageBox.Show("No symbols loaded. Check resource files.");
                Close();
                return;
            }

            _gridManager.InitializeGrid();
            _gridManager.GenerateGrid(inBonus);
            _gridManager.UpdateGridDisplay();
            _uiManager.UpdateUI(balance, bet, bonusSpins, bonusMultiplier, collectedTreasures, goldCoinRequirement, biggestWin, biggestMultiplier, inBonus, isSpinComplete, isAutoSpinning);
            _audioManager.PlayBackgroundMusic("BackgroundMusic.wav");
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
                    _uiManager.btnAutoSpin.Text = "Auto Spin";
                    _uiManager.tmrAutoSpin.Stop();
                    _uiManager.UpdateUI(balance, bet, bonusSpins, bonusMultiplier, collectedTreasures, goldCoinRequirement, biggestWin, biggestMultiplier, inBonus, isSpinComplete, isAutoSpinning);
                    return;
                }
                balance -= bet;
                totalSpinWin = 0.00;
                bonusTriggeredThisSpin = false;
            }
            currentWin = 0.00;
            if (!inBonus) bonusMultiplier = 1;
            isSpinComplete = false;
            goldCoinsCountedThisSpin = false;
            bonusTriggeredThisSpin = false; // Reset to allow retrigger in bonus mode
            _audioManager.PlaySound("Spin.wav");
            _gridManager.GenerateGrid(inBonus);
            _gridManager.AnimateDrop();
            tmrDrop.Start();
            _uiManager.UpdateUI(balance, bet, bonusSpins, bonusMultiplier, collectedTreasures, goldCoinRequirement, biggestWin, biggestMultiplier, inBonus, isSpinComplete, isAutoSpinning);
        }

        private void btnAutoSpin_Click(object sender, EventArgs e)
        {
            if (inBonus)
            {
                MessageBox.Show("Auto-spin disabled during bonus game!");
                return;
            }
            isAutoSpinning = !isAutoSpinning;
            _uiManager.btnAutoSpin.Text = isAutoSpinning ? "Stop Auto" : "Auto Spin";
            if (isAutoSpinning)
            {
                if (balance < (double)nudBet.Value)
                {
                    MessageBox.Show("Insufficient balance to start auto-spin!");
                    isAutoSpinning = false;
                    _uiManager.btnAutoSpin.Text = "Auto Spin";
                    return;
                }
                _uiManager.tmrAutoSpin.Start();
                if (isSpinComplete)
                    btnSpin_Click(sender, e);
            }
            else
            {
                _uiManager.tmrAutoSpin.Stop();
            }
            _uiManager.UpdateUI(balance, bet, bonusSpins, bonusMultiplier, collectedTreasures, goldCoinRequirement, biggestWin, biggestMultiplier, inBonus, isSpinComplete, isAutoSpinning);
        }

        private void tmrAutoSpin_Tick(object sender, EventArgs e)
        {
            if (!isAutoSpinning || inBonus || balance < (double)nudBet.Value || !isSpinComplete)
            {
                if (!isAutoSpinning || inBonus || balance < (double)nudBet.Value)
                {
                    isAutoSpinning = false;
                    _uiManager.btnAutoSpin.Text = "Auto Spin";
                    _uiManager.tmrAutoSpin.Stop();
                    _uiManager.UpdateUI(balance, bet, bonusSpins, bonusMultiplier, collectedTreasures, goldCoinRequirement, biggestWin, biggestMultiplier, inBonus, isSpinComplete, isAutoSpinning);
                    if (balance < (double)nudBet.Value)
                    {
                        MessageBox.Show("Insufficient balance to continue auto-spin!");
                    }
                }
                return;
            }
            btnSpin_Click(sender, e);
        }

        private void tmrDrop_Tick(object sender, EventArgs e)
        {
            const int dropSpeed = 60;
            bool allDropped = true;

            for (int row = 0; row < GridManager.ROWS; row++)
            {
                for (int col = 0; col < GridManager.COLS; col++)
                {
                    int targetY = _gridManager.finalYPositions[row, col];
                    if (_gridManager.pbGrid[row, col].Top < targetY)
                    {
                        int newTop = _gridManager.pbGrid[row, col].Top + dropSpeed;
                        if (newTop > targetY) newTop = targetY;
                        _gridManager.pbGrid[row, col].Location = new Point(_gridManager.pbGrid[row, col].Left, newTop);
                        if (newTop == targetY)
                        {
                            var sym = _symbolManager.Symbols.Find(s => s.Type == _gridManager.grid[row, col]);
                            _gridManager.pbGrid[row, col].Image = sym?.Image;
                            if (_gridManager.grid[row, col] == SymbolType.GoldCoin)
                            {
                                _audioManager.PlaySound("GoldCoin.wav");
                            }
                            else if (_gridManager.grid[row, col] == SymbolType.Scatter)
                            {
                                _audioManager.PlaySound("Win.wav");
                            }
                        }
                        allDropped = false;
                    }
                }
            }

            if (allDropped)
            {
                tmrDrop.Stop();
                _gridManager.UpdateGridDisplay();
                CheckWinsAndCascades();
            }
            this.Invalidate();
        }

        private void tmrCascadeDrop_Tick(object sender, EventArgs e)
        {
            const int dropSpeed = 60;
            bool allDropped = true;

            for (int row = 0; row < GridManager.ROWS; row++)
            {
                for (int col = 0; col < GridManager.COLS; col++)
                {
                    int targetY = _gridManager.finalYPositions[row, col];
                    if (_gridManager.pbGrid[row, col].Top < targetY)
                    {
                        int newTop = _gridManager.pbGrid[row, col].Top + dropSpeed;
                        if (newTop > targetY) newTop = targetY;
                        _gridManager.pbGrid[row, col].Location = new Point(_gridManager.pbGrid[row, col].Left, newTop);
                        if (newTop == targetY)
                        {
                            var sym = _symbolManager.Symbols.Find(s => s.Type == _gridManager.grid[row, col]);
                            _gridManager.pbGrid[row, col].Image = sym?.Image;
                            if (_gridManager.grid[row, col] == SymbolType.GoldCoin)
                            {
                                _audioManager.PlaySound("GoldCoin.wav");
                            }
                            else if (_gridManager.grid[row, col] == SymbolType.Scatter)
                            {
                                _audioManager.PlaySound("Win.wav");
                            }
                        }
                        allDropped = false;
                    }
                }
            }

            if (allDropped)
            {
                _uiManager.tmrCascadeDrop.Stop();
                isCascading = false;
                _gridManager.UpdateGridDisplay();
                var tmrDelay = new Timer { Interval = 500 };
                tmrDelay.Tick += (s, ev) =>
                {
                    tmrDelay.Stop();
                    CheckWinsAndCascades();
                    tmrDelay.Dispose();
                };
                tmrDelay.Start();
            }
            this.Invalidate();
        }

        private void tmrExplode_Tick(object sender, EventArgs e)
        {
            _uiManager.tmrExplode.Stop();
            if (currentExplodingCluster != null)
            {
                foreach (var p in currentExplodingCluster)
                {
                    _gridManager.pbGrid[p.Y, p.X].BackColor = Color.FromArgb(255, 255, 100, 100);
                }
                this.Invalidate();
                if (!inBonus && goldCoins >= 2)
                {
                    currentWin *= goldCoins;
                    _uiManager.lblWin.Location = new Point(350, 550);
                    _uiManager.lblWin.Text = $"Base Pay: {baseWin:N2} x{goldCoins} \n Gold Coin Multiplier";
                    _uiManager.lblWin.Visible = true;
                    _uiManager.tmrBaseWinDisplay.Start();
                }
                else
                {
                    _uiManager.lblWin.Location = new Point(450, 600);
                    _uiManager.lblWin.Text = $"Win: {currentWin:N2}";
                    _uiManager.lblWin.Visible = true;
                    _uiManager.tmrWinDisplay.Start();
                }
                _audioManager.PlaySound("Explosion.wav");
                _audioManager.PlaySound("Win.wav");

                var tmrClear = new Timer { Interval = 300 };
                tmrClear.Tick += (s, ev) =>
                {
                    tmrClear.Stop();
                    _gridManager.ExplodeCluster(currentExplodingCluster);
                    currentExplodingCluster = null;
                    isExploding = false;
                    _gridManager.CascadeSymbols(rnd, inBonus);
                    _gridManager.AnimateCascadeDrop();
                    isCascading = true;
                    _uiManager.tmrCascadeDrop.Start();
                    tmrClear.Dispose();
                };
                tmrClear.Start();
            }
            else
            {
                isExploding = false;
                _gridManager.CascadeSymbols(rnd, inBonus);
                _gridManager.AnimateCascadeDrop();
                isCascading = true;
                _uiManager.tmrCascadeDrop.Start();
            }
        }

        private void tmrBaseWinDisplay_Tick(object sender, EventArgs e)
        {
            _uiManager.tmrBaseWinDisplay.Stop();
            _uiManager.lblWin.Text = $"Win: {totalSpinWin:N2}";
            _uiManager.tmrWinDisplay.Start();
        }

        private void tmrWinDisplay_Tick(object sender, EventArgs e)
        {
            _uiManager.tmrWinDisplay.Stop();
            _uiManager.lblWin.Visible = false;
        }

        private void CheckWinsAndCascades()
        {
            if (isExploding || isCascading) return;

            bool hasWin = false;
            double totalPayout = 0;
            int scatterCount = 0;
            int goldCoinCount = 0;

            bool[,] visited = new bool[GridManager.ROWS, GridManager.COLS];
            List<List<Point>> winningClusters = new List<List<Point>>();

            for (int row = 0; row < GridManager.ROWS; row++)
            {
                for (int col = 0; col < GridManager.COLS; col++)
                {
                    if (_gridManager.grid[row, col] == SymbolType.Scatter) scatterCount++;
                    if (_gridManager.grid[row, col] == SymbolType.GoldCoin) goldCoinCount++;
                    if (!visited[row, col] && _gridManager.grid[row, col] != SymbolType.Empty && _gridManager.grid[row, col] != SymbolType.Scatter && _gridManager.grid[row, col] != SymbolType.GoldCoin)
                    {
                        List<Point> cluster = _gridManager.GetCluster(row, col, visited, _symbolManager.Symbols);
                        if (cluster.Count >= 4)
                        {
                            hasWin = true;
                            winningClusters.Add(cluster);
                            double payout = _gridManager.CalculatePayout(cluster, _symbolManager.Symbols);
                            totalPayout += payout;
                        }
                    }
                }
            }

            if (inBonus && goldCoinCount > 0 && !goldCoinsCountedThisSpin)
            {
                collectedTreasures += goldCoinCount;
                goldCoinsCountedThisSpin = true;
                if (collectedTreasures >= goldCoinRequirement && extraSpinsAdded < 11)
                {
                    bonusMultiplier++;
                    bonusSpins += 2;
                    extraSpinsAdded += 1;
                    collectedTreasures = 0;
                    goldCoinRequirement = Math.Min(goldCoinRequirement * 2, 10);
                    biggestMultiplier = Math.Max(biggestMultiplier, bonusMultiplier);
                    MessageBox.Show("Coins collected!");
                    MessageBox.Show("+1 Multiplier, +2 Spins Added!");
                    _uiManager.UpdateUI(balance, bet, bonusSpins, bonusMultiplier, collectedTreasures, goldCoinRequirement, biggestWin, biggestMultiplier, inBonus, isSpinComplete, isAutoSpinning);
                }
            }

            if (hasWin)
            {
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
                _uiManager.UpdateUI(balance, bet, bonusSpins, bonusMultiplier, collectedTreasures, goldCoinRequirement, biggestWin, biggestMultiplier, inBonus, isSpinComplete, isAutoSpinning);

                if (winningClusters.Count > 0)
                {
                    currentExplodingCluster = winningClusters[0];
                    isExploding = true;
                    _uiManager.tmrExplode.Start();
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
                    _audioManager.PlaySound("BonusPirate.wav");
                    MessageBox.Show("Bonus Retriggered! +10 Spins!");
                    _uiManager.UpdateUI(balance, bet, bonusSpins, bonusMultiplier, collectedTreasures, goldCoinRequirement, biggestWin, biggestMultiplier, inBonus, isSpinComplete, isAutoSpinning);
                    if (!isCascading && !isExploding && bonusSpins > 0)
                    {
                        _gridManager.UpdateGridDisplay();
                        bonusSpins--;
                        btnSpin_Click(null, null);
                    }
                }
                else if (inBonus)
                {
                    bool needsCascade = false;
                    for (int row = 0; row < GridManager.ROWS; row++)
                    {
                        for (int col = 0; col < GridManager.COLS; col++)
                        {
                            if (_gridManager.grid[row, col] == SymbolType.Empty)
                            {
                                needsCascade = true;
                                break;
                            }
                        }
                        if (needsCascade) break;
                    }

                    if (needsCascade)
                    {
                        _gridManager.CascadeSymbols(rnd, inBonus);
                        _gridManager.AnimateCascadeDrop();
                        isCascading = true;
                        _uiManager.tmrCascadeDrop.Start();
                    }
                    else if (bonusSpins > 0)
                    {
                        _gridManager.UpdateGridDisplay();
                        bonusSpins--;
                        btnSpin_Click(null, null);
                    }
                    else
                    {
                        _audioManager.PlaySound("Win.wav");
                        _audioManager.PlaySound("PirateTalk.wav");
                        inBonus = false;
                        extraSpinsAdded = 0;
                        goldCoinRequirement = 5;
                        collectedTreasures = 0;
                        isAutoSpinning = false;
                        _uiManager.btnAutoSpin.Text = "Auto Spin";
                        _uiManager.tmrAutoSpin.Stop();
                        balance += totalSpinWin;
                        MessageBox.Show($"Bonus over! Total win: {totalSpinWin:N2}");
                        totalSpinWin = 0;
                        _uiManager.lblWin.Visible = false;
                        isSpinComplete = true;
                        bonusTriggeredThisSpin = false;
                        goldCoinsCountedThisSpin = false;
                        _audioManager.PlayBackgroundMusic("BackgroundMusic.wav");
                        _uiManager.UpdateBackgroundImage(inBonus);
                        _uiManager.UpdateUI(balance, bet, bonusSpins, bonusMultiplier, collectedTreasures, goldCoinRequirement, biggestWin, biggestMultiplier, inBonus, isSpinComplete, isAutoSpinning);
                    }
                }
                else
                {
                    bool needsCascade = false;
                    for (int row = 0; row < GridManager.ROWS; row++)
                    {
                        for (int col = 0; col < GridManager.COLS; col++)
                        {
                            if (_gridManager.grid[row, col] == SymbolType.Empty)
                            {
                                needsCascade = true;
                                break;
                            }
                        }
                        if (needsCascade) break;
                    }
                    if (needsCascade)
                    {
                        _gridManager.CascadeSymbols(rnd, inBonus);
                        _gridManager.AnimateCascadeDrop();
                        isCascading = true;
                        _uiManager.tmrCascadeDrop.Start();
                    }
                    else
                    {
                        balance += totalSpinWin;
                        totalSpinWin = 0;
                        _uiManager.lblWin.Visible = false;
                        isSpinComplete = true;
                        bonusTriggeredThisSpin = false;
                        goldCoinsCountedThisSpin = false;
                        _uiManager.UpdateUI(balance, bet, bonusSpins, bonusMultiplier, collectedTreasures, goldCoinRequirement, biggestWin, biggestMultiplier, inBonus, isSpinComplete, isAutoSpinning);
                    }
                }
            }
        }

        private void EnterBonus(int scatters)
        {
            _audioManager.PlaySound("Win.wav");
            _audioManager.PlaySound("BonusPirate.wav");
            inBonus = true;
            bonusSpins = 10 + (scatters > 3 ? 5 * (scatters - 3) : 0);
            bonusMultiplier = 1;
            collectedTreasures = 0;
            extraSpinsAdded = 0;
            goldCoinRequirement = 5;
            isAutoSpinning = false;
            _uiManager.btnAutoSpin.Text = "Auto Spin";
            _uiManager.tmrAutoSpin.Stop();
            MessageBox.Show("Ahoy! Treasure Hunt Bonus Activated!");
            _audioManager.PlayBackgroundMusic("Stormy.wav");
            _uiManager.UpdateBackgroundImage(inBonus);
            btnSpin_Click(null, null);
        }

        private void tmrCascade_Tick(object sender, EventArgs e)
        {
            tmrCascade.Stop();
            _gridManager.CascadeSymbols(rnd, inBonus);
            _gridManager.AnimateCascadeDrop();
            isCascading = true;
            _uiManager.tmrCascadeDrop.Start();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            closingForm = true;
            _audioManager.Dispose();
            if (this.BackgroundImage != null)
            {
                this.BackgroundImage.Dispose();
            }
        }
    }
}