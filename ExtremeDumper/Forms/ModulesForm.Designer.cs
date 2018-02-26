namespace ExtremeDumper.Forms
{
    partial class ModulesForm
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
            this.lvwModules = new System.Windows.Forms.ListView();
            this.chModuleName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
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
            this.fbdlgDumped = new System.Windows.Forms.FolderBrowserDialog();
            this.mnuModulesContext.SuspendLayout();
            this.SuspendLayout();
            // 
            // lvwModules
            // 
            this.lvwModules.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lvwModules.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chModuleName,
            this.chModuleHandle,
            this.chModuleSize,
            this.chModulePath});
            this.lvwModules.ContextMenuStrip = this.mnuModulesContext;
            this.lvwModules.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvwModules.FullRowSelect = true;
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
            this.chModuleName.Text = "模块名";
            // 
            // chModuleHandle
            // 
            this.chModuleHandle.Text = "模块句柄（基址）";
            // 
            // chModuleSize
            // 
            this.chModuleSize.Text = "模块大小";
            // 
            // chModulePath
            // 
            this.chModulePath.Text = "模块路径";
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
            this.mnuModulesContext.Size = new System.Drawing.Size(185, 148);
            // 
            // mnuDumpModule
            // 
            this.mnuDumpModule.Name = "mnuDumpModule";
            this.mnuDumpModule.Size = new System.Drawing.Size(184, 22);
            this.mnuDumpModule.Text = "转储模块";
            this.mnuDumpModule.Click += new System.EventHandler(this.mnuDumpModule_Click);
            // 
            // mnuRefreshModuleList
            // 
            this.mnuRefreshModuleList.Name = "mnuRefreshModuleList";
            this.mnuRefreshModuleList.Size = new System.Drawing.Size(184, 22);
            this.mnuRefreshModuleList.Text = "刷新模块列表";
            this.mnuRefreshModuleList.Click += new System.EventHandler(this.mnuRefreshModuleList_Click);
            // 
            // mnuViewFunctions
            // 
            this.mnuViewFunctions.Name = "mnuViewFunctions";
            this.mnuViewFunctions.Size = new System.Drawing.Size(184, 22);
            this.mnuViewFunctions.Text = "查看导出函数列表";
            this.mnuViewFunctions.Click += new System.EventHandler(this.mnuViewFunctions_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(181, 6);
            // 
            // mnuOnlyDotNetModule
            // 
            this.mnuOnlyDotNetModule.CheckOnClick = true;
            this.mnuOnlyDotNetModule.Name = "mnuOnlyDotNetModule";
            this.mnuOnlyDotNetModule.Size = new System.Drawing.Size(184, 22);
            this.mnuOnlyDotNetModule.Text = "仅显示.Net模块";
            this.mnuOnlyDotNetModule.Click += new System.EventHandler(this.mnuOnlyDotNetModule_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(181, 6);
            // 
            // mnuGotoLocation
            // 
            this.mnuGotoLocation.Name = "mnuGotoLocation";
            this.mnuGotoLocation.Size = new System.Drawing.Size(184, 22);
            this.mnuGotoLocation.Text = "打开文件所在的位置";
            this.mnuGotoLocation.Click += new System.EventHandler(this.mnuGotoLocation_Click);
            // 
            // fbdlgDumped
            // 
            this.fbdlgDumped.Description = "选择转储文件保存位置";
            // 
            // ModulesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(933, 637);
            this.Controls.Add(this.lvwModules);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
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
        private System.Windows.Forms.FolderBrowserDialog fbdlgDumped;
        private System.Windows.Forms.ContextMenuStrip mnuModulesContext;
        private System.Windows.Forms.ToolStripMenuItem mnuDumpModule;
        private System.Windows.Forms.ToolStripMenuItem mnuRefreshModuleList;
        private System.Windows.Forms.ToolStripMenuItem mnuViewFunctions;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem mnuOnlyDotNetModule;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem mnuGotoLocation;
    }
}