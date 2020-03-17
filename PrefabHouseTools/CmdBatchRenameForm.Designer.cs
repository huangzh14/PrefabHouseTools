namespace PrefabHouseTools
{
    partial class CmdBatchRenameForm
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
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.openFolderButton = new System.Windows.Forms.Button();
            this.openMatchCsvButton = new System.Windows.Forms.Button();
            this.startRenameButton = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.nameTextBox = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // openFolderButton
            // 
            this.openFolderButton.Location = new System.Drawing.Point(733, 58);
            this.openFolderButton.Name = "openFolderButton";
            this.openFolderButton.Size = new System.Drawing.Size(193, 70);
            this.openFolderButton.TabIndex = 1;
            this.openFolderButton.Text = "选择文件夹";
            this.openFolderButton.UseVisualStyleBackColor = true;
            this.openFolderButton.Click += new System.EventHandler(this.openFolderButton_Click);
            // 
            // openMatchCsvButton
            // 
            this.openMatchCsvButton.Location = new System.Drawing.Point(733, 174);
            this.openMatchCsvButton.Name = "openMatchCsvButton";
            this.openMatchCsvButton.Size = new System.Drawing.Size(193, 70);
            this.openMatchCsvButton.TabIndex = 2;
            this.openMatchCsvButton.Text = "选择对应表（.csv)";
            this.openMatchCsvButton.UseVisualStyleBackColor = true;
            this.openMatchCsvButton.Click += new System.EventHandler(this.openMatchCsvButton_Click);
            // 
            // startRenameButton
            // 
            this.startRenameButton.Location = new System.Drawing.Point(733, 657);
            this.startRenameButton.Name = "startRenameButton";
            this.startRenameButton.Size = new System.Drawing.Size(193, 70);
            this.startRenameButton.TabIndex = 3;
            this.startRenameButton.Text = "批量重命名";
            this.startRenameButton.UseVisualStyleBackColor = true;
            this.startRenameButton.Click += new System.EventHandler(this.startRenameButton_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(739, 282);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(208, 30);
            this.label1.TabIndex = 4;
            this.label1.Text = "csv格式为两列";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(739, 326);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(223, 30);
            this.label2.TabIndex = 5;
            this.label2.Text = "首列为原文件名";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(739, 374);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(223, 30);
            this.label3.TabIndex = 6;
            this.label3.Text = "次列为新文件名";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(739, 419);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(283, 30);
            this.label4.TabIndex = 7;
            this.label4.Text = "新文件名不包含扩展";
            // 
            // nameTextBox
            // 
            this.nameTextBox.Location = new System.Drawing.Point(31, 32);
            this.nameTextBox.Name = "nameTextBox";
            this.nameTextBox.Size = new System.Drawing.Size(621, 695);
            this.nameTextBox.TabIndex = 8;
            this.nameTextBox.Text = "";
            // 
            // CmdBatchRenameForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(15F, 30F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1034, 755);
            this.Controls.Add(this.nameTextBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.startRenameButton);
            this.Controls.Add(this.openMatchCsvButton);
            this.Controls.Add(this.openFolderButton);
            this.Name = "CmdBatchRenameForm";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.Button openFolderButton;
        private System.Windows.Forms.Button openMatchCsvButton;
        private System.Windows.Forms.Button startRenameButton;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.RichTextBox nameTextBox;
    }
}