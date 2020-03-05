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
            this.SuspendLayout();
            // 
            // ChooseFile
            // 
            this.ChooseFile.Location = new System.Drawing.Point(580, 362);
            this.ChooseFile.Margin = new System.Windows.Forms.Padding(1);
            this.ChooseFile.Name = "ChooseFile";
            this.ChooseFile.Size = new System.Drawing.Size(69, 23);
            this.ChooseFile.TabIndex = 0;
            this.ChooseFile.Text = "选择文件";
            this.ChooseFile.UseVisualStyleBackColor = true;
            this.ChooseFile.Click += new System.EventHandler(this.ChooseFile_Click);
            // 
            // StartModel
            // 
            this.StartModel.Location = new System.Drawing.Point(489, 362);
            this.StartModel.Margin = new System.Windows.Forms.Padding(1);
            this.StartModel.Name = "StartModel";
            this.StartModel.Size = new System.Drawing.Size(69, 23);
            this.StartModel.TabIndex = 1;
            this.StartModel.Text = "开始建模";
            this.StartModel.UseVisualStyleBackColor = true;
            this.StartModel.Click += new System.EventHandler(this.StartModel_Click);
            // 
            // WallTypeBox
            // 
            this.WallTypeBox.FormattingEnabled = true;
            this.WallTypeBox.ItemHeight = 12;
            this.WallTypeBox.Location = new System.Drawing.Point(50, 28);
            this.WallTypeBox.Name = "WallTypeBox";
            this.WallTypeBox.Size = new System.Drawing.Size(292, 196);
            this.WallTypeBox.TabIndex = 2;
            this.WallTypeBox.SelectedIndexChanged += new System.EventHandler(this.WallTypeBox_SelectedIndexChanged);
            // 
            // LevelBox
            // 
            this.LevelBox.FormattingEnabled = true;
            this.LevelBox.ItemHeight = 12;
            this.LevelBox.Location = new System.Drawing.Point(399, 28);
            this.LevelBox.Name = "LevelBox";
            this.LevelBox.Size = new System.Drawing.Size(292, 196);
            this.LevelBox.TabIndex = 3;
            // 
            // InputForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(733, 430);
            this.Controls.Add(this.LevelBox);
            this.Controls.Add(this.WallTypeBox);
            this.Controls.Add(this.StartModel);
            this.Controls.Add(this.ChooseFile);
            this.Margin = new System.Windows.Forms.Padding(1);
            this.Name = "InputForm";
            this.Text = "InputForm";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button ChooseFile;
        private System.Windows.Forms.Button StartModel;
        public System.Windows.Forms.ListBox WallTypeBox;
        public System.Windows.Forms.ListBox LevelBox;
    }
}