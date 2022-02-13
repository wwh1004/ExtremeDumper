namespace ExtremeDumper.Forms
{
    partial class FunctionsForm
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
            this.components = new System.ComponentModel.Container();
            this.lvwFunctions = new System.Windows.Forms.ListView();
            this.chFunctionName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chFunctionAddress = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chOrdinal = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.mnuFunctionsContext = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuRefreshFunctionList = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuFunctionsContext.SuspendLayout();
            this.SuspendLayout();
            // 
            // lvwFunctions
            // 
            this.lvwFunctions.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lvwFunctions.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chFunctionName,
            this.chFunctionAddress,
            this.chOrdinal});
            this.lvwFunctions.ContextMenuStrip = this.mnuFunctionsContext;
            this.lvwFunctions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvwFunctions.FullRowSelect = true;
            this.lvwFunctions.HideSelection = false;
            this.lvwFunctions.Location = new System.Drawing.Point(0, 0);
            this.lvwFunctions.Name = "lvwFunctions";
            this.lvwFunctions.Size = new System.Drawing.Size(933, 637);
            this.lvwFunctions.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.lvwFunctions.TabIndex = 0;
            this.lvwFunctions.UseCompatibleStateImageBehavior = false;
            this.lvwFunctions.View = System.Windows.Forms.View.Details;
            this.lvwFunctions.Resize += new System.EventHandler(this.lvwFunctions_Resize);
            // 
            // chFunctionName
            // 
            this.chFunctionName.Text = "Name";
            // 
            // chFunctionAddress
            // 
            this.chFunctionAddress.Text = "Address";
            // 
            // chOrdinal
            // 
            this.chOrdinal.Text = "Ordinal";
            // 
            // mnuFunctionsContext
            // 
            this.mnuFunctionsContext.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuRefreshFunctionList,
            this.toolStripSeparator1});
            this.mnuFunctionsContext.Name = "mnuModulesContext";
            this.mnuFunctionsContext.Size = new System.Drawing.Size(121, 32);
            // 
            // mnuRefreshFunctionList
            // 
            this.mnuRefreshFunctionList.Name = "mnuRefreshFunctionList";
            this.mnuRefreshFunctionList.Size = new System.Drawing.Size(120, 22);
            this.mnuRefreshFunctionList.Text = "Refresh";
            this.mnuRefreshFunctionList.Click += new System.EventHandler(this.mnuRefreshFunctionList_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(117, 6);
            // 
            // FunctionsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(933, 637);
            this.Controls.Add(this.lvwFunctions);
            this.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this.Icon = global::ExtremeDumper.Forms.Resources.Icon;
            this.Name = "FunctionsForm";
            this.mnuFunctionsContext.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView lvwFunctions;
        private System.Windows.Forms.ColumnHeader chFunctionName;
        private System.Windows.Forms.ColumnHeader chFunctionAddress;
        private System.Windows.Forms.ColumnHeader chOrdinal;
        private System.Windows.Forms.ContextMenuStrip mnuFunctionsContext;
        private System.Windows.Forms.ToolStripMenuItem mnuRefreshFunctionList;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
    }
}
