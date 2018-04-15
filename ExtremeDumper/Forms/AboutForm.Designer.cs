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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutForm));
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
            resources.ApplyResources(this.picIcon, "picIcon");
            this.picIcon.Image = global::ExtremeDumper.Forms.Resources.Avatar;
            this.picIcon.Name = "picIcon";
            this.picIcon.TabStop = false;
            // 
            // lblTextAuthor
            // 
            resources.ApplyResources(this.lblTextAuthor, "lblTextAuthor");
            this.lblTextAuthor.Name = "lblTextAuthor";
            // 
            // lblTextProductName
            // 
            resources.ApplyResources(this.lblTextProductName, "lblTextProductName");
            this.lblTextProductName.Name = "lblTextProductName";
            // 
            // lblAuthor
            // 
            resources.ApplyResources(this.lblAuthor, "lblAuthor");
            this.lblAuthor.Name = "lblAuthor";
            // 
            // lblVersion
            // 
            resources.ApplyResources(this.lblVersion, "lblVersion");
            this.lblVersion.Name = "lblVersion";
            // 
            // lblTextThanks
            // 
            resources.ApplyResources(this.lblTextThanks, "lblTextThanks");
            this.lblTextThanks.Name = "lblTextThanks";
            // 
            // lblThanks
            // 
            resources.ApplyResources(this.lblThanks, "lblThanks");
            this.lblThanks.Name = "lblThanks";
            // 
            // llblGithub
            // 
            resources.ApplyResources(this.llblGithub, "llblGithub");
            this.llblGithub.Name = "llblGithub";
            this.llblGithub.TabStop = true;
            this.llblGithub.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lkbGithub_LinkClicked);
            // 
            // AboutForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.llblGithub);
            this.Controls.Add(this.lblThanks);
            this.Controls.Add(this.lblTextThanks);
            this.Controls.Add(this.lblVersion);
            this.Controls.Add(this.lblAuthor);
            this.Controls.Add(this.lblTextProductName);
            this.Controls.Add(this.lblTextAuthor);
            this.Controls.Add(this.picIcon);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutForm";
            this.ShowInTaskbar = false;
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