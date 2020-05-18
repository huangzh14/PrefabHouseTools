namespace PrefabHouseTools
{
    partial class CmdReadJsonForm
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
            this.LevelBox = new System.Windows.Forms.ListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label4 = new System.Windows.Forms.Label();
            this.basicLayoutCheckbox = new System.Windows.Forms.CheckBox();
            this.waterTerminalCheckbox = new System.Windows.Forms.CheckBox();
            this.socketCheckbox = new System.Windows.Forms.CheckBox();
            this.lightingCheckbox = new System.Windows.Forms.CheckBox();
            this.furnitureCheckbox = new System.Windows.Forms.CheckBox();
            this.StartModel = new System.Windows.Forms.Button();
            this.PreviewCanvas = new System.Windows.Forms.Panel();
            this.prograssLabel = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.label1 = new System.Windows.Forms.Label();
            this.ChooseFile = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // LevelBox
            // 
            this.LevelBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LevelBox.FormattingEnabled = true;
            this.LevelBox.ItemHeight = 30;
            this.LevelBox.Location = new System.Drawing.Point(8, 408);
            this.LevelBox.Margin = new System.Windows.Forms.Padding(8);
            this.LevelBox.Name = "LevelBox";
            this.LevelBox.Size = new System.Drawing.Size(257, 454);
            this.LevelBox.TabIndex = 3;
            this.LevelBox.SelectedIndexChanged += new System.EventHandler(this.LevelBox_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 350);
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
            this.panel1.Size = new System.Drawing.Size(0, 897);
            this.panel1.TabIndex = 8;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.label4, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.basicLayoutCheckbox, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.waterTerminalCheckbox, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.socketCheckbox, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.lightingCheckbox, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.furnitureCheckbox, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.LevelBox, 0, 8);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 7);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 9);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 9;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(273, 876);
            this.tableLayoutPanel1.TabIndex = 10;
            this.tableLayoutPanel1.Paint += new System.Windows.Forms.PaintEventHandler(this.tableLayoutPanel1_Paint);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(133, 30);
            this.label4.TabIndex = 7;
            this.label4.Text = "执行模块";
            // 
            // basicLayoutCheckbox
            // 
            this.basicLayoutCheckbox.AutoSize = true;
            this.basicLayoutCheckbox.Checked = true;
            this.basicLayoutCheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.basicLayoutCheckbox.Location = new System.Drawing.Point(3, 53);
            this.basicLayoutCheckbox.Name = "basicLayoutCheckbox";
            this.basicLayoutCheckbox.Size = new System.Drawing.Size(261, 34);
            this.basicLayoutCheckbox.TabIndex = 8;
            this.basicLayoutCheckbox.Text = "基本户型和门窗";
            this.basicLayoutCheckbox.UseVisualStyleBackColor = true;
            // 
            // waterTerminalCheckbox
            // 
            this.waterTerminalCheckbox.AutoSize = true;
            this.waterTerminalCheckbox.Location = new System.Drawing.Point(3, 103);
            this.waterTerminalCheckbox.Name = "waterTerminalCheckbox";
            this.waterTerminalCheckbox.Size = new System.Drawing.Size(201, 34);
            this.waterTerminalCheckbox.TabIndex = 9;
            this.waterTerminalCheckbox.Text = "给排水末端";
            this.waterTerminalCheckbox.UseVisualStyleBackColor = true;
            // 
            // socketCheckbox
            // 
            this.socketCheckbox.AutoSize = true;
            this.socketCheckbox.Location = new System.Drawing.Point(3, 153);
            this.socketCheckbox.Name = "socketCheckbox";
            this.socketCheckbox.Size = new System.Drawing.Size(171, 34);
            this.socketCheckbox.TabIndex = 10;
            this.socketCheckbox.Text = "插座末端";
            this.socketCheckbox.UseVisualStyleBackColor = true;
            // 
            // lightingCheckbox
            // 
            this.lightingCheckbox.AutoSize = true;
            this.lightingCheckbox.Location = new System.Drawing.Point(3, 203);
            this.lightingCheckbox.Name = "lightingCheckbox";
            this.lightingCheckbox.Size = new System.Drawing.Size(171, 34);
            this.lightingCheckbox.TabIndex = 11;
            this.lightingCheckbox.Text = "照明末端";
            this.lightingCheckbox.UseVisualStyleBackColor = true;
            // 
            // furnitureCheckbox
            // 
            this.furnitureCheckbox.AutoSize = true;
            this.furnitureCheckbox.Location = new System.Drawing.Point(3, 253);
            this.furnitureCheckbox.Name = "furnitureCheckbox";
            this.furnitureCheckbox.Size = new System.Drawing.Size(111, 34);
            this.furnitureCheckbox.TabIndex = 12;
            this.furnitureCheckbox.Text = "家具";
            this.furnitureCheckbox.UseVisualStyleBackColor = true;
            // 
            // StartModel
            // 
            this.StartModel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.StartModel.Location = new System.Drawing.Point(285, 788);
            this.StartModel.Margin = new System.Windows.Forms.Padding(2);
            this.StartModel.Name = "StartModel";
            this.StartModel.Size = new System.Drawing.Size(157, 74);
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
            this.PreviewCanvas.Size = new System.Drawing.Size(663, 701);
            this.PreviewCanvas.TabIndex = 6;
            this.PreviewCanvas.Paint += new System.Windows.Forms.PaintEventHandler(this.PreviewCanvas_Paint);
            // 
            // prograssLabel
            // 
            this.prograssLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.prograssLabel.AutoSize = true;
            this.prograssLabel.Location = new System.Drawing.Point(155, 835);
            this.prograssLabel.Name = "prograssLabel";
            this.prograssLabel.Size = new System.Drawing.Size(28, 30);
            this.prograssLabel.TabIndex = 10;
            this.prograssLabel.Text = "0";
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
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(11, 784);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(269, 40);
            this.progressBar1.TabIndex = 8;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 835);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(223, 30);
            this.label1.TabIndex = 9;
            this.label1.Text = "建模进度：   %";
            // 
            // ChooseFile
            // 
            this.ChooseFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ChooseFile.Location = new System.Drawing.Point(463, 788);
            this.ChooseFile.Margin = new System.Windows.Forms.Padding(2);
            this.ChooseFile.Name = "ChooseFile";
            this.ChooseFile.Size = new System.Drawing.Size(211, 74);
            this.ChooseFile.TabIndex = 0;
            this.ChooseFile.Text = "打开json文件";
            this.ChooseFile.UseVisualStyleBackColor = true;
            this.ChooseFile.Click += new System.EventHandler(this.ChooseFile_Click);
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.Controls.Add(this.prograssLabel);
            this.panel2.Controls.Add(this.ChooseFile);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Controls.Add(this.progressBar1);
            this.panel2.Controls.Add(this.label3);
            this.panel2.Controls.Add(this.PreviewCanvas);
            this.panel2.Controls.Add(this.StartModel);
            this.panel2.Location = new System.Drawing.Point(282, 4);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(682, 882);
            this.panel2.TabIndex = 9;
            // 
            // CmdReadJsonForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(15F, 30F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(968, 897);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MinimumSize = new System.Drawing.Size(1000, 500);
            this.Name = "CmdReadJsonForm";
            this.Text = "模型转换";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CmdReadJsonForm_FormClosing);
            this.SizeChanged += new System.EventHandler(this.CmdReadJsonForm_SizeChanged);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        public System.Windows.Forms.ListBox LevelBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button StartModel;
        private System.Windows.Forms.Panel PreviewCanvas;
        private System.Windows.Forms.Label prograssLabel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button ChooseFile;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox basicLayoutCheckbox;
        private System.Windows.Forms.CheckBox waterTerminalCheckbox;
        private System.Windows.Forms.CheckBox socketCheckbox;
        private System.Windows.Forms.CheckBox lightingCheckbox;
        private System.Windows.Forms.CheckBox furnitureCheckbox;
    }
}