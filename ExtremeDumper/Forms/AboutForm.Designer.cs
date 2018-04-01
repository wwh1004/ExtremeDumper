namespace ExtremeDumper.Forms
{
    partial class AboutForm
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
            this.picIcon = new System.Windows.Forms.PictureBox();
            this.lblTextAuthor = new System.Windows.Forms.Label();
            this.lblTextProductName = new System.Windows.Forms.Label();
            this.lblAuthor = new System.Windows.Forms.Label();
            this.lblVersion = new System.Windows.Forms.Label();
            this.lblTextThanks = new System.Windows.Forms.Label();
            this.lblThanks = new System.Windows.Forms.Label();
            this.llblGithub = new System.Windows.Forms.LinkLabel();
            ((System.ComponentModel.ISupportInitialize)(this.picIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // picIcon
            // 
            this.picIcon.Image = global::ExtremeDumper.Forms.Resources.Avatar;
            this.picIcon.Location = new System.Drawing.Point(0, 0);
            this.picIcon.Name = "picIcon";
            this.picIcon.Size = new System.Drawing.Size(256, 256);
            this.picIcon.TabIndex = 0;
            this.picIcon.TabStop = false;
            // 
            // lblTextAuthor
            // 
            this.lblTextAuthor.AutoSize = true;
            this.lblTextAuthor.Font = new System.Drawing.Font("微软雅黑", 13F);
            this.lblTextAuthor.Location = new System.Drawing.Point(262, 67);
            this.lblTextAuthor.Name = "lblTextAuthor";
            this.lblTextAuthor.Size = new System.Drawing.Size(46, 24);
            this.lblTextAuthor.TabIndex = 1;
            this.lblTextAuthor.Text = "作者";
            // 
            // lblTextProductName
            // 
            this.lblTextProductName.AutoSize = true;
            this.lblTextProductName.Font = new System.Drawing.Font("微软雅黑", 13F);
            this.lblTextProductName.Location = new System.Drawing.Point(262, 9);
            this.lblTextProductName.Name = "lblTextProductName";
            this.lblTextProductName.Size = new System.Drawing.Size(0, 24);
            this.lblTextProductName.TabIndex = 2;
            // 
            // lblAuthor
            // 
            this.lblAuthor.AutoSize = true;
            this.lblAuthor.Location = new System.Drawing.Point(263, 91);
            this.lblAuthor.Name = "lblAuthor";
            this.lblAuthor.Size = new System.Drawing.Size(61, 17);
            this.lblAuthor.TabIndex = 3;
            this.lblAuthor.Text = "wwh1004";
            // 
            // lblVersion
            // 
            this.lblVersion.AutoSize = true;
            this.lblVersion.Location = new System.Drawing.Point(263, 33);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(52, 17);
            this.lblVersion.TabIndex = 4;
            this.lblVersion.Text = "Version";
            // 
            // lblTextThanks
            // 
            this.lblTextThanks.AutoSize = true;
            this.lblTextThanks.Font = new System.Drawing.Font("微软雅黑", 13F);
            this.lblTextThanks.Location = new System.Drawing.Point(262, 126);
            this.lblTextThanks.Name = "lblTextThanks";
            this.lblTextThanks.Size = new System.Drawing.Size(46, 24);
            this.lblTextThanks.TabIndex = 5;
            this.lblTextThanks.Text = "感谢";
            // 
            // lblThanks
            // 
            this.lblThanks.AutoSize = true;
            this.lblThanks.Location = new System.Drawing.Point(263, 150);
            this.lblThanks.Name = "lblThanks";
            this.lblThanks.Size = new System.Drawing.Size(178, 51);
            this.lblThanks.TabIndex = 6;
            this.lblThanks.Text = "0xd4d - dnlib & dndbg\r\nCodeCracker - MegaDumper\r\nClrMD - Microsoft";
            // 
            // llblGithub
            // 
            this.llblGithub.AutoSize = true;
            this.llblGithub.Location = new System.Drawing.Point(263, 213);
            this.llblGithub.Name = "llblGithub";
            this.llblGithub.Size = new System.Drawing.Size(176, 34);
            this.llblGithub.TabIndex = 7;
            this.llblGithub.TabStop = true;
            this.llblGithub.Text = "https://github.com/23651039\r\n39/ExtremeDumper";
            this.llblGithub.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lkbGithub_LinkClicked);
            // 
            // AboutForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(452, 256);
            this.Controls.Add(this.llblGithub);
            this.Controls.Add(this.lblThanks);
            this.Controls.Add(this.lblTextThanks);
            this.Controls.Add(this.lblVersion);
            this.Controls.Add(this.lblAuthor);
            this.Controls.Add(this.lblTextProductName);
            this.Controls.Add(this.lblTextAuthor);
            this.Controls.Add(this.picIcon);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            ((System.ComponentModel.ISupportInitialize)(this.picIcon)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox picIcon;
        private System.Windows.Forms.Label lblTextAuthor;
        private System.Windows.Forms.Label lblTextProductName;
        private System.Windows.Forms.Label lblAuthor;
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.Label lblTextThanks;
        private System.Windows.Forms.Label lblThanks;
        private System.Windows.Forms.LinkLabel llblGithub;
    }
}