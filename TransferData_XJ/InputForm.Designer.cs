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
            this.ChooseFile = new System.Windows.Forms.Button();
            this.StartModel = new System.Windows.Forms.Button();
            this.WallTypeBox = new System.Windows.Forms.ListBox();
            this.LevelBox = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.PreviewCanvas = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // ChooseFile
            // 
            this.ChooseFile.Location = new System.Drawing.Point(842, 810);
            this.ChooseFile.Margin = new System.Windows.Forms.Padding(2);
            this.ChooseFile.Name = "ChooseFile";
            this.ChooseFile.Size = new System.Drawing.Size(227, 58);
            this.ChooseFile.TabIndex = 0;
            this.ChooseFile.Text = "打开json文件";
            this.ChooseFile.UseVisualStyleBackColor = true;
            this.ChooseFile.Click += new System.EventHandler(this.ChooseFile_Click);
            // 
            // StartModel
            // 
            this.StartModel.Location = new System.Drawing.Point(623, 810);
            this.StartModel.Margin = new System.Windows.Forms.Padding(2);
            this.StartModel.Name = "StartModel";
            this.StartModel.Size = new System.Drawing.Size(172, 58);
            this.StartModel.TabIndex = 1;
            this.StartModel.Text = "开始建模";
            this.StartModel.UseVisualStyleBackColor = true;
            this.StartModel.Click += new System.EventHandler(this.StartModel_Click);
            // 
            // WallTypeBox
            // 
            this.WallTypeBox.FormattingEnabled = true;
            this.WallTypeBox.ItemHeight = 30;
            this.WallTypeBox.Location = new System.Drawing.Point(54, 70);
            this.WallTypeBox.Margin = new System.Windows.Forms.Padding(8);
            this.WallTypeBox.Name = "WallTypeBox";
            this.WallTypeBox.Size = new System.Drawing.Size(374, 424);
            this.WallTypeBox.TabIndex = 2;
            this.WallTypeBox.SelectedIndexChanged += new System.EventHandler(this.WallTypeBox_SelectedIndexChanged);
            // 
            // LevelBox
            // 
            this.LevelBox.FormattingEnabled = true;
            this.LevelBox.ItemHeight = 30;
            this.LevelBox.Location = new System.Drawing.Point(54, 624);
            this.LevelBox.Margin = new System.Windows.Forms.Padding(8);
            this.LevelBox.Name = "LevelBox";
            this.LevelBox.Size = new System.Drawing.Size(374, 244);
            this.LevelBox.TabIndex = 3;
            this.LevelBox.SelectedIndexChanged += new System.EventHandler(this.LevelBox_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(49, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(223, 30);
            this.label1.TabIndex = 4;
            this.label1.Text = "选择基本墙类型";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(49, 586);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(148, 30);
            this.label2.TabIndex = 5;
            this.label2.Text = "选择0标高";
            // 
            // PreviewCanvas
            // 
            this.PreviewCanvas.BackColor = System.Drawing.Color.White;
            this.PreviewCanvas.Location = new System.Drawing.Point(482, 70);
            this.PreviewCanvas.Name = "PreviewCanvas";
            this.PreviewCanvas.Size = new System.Drawing.Size(586, 681);
            this.PreviewCanvas.TabIndex = 6;
            this.PreviewCanvas.Paint += new System.Windows.Forms.PaintEventHandler(this.PreviewCanvas_Paint);
            // 
            // InputForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(15F, 30F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1145, 925);
            this.Controls.Add(this.PreviewCanvas);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.LevelBox);
            this.Controls.Add(this.WallTypeBox);
            this.Controls.Add(this.StartModel);
            this.Controls.Add(this.ChooseFile);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "InputForm";
            this.Text = "InputForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button ChooseFile;
        private System.Windows.Forms.Button StartModel;
        public System.Windows.Forms.ListBox WallTypeBox;
        public System.Windows.Forms.ListBox LevelBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel PreviewCanvas;
    }
}