namespace PiratesFortuneSlot
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.lblBalance = new System.Windows.Forms.Label();
            this.lblWin = new System.Windows.Forms.Label();
            this.nudBet = new System.Windows.Forms.NumericUpDown();
            this.btnSpin = new System.Windows.Forms.Button();
            this.tmrDrop = new System.Windows.Forms.Timer(this.components);
            this.tmrCascade = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.nudBet)).BeginInit();
            this.SuspendLayout();
            // 
            // lblBalance
            // 
            this.lblBalance.AutoSize = true;
            this.lblBalance.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBalance.Location = new System.Drawing.Point(42, 1064);
            this.lblBalance.Name = "lblBalance";
            this.lblBalance.Size = new System.Drawing.Size(60, 25);
            this.lblBalance.TabIndex = 0;
            this.lblBalance.Text = "1000";
            // 
            // lblWin
            // 
            this.lblWin.AutoSize = true;
            this.lblWin.Location = new System.Drawing.Point(144, 1064);
            this.lblWin.Name = "lblWin";
            this.lblWin.Size = new System.Drawing.Size(24, 25);
            this.lblWin.TabIndex = 1;
            this.lblWin.Text = "0";
            // 
            // nudBet
            // 
            this.nudBet.Location = new System.Drawing.Point(212, 1064);
            this.nudBet.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nudBet.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudBet.Name = "nudBet";
            this.nudBet.Size = new System.Drawing.Size(120, 31);
            this.nudBet.TabIndex = 2;
            this.nudBet.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // btnSpin
            // 
            this.btnSpin.Location = new System.Drawing.Point(1632, 1017);
            this.btnSpin.Name = "btnSpin";
            this.btnSpin.Size = new System.Drawing.Size(250, 100);
            this.btnSpin.TabIndex = 3;
            this.btnSpin.Text = "Spin";
            this.btnSpin.UseVisualStyleBackColor = true;
            this.btnSpin.Click += new System.EventHandler(this.btnSpin_Click);
            // 
            // tmrDrop
            // 
            this.tmrDrop.Interval = 16;
            this.tmrDrop.Tick += new System.EventHandler(this.tmrDrop_Tick);
            // 
            // tmrCascade
            // 
            this.tmrCascade.Interval = 300;
            this.tmrCascade.Tick += new System.EventHandler(this.tmrCascade_Tick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(1894, 1129);
            this.Controls.Add(this.btnSpin);
            this.Controls.Add(this.nudBet);
            this.Controls.Add(this.lblWin);
            this.Controls.Add(this.lblBalance);
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nudBet)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        public System.Windows.Forms.Label lblBalance;
        public System.Windows.Forms.Label lblWin;
        public System.Windows.Forms.NumericUpDown nudBet;
        public System.Windows.Forms.Button btnSpin;
        private System.Windows.Forms.Timer tmrDrop;
        private System.Windows.Forms.Timer tmrCascade;
    }
}