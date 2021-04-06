namespace ExtremeDumper.Forms
{
    partial class ModulesForm
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
            this.lvwModules = new System.Windows.Forms.ListView();
            this.chModuleName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chDomainName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chCLRVersion = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chModuleHandle = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chModuleSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chModulePath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.mnuModulesContext = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuDumpModule = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuRefreshModuleList = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuViewFunctions = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuOnlyDotNetModule = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuGotoLocation = new System.Windows.Forms.ToolStripMenuItem();
            this.sfdlgDumped = new System.Windows.Forms.SaveFileDialog();
            this.mnuModulesContext.SuspendLayout();
            this.SuspendLayout();
            // 
            // lvwModules
            // 
            this.lvwModules.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lvwModules.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chModuleName,
            this.chDomainName,
            this.chCLRVersion,
            this.chModuleHandle,
            this.chModuleSize,
            this.chModulePath});
            this.lvwModules.ContextMenuStrip = this.mnuModulesContext;
            this.lvwModules.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvwModules.FullRowSelect = true;
            this.lvwModules.HideSelection = false;
            this.lvwModules.Location = new System.Drawing.Point(0, 0);
            this.lvwModules.Name = "lvwModules";
            this.lvwModules.Size = new System.Drawing.Size(933, 637);
            this.lvwModules.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.lvwModules.TabIndex = 0;
            this.lvwModules.UseCompatibleStateImageBehavior = false;
            this.lvwModules.View = System.Windows.Forms.View.Details;
            this.lvwModules.Resize += new System.EventHandler(this.lvwModules_Resize);
            // 
            // chModuleName
            // 
            this.chModuleName.Text = "Name";
            // 
            // chDomainName
            // 
            this.chDomainName.Text = "Domain Name";
            // 
            // chCLRVersion
            // 
            this.chCLRVersion.Text = "CLR Version";
            // 
            // chModuleHandle
            // 
            this.chModuleHandle.Text = "BaseAddress";
            // 
            // chModuleSize
            // 
            this.chModuleSize.Text = "Size";
            // 
            // chModulePath
            // 
            this.chModulePath.Text = "Path";
            // 
            // mnuModulesContext
            // 
            this.mnuModulesContext.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuDumpModule,
            this.mnuRefreshModuleList,
            this.mnuViewFunctions,
            this.toolStripSeparator1,
            this.mnuOnlyDotNetModule,
            this.toolStripSeparator2,
            this.mnuGotoLocation});
            this.mnuModulesContext.Name = "mnuModulesContext";
            this.mnuModulesContext.Size = new System.Drawing.Size(214, 126);
            // 
            // mnuDumpModule
            // 
            this.mnuDumpModule.Name = "mnuDumpModule";
            this.mnuDumpModule.Size = new System.Drawing.Size(213, 22);
            this.mnuDumpModule.Text = "Dump Selected Module";
            this.mnuDumpModule.Click += new System.EventHandler(this.mnuDumpModule_Click);
            // 
            // mnuRefreshModuleList
            // 
            this.mnuRefreshModuleList.Name = "mnuRefreshModuleList";
            this.mnuRefreshModuleList.Size = new System.Drawing.Size(213, 22);
            this.mnuRefreshModuleList.Text = "Refresh";
            this.mnuRefreshModuleList.Click += new System.EventHandler(this.mnuRefreshModuleList_Click);
            // 
            // mnuViewFunctions
            // 
            this.mnuViewFunctions.Name = "mnuViewFunctions";
            this.mnuViewFunctions.Size = new System.Drawing.Size(213, 22);
            this.mnuViewFunctions.Text = "View Export Functions";
            this.mnuViewFunctions.Click += new System.EventHandler(this.mnuViewFunctions_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(210, 6);
            // 
            // mnuOnlyDotNetModule
            // 
            this.mnuOnlyDotNetModule.CheckOnClick = true;
            this.mnuOnlyDotNetModule.Name = "mnuOnlyDotNetModule";
            this.mnuOnlyDotNetModule.Size = new System.Drawing.Size(213, 22);
            this.mnuOnlyDotNetModule.Text = "Only .NET Modules";
            this.mnuOnlyDotNetModule.Click += new System.EventHandler(this.mnuOnlyDotNetModule_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(210, 6);
            // 
            // mnuGotoLocation
            // 
            this.mnuGotoLocation.Name = "mnuGotoLocation";
            this.mnuGotoLocation.Size = new System.Drawing.Size(213, 22);
            this.mnuGotoLocation.Text = "Goto Location";
            this.mnuGotoLocation.Click += new System.EventHandler(this.mnuGotoLocation_Click);
            // 
            // ModulesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(933, 637);
            this.Controls.Add(this.lvwModules);
            this.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this.Icon = global::ExtremeDumper.Forms.Resources.Icon;
            this.Name = "ModulesForm";
            this.mnuModulesContext.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView lvwModules;
        private System.Windows.Forms.ColumnHeader chModuleName;
        private System.Windows.Forms.ColumnHeader chModuleHandle;
        private System.Windows.Forms.ColumnHeader chModuleSize;
        private System.Windows.Forms.ColumnHeader chModulePath;
        private System.Windows.Forms.SaveFileDialog sfdlgDumped;
        private System.Windows.Forms.ContextMenuStrip mnuModulesContext;
        private System.Windows.Forms.ToolStripMenuItem mnuDumpModule;
        private System.Windows.Forms.ToolStripMenuItem mnuRefreshModuleList;
        private System.Windows.Forms.ToolStripMenuItem mnuViewFunctions;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem mnuOnlyDotNetModule;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem mnuGotoLocation;
		private System.Windows.Forms.ColumnHeader chDomainName;
		private System.Windows.Forms.ColumnHeader chCLRVersion;
	}
}
