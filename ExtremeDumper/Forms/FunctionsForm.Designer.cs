namespace ExtremeDumper.Forms
{
    partial class FunctionsForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FunctionsForm));
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
            resources.ApplyResources(this.lvwFunctions, "lvwFunctions");
            this.lvwFunctions.FullRowSelect = true;
            this.lvwFunctions.Name = "lvwFunctions";
            this.lvwFunctions.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.lvwFunctions.UseCompatibleStateImageBehavior = false;
            this.lvwFunctions.View = System.Windows.Forms.View.Details;
            this.lvwFunctions.Resize += new System.EventHandler(this.lvwFunctions_Resize);
            // 
            // chFunctionName
            // 
            resources.ApplyResources(this.chFunctionName, "chFunctionName");
            // 
            // chFunctionAddress
            // 
            resources.ApplyResources(this.chFunctionAddress, "chFunctionAddress");
            // 
            // chOrdinal
            // 
            resources.ApplyResources(this.chOrdinal, "chOrdinal");
            // 
            // mnuFunctionsContext
            // 
            this.mnuFunctionsContext.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuRefreshFunctionList,
            this.toolStripSeparator1});
            this.mnuFunctionsContext.Name = "mnuModulesContext";
            resources.ApplyResources(this.mnuFunctionsContext, "mnuFunctionsContext");
            // 
            // mnuRefreshFunctionList
            // 
            this.mnuRefreshFunctionList.Name = "mnuRefreshFunctionList";
            resources.ApplyResources(this.mnuRefreshFunctionList, "mnuRefreshFunctionList");
            this.mnuRefreshFunctionList.Click += new System.EventHandler(this.mnuRefreshFunctionList_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            // 
            // FunctionsForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lvwFunctions);
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
