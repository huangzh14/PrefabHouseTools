namespace PrefabHouseTools
{
    partial class CmdAutoRouteEform
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
            this.button_ok = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.listCeilingLevel = new System.Windows.Forms.ListBox();
            this.listFloorLevel = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // button_ok
            // 
            this.button_ok.Location = new System.Drawing.Point(701, 560);
            this.button_ok.Name = "button_ok";
            this.button_ok.Size = new System.Drawing.Size(173, 63);
            this.button_ok.TabIndex = 0;
            this.button_ok.Text = "OK";
            this.button_ok.UseVisualStyleBackColor = true;
            this.button_ok.Click += new System.EventHandler(this.button_ok_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Location = new System.Drawing.Point(952, 560);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(177, 62);
            this.button_Cancel.TabIndex = 1;
            this.button_Cancel.Text = "Cancel";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // listCeilingLevel
            // 
            this.listCeilingLevel.FormattingEnabled = true;
            this.listCeilingLevel.ItemHeight = 30;
            this.listCeilingLevel.Location = new System.Drawing.Point(52, 194);
            this.listCeilingLevel.Name = "listCeilingLevel";
            this.listCeilingLevel.Size = new System.Drawing.Size(488, 274);
            this.listCeilingLevel.TabIndex = 2;
            // 
            // listFloorLevel
            // 
            this.listFloorLevel.FormattingEnabled = true;
            this.listFloorLevel.ItemHeight = 30;
            this.listFloorLevel.Location = new System.Drawing.Point(625, 194);
            this.listFloorLevel.Name = "listFloorLevel";
            this.listFloorLevel.Size = new System.Drawing.Size(488, 274);
            this.listFloorLevel.TabIndex = 3;
            this.listFloorLevel.SelectedIndexChanged += new System.EventHandler(this.listFloorLevel_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(47, 69);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(253, 30);
            this.label1.TabIndex = 4;
            this.label1.Text = "请选择结构顶标高";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(620, 69);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(253, 30);
            this.label2.TabIndex = 5;
            this.label2.Text = "请选择结构底标高";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(47, 132);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(493, 30);
            this.label4.TabIndex = 6;
            this.label4.Text = "Choose structural ceiling level.";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(620, 132);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(463, 30);
            this.label3.TabIndex = 7;
            this.label3.Text = "Choose structural floor level.";
            // 
            // CmdAutoRouteEform
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(15F, 30F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1199, 673);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.listFloorLevel);
            this.Controls.Add(this.listCeilingLevel);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_ok);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "CmdAutoRouteEform";
            this.Text = "LevelSelection标高选择";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_ok;
        private System.Windows.Forms.Button button_Cancel;
        public System.Windows.Forms.ListBox listCeilingLevel;
        public System.Windows.Forms.ListBox listFloorLevel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
    }
}