namespace ExtremeDumper.Forms
{
    partial class InjectingForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InjectingForm));
            this.chkWaitReturn = new System.Windows.Forms.CheckBox();
            this.btInject = new System.Windows.Forms.Button();
            this.cmbEntryPoint = new System.Windows.Forms.ComboBox();
            this.btSelectAssembly = new System.Windows.Forms.Button();
            this.tbAssemblyPath = new System.Windows.Forms.TextBox();
            this.tbArgument = new System.Windows.Forms.TextBox();
            this.odlgSelectAssembly = new System.Windows.Forms.OpenFileDialog();
            this.SuspendLayout();
            // 
            // chkWaitReturn
            // 
            resources.ApplyResources(this.chkWaitReturn, "chkWaitReturn");
            this.chkWaitReturn.Name = "chkWaitReturn";
            this.chkWaitReturn.UseVisualStyleBackColor = true;
            // 
            // btInject
            // 
            resources.ApplyResources(this.btInject, "btInject");
            this.btInject.Name = "btInject";
            this.btInject.UseVisualStyleBackColor = true;
            this.btInject.Click += new System.EventHandler(this.btInject_Click);
            // 
            // cmbEntryPoint
            // 
            resources.ApplyResources(this.cmbEntryPoint, "cmbEntryPoint");
            this.cmbEntryPoint.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbEntryPoint.FormattingEnabled = true;
            this.cmbEntryPoint.Name = "cmbEntryPoint";
            this.cmbEntryPoint.SelectedIndexChanged += new System.EventHandler(this.cmbEntryPoint_SelectedIndexChanged);
            // 
            // btSelectAssembly
            // 
            resources.ApplyResources(this.btSelectAssembly, "btSelectAssembly");
            this.btSelectAssembly.Name = "btSelectAssembly";
            this.btSelectAssembly.UseVisualStyleBackColor = true;
            this.btSelectAssembly.Click += new System.EventHandler(this.btSelectAssembly_Click);
            // 
            // tbAssemblyPath
            // 
            resources.ApplyResources(this.tbAssemblyPath, "tbAssemblyPath");
            this.tbAssemblyPath.Name = "tbAssemblyPath";
            this.tbAssemblyPath.TextChanged += new System.EventHandler(this.tbAssemblyPath_TextChanged);
            // 
            // tbArgument
            // 
            resources.ApplyResources(this.tbArgument, "tbArgument");
            this.tbArgument.Name = "tbArgument";
            this.tbArgument.TextChanged += new System.EventHandler(this.tbArgument_TextChanged);
            // 
            // odlgSelectAssembly
            // 
            resources.ApplyResources(this.odlgSelectAssembly, "odlgSelectAssembly");
            // 
            // InjectingForm
            // 
            resources.ApplyResources(this, "$this");
            this.AllowDrop = true;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tbArgument);
            this.Controls.Add(this.chkWaitReturn);
            this.Controls.Add(this.btInject);
            this.Controls.Add(this.cmbEntryPoint);
            this.Controls.Add(this.btSelectAssembly);
            this.Controls.Add(this.tbAssemblyPath);
            this.Icon = global::ExtremeDumper.Forms.Resources.Icon;
            this.Name = "InjectingForm";
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.InjectingForm_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.InjectingForm_DragEnter);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkWaitReturn;
        private System.Windows.Forms.Button btInject;
        private System.Windows.Forms.ComboBox cmbEntryPoint;
        private System.Windows.Forms.Button btSelectAssembly;
        private System.Windows.Forms.TextBox tbAssemblyPath;
        private System.Windows.Forms.TextBox tbArgument;
        private System.Windows.Forms.OpenFileDialog odlgSelectAssembly;
    }
}
