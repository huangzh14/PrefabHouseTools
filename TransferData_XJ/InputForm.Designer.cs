namespace TransferData_XJ
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
            this.WallTypeBox = new System.Windows.Forms.ListBox();
            this.LevelBox = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.ChooseFile = new System.Windows.Forms.Button();
            this.StartModel = new System.Windows.Forms.Button();
            this.PreviewCanvas = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel2.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // WallTypeBox
            // 
            this.WallTypeBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.WallTypeBox.FormattingEnabled = true;
            this.WallTypeBox.ItemHeight = 30;
            this.WallTypeBox.Location = new System.Drawing.Point(8, 58);
            this.WallTypeBox.Margin = new System.Windows.Forms.Padding(8);
            this.WallTypeBox.Name = "WallTypeBox";
            this.WallTypeBox.Size = new System.Drawing.Size(326, 454);
            this.WallTypeBox.TabIndex = 2;
            this.WallTypeBox.SelectedIndexChanged += new System.EventHandler(this.WallTypeBox_SelectedIndexChanged);
            // 
            // LevelBox
            // 
            this.LevelBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LevelBox.FormattingEnabled = true;
            this.LevelBox.ItemHeight = 30;
            this.LevelBox.Location = new System.Drawing.Point(8, 582);
            this.LevelBox.Margin = new System.Windows.Forms.Padding(8);
            this.LevelBox.Name = "LevelBox";
            this.LevelBox.Size = new System.Drawing.Size(326, 274);
            this.LevelBox.TabIndex = 3;
            this.LevelBox.SelectedIndexChanged += new System.EventHandler(this.LevelBox_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(223, 30);
            this.label1.TabIndex = 4;
            this.label1.Text = "选择基本墙类型";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 524);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(148, 30);
            this.label2.TabIndex = 5;
            this.label2.Text = "选择0标高";
            // 
            // panel1
            // 
            this.panel1.AutoSize = true;
            this.panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(0, 912);
            this.panel1.TabIndex = 8;
            // 
            // ChooseFile
            // 
            this.ChooseFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ChooseFile.Location = new System.Drawing.Point(594, 807);
            this.ChooseFile.Margin = new System.Windows.Forms.Padding(2);
            this.ChooseFile.Name = "ChooseFile";
            this.ChooseFile.Size = new System.Drawing.Size(211, 58);
            this.ChooseFile.TabIndex = 0;
            this.ChooseFile.Text = "打开json文件";
            this.ChooseFile.UseVisualStyleBackColor = true;
            this.ChooseFile.Click += new System.EventHandler(this.ChooseFile_Click);
            // 
            // StartModel
            // 
            this.StartModel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.StartModel.Location = new System.Drawing.Point(411, 803);
            this.StartModel.Margin = new System.Windows.Forms.Padding(2);
            this.StartModel.Name = "StartModel";
            this.StartModel.Size = new System.Drawing.Size(157, 58);
            this.StartModel.TabIndex = 1;
            this.StartModel.Text = "开始建模";
            this.StartModel.UseVisualStyleBackColor = true;
            this.StartModel.Click += new System.EventHandler(this.StartModel_Click);
            // 
            // PreviewCanvas
            // 
            this.PreviewCanvas.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PreviewCanvas.BackColor = System.Drawing.Color.White;
            this.PreviewCanvas.Location = new System.Drawing.Point(11, 62);
            this.PreviewCanvas.Name = "PreviewCanvas";
            this.PreviewCanvas.Size = new System.Drawing.Size(794, 716);
            this.PreviewCanvas.TabIndex = 6;
            this.PreviewCanvas.Paint += new System.Windows.Forms.PaintEventHandler(this.PreviewCanvas_Paint);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 12);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(133, 30);
            this.label3.TabIndex = 7;
            this.label3.Text = "户型预览";
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.Controls.Add(this.label3);
            this.panel2.Controls.Add(this.PreviewCanvas);
            this.panel2.Controls.Add(this.StartModel);
            this.panel2.Controls.Add(this.ChooseFile);
            this.panel2.Location = new System.Drawing.Point(351, 4);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(813, 897);
            this.panel2.TabIndex = 9;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.LevelBox, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.WallTypeBox, 0, 1);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 9);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(342, 891);
            this.tableLayoutPanel1.TabIndex = 10;
            // 
            // InputForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(15F, 30F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1168, 912);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MinimumSize = new System.Drawing.Size(800, 400);
            this.Name = "InputForm";
            this.Text = "InputForm";
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        public System.Windows.Forms.ListBox WallTypeBox;
        public System.Windows.Forms.ListBox LevelBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button ChooseFile;
        private System.Windows.Forms.Button StartModel;
        private System.Windows.Forms.Panel PreviewCanvas;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}