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
            this.chkWaitReturn.AutoSize = true;
            this.chkWaitReturn.Location = new System.Drawing.Point(902, 45);
            this.chkWaitReturn.Name = "chkWaitReturn";
            this.chkWaitReturn.Size = new System.Drawing.Size(53, 21);
            this.chkWaitReturn.TabIndex = 11;
            this.chkWaitReturn.Text = "Wait";
            this.chkWaitReturn.UseVisualStyleBackColor = true;
            // 
            // btInject
            // 
            this.btInject.Location = new System.Drawing.Point(983, 41);
            this.btInject.Name = "btInject";
            this.btInject.Size = new System.Drawing.Size(98, 27);
            this.btInject.TabIndex = 9;
            this.btInject.Text = "Inject";
            this.btInject.UseVisualStyleBackColor = true;
            this.btInject.Click += new System.EventHandler(this.btInject_Click);
            // 
            // cmbEntryPoint
            // 
            this.cmbEntryPoint.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbEntryPoint.FormattingEnabled = true;
            this.cmbEntryPoint.Location = new System.Drawing.Point(12, 43);
            this.cmbEntryPoint.Name = "cmbEntryPoint";
            this.cmbEntryPoint.Size = new System.Drawing.Size(718, 25);
            this.cmbEntryPoint.TabIndex = 8;
            this.cmbEntryPoint.SelectedIndexChanged += new System.EventHandler(this.cmbEntryPoint_SelectedIndexChanged);
            // 
            // btSelectAssembly
            // 
            this.btSelectAssembly.Location = new System.Drawing.Point(983, 13);
            this.btSelectAssembly.Name = "btSelectAssembly";
            this.btSelectAssembly.Size = new System.Drawing.Size(98, 23);
            this.btSelectAssembly.TabIndex = 7;
            this.btSelectAssembly.Text = "Select Assembly...";
            this.btSelectAssembly.UseVisualStyleBackColor = true;
            this.btSelectAssembly.Click += new System.EventHandler(this.btSelectAssembly_Click);
            // 
            // tbAssemblyPath
            // 
            this.tbAssemblyPath.Location = new System.Drawing.Point(12, 13);
            this.tbAssemblyPath.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tbAssemblyPath.Name = "tbAssemblyPath";
            this.tbAssemblyPath.Size = new System.Drawing.Size(965, 23);
            this.tbAssemblyPath.TabIndex = 6;
            this.tbAssemblyPath.TextChanged += new System.EventHandler(this.tbAssemblyPath_TextChanged);
            // 
            // tbArgument
            // 
            this.tbArgument.Location = new System.Drawing.Point(736, 43);
            this.tbArgument.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tbArgument.Name = "tbArgument";
            this.tbArgument.Size = new System.Drawing.Size(160, 23);
            this.tbArgument.TabIndex = 12;
            this.tbArgument.Text = "<Optional Argument>";
            this.tbArgument.TextChanged += new System.EventHandler(this.tbArgument_TextChanged);
            // 
            // InjectingForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1093, 82);
            this.Controls.Add(this.tbArgument);
            this.Controls.Add(this.chkWaitReturn);
            this.Controls.Add(this.btInject);
            this.Controls.Add(this.cmbEntryPoint);
            this.Controls.Add(this.btSelectAssembly);
            this.Controls.Add(this.tbAssemblyPath);
            this.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
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
