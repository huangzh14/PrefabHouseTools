namespace InteriorAutoPanel
{
    partial class InputForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.Start = new System.Windows.Forms.Button();
            this.Cancel = new System.Windows.Forms.Button();
            this.DistanceToWall = new System.Windows.Forms.NumericUpDown();
            this.UnitWidth = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.DistanceToWall)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.UnitWidth)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(36, 73);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(197, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "Distance from Panel to Wall (mm)";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(36, 136);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(149, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "Standard Unit Width (mm)";
            // 
            // Start
            // 
            this.Start.Location = new System.Drawing.Point(183, 191);
            this.Start.Name = "Start";
            this.Start.Size = new System.Drawing.Size(75, 23);
            this.Start.TabIndex = 4;
            this.Start.Text = "Start";
            this.Start.UseVisualStyleBackColor = true;
            this.Start.Click += new System.EventHandler(this.Start_Click);
            // 
            // Cancel
            // 
            this.Cancel.Location = new System.Drawing.Point(279, 191);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 23);
            this.Cancel.TabIndex = 5;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // DistanceToWall
            // 
            this.DistanceToWall.Location = new System.Drawing.Point(254, 71);
            this.DistanceToWall.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.DistanceToWall.Name = "DistanceToWall";
            this.DistanceToWall.Size = new System.Drawing.Size(79, 21);
            this.DistanceToWall.TabIndex = 6;
            this.DistanceToWall.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // UnitWidth
            // 
            this.UnitWidth.Location = new System.Drawing.Point(254, 134);
            this.UnitWidth.Maximum = new decimal(new int[] {
            3000,
            0,
            0,
            0});
            this.UnitWidth.Name = "UnitWidth";
            this.UnitWidth.Size = new System.Drawing.Size(79, 21);
            this.UnitWidth.TabIndex = 7;
            this.UnitWidth.Value = new decimal(new int[] {
            600,
            0,
            0,
            0});
            // 
            // InputForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 256);
            this.Controls.Add(this.UnitWidth);
            this.Controls.Add(this.DistanceToWall);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.Start);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "InputForm";
            this.Text = "Input";
            this.Load += new System.EventHandler(this.Input_Load);
            ((System.ComponentModel.ISupportInitialize)(this.DistanceToWall)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.UnitWidth)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button Start;
        private System.Windows.Forms.Button Cancel;
        public System.Windows.Forms.NumericUpDown DistanceToWall;
        public System.Windows.Forms.NumericUpDown UnitWidth;
    }
}