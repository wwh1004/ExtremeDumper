namespace ExtremeDumper.Forms
{
    partial class ProcessesForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.lvwProcesses = new System.Windows.Forms.ListView();
            this.chProcessName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chProcessId = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chProcessPath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.mnuProcessContext = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuDumpProcess = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuRefreshProcessList = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuViewModules = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuOnlyDotNetProcess = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuInjectDll = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuGotoLocation = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMain = new System.Windows.Forms.MenuStrip();
            this.mnuOptions = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuDebugPrivilege = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuDumperType = new System.Windows.Forms.ToolStripMenuItem();
            this.fbdlgDumped = new Ookii.Dialogs.WinForms.VistaFolderBrowserDialog();
            this.mnuProcessContext.SuspendLayout();
            this.mnuMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // lvwProcesses
            // 
            this.lvwProcesses.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lvwProcesses.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chProcessName,
            this.chProcessId,
            this.chProcessPath});
            this.lvwProcesses.ContextMenuStrip = this.mnuProcessContext;
            this.lvwProcesses.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvwProcesses.FullRowSelect = true;
            this.lvwProcesses.HideSelection = false;
            this.lvwProcesses.Location = new System.Drawing.Point(0, 27);
            this.lvwProcesses.Name = "lvwProcesses";
            this.lvwProcesses.Size = new System.Drawing.Size(933, 610);
            this.lvwProcesses.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.lvwProcesses.TabIndex = 0;
            this.lvwProcesses.UseCompatibleStateImageBehavior = false;
            this.lvwProcesses.View = System.Windows.Forms.View.Details;
            this.lvwProcesses.Resize += new System.EventHandler(this.lvwProcesses_Resize);
            // 
            // chProcessName
            // 
            this.chProcessName.Text = "Name";
            // 
            // chProcessId
            // 
            this.chProcessId.Text = "PID";
            // 
            // chProcessPath
            // 
            this.chProcessPath.Text = "Path";
            // 
            // mnuProcessContext
            // 
            this.mnuProcessContext.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuDumpProcess,
            this.mnuRefreshProcessList,
            this.mnuViewModules,
            this.toolStripSeparator2,
            this.mnuOnlyDotNetProcess,
            this.toolStripSeparator3,
            this.mnuInjectDll,
            this.toolStripSeparator4,
            this.mnuGotoLocation});
            this.mnuProcessContext.Name = "contextMenuStrip1";
            this.mnuProcessContext.Size = new System.Drawing.Size(214, 154);
            // 
            // mnuDumpProcess
            // 
            this.mnuDumpProcess.Name = "mnuDumpProcess";
            this.mnuDumpProcess.Size = new System.Drawing.Size(213, 22);
            this.mnuDumpProcess.Text = "Dump Selected Process";
            this.mnuDumpProcess.Click += new System.EventHandler(this.mnuDumpProcess_Click);
            // 
            // mnuRefreshProcessList
            // 
            this.mnuRefreshProcessList.Name = "mnuRefreshProcessList";
            this.mnuRefreshProcessList.Size = new System.Drawing.Size(213, 22);
            this.mnuRefreshProcessList.Text = "Refresh";
            this.mnuRefreshProcessList.Click += new System.EventHandler(this.mnuRefreshProcessList_Click);
            // 
            // mnuViewModules
            // 
            this.mnuViewModules.Name = "mnuViewModules";
            this.mnuViewModules.Size = new System.Drawing.Size(213, 22);
            this.mnuViewModules.Text = "View Modules";
            this.mnuViewModules.Click += new System.EventHandler(this.mnuViewModules_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(210, 6);
            // 
            // mnuOnlyDotNetProcess
            // 
            this.mnuOnlyDotNetProcess.Checked = true;
            this.mnuOnlyDotNetProcess.CheckOnClick = true;
            this.mnuOnlyDotNetProcess.CheckState = System.Windows.Forms.CheckState.Checked;
            this.mnuOnlyDotNetProcess.Name = "mnuOnlyDotNetProcess";
            this.mnuOnlyDotNetProcess.Size = new System.Drawing.Size(213, 22);
            this.mnuOnlyDotNetProcess.Text = "Only .NET Processes";
            this.mnuOnlyDotNetProcess.Click += new System.EventHandler(this.mnuOnlyDotNetProcess_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(210, 6);
            // 
            // mnuInjectDll
            // 
            this.mnuInjectDll.Name = "mnuInjectDll";
            this.mnuInjectDll.Size = new System.Drawing.Size(213, 22);
            this.mnuInjectDll.Text = "Inject Dll";
            this.mnuInjectDll.Click += new System.EventHandler(this.mnuInjectDll_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(210, 6);
            // 
            // mnuGotoLocation
            // 
            this.mnuGotoLocation.Name = "mnuGotoLocation";
            this.mnuGotoLocation.Size = new System.Drawing.Size(213, 22);
            this.mnuGotoLocation.Text = "Goto Location";
            this.mnuGotoLocation.Click += new System.EventHandler(this.mnuGotoLocation_Click);
            // 
            // mnuMain
            // 
            this.mnuMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuOptions});
            this.mnuMain.Location = new System.Drawing.Point(0, 0);
            this.mnuMain.Name = "mnuMain";
            this.mnuMain.Padding = new System.Windows.Forms.Padding(7, 3, 0, 3);
            this.mnuMain.Size = new System.Drawing.Size(933, 27);
            this.mnuMain.TabIndex = 1;
            // 
            // mnuOptions
            // 
            this.mnuOptions.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuDebugPrivilege,
            this.toolStripSeparator1,
            this.mnuDumperType});
            this.mnuOptions.Name = "mnuOptions";
            this.mnuOptions.Size = new System.Drawing.Size(66, 21);
            this.mnuOptions.Text = "Options";
            // 
            // mnuDebugPrivilege
            // 
            this.mnuDebugPrivilege.Name = "mnuDebugPrivilege";
            this.mnuDebugPrivilege.Size = new System.Drawing.Size(211, 22);
            this.mnuDebugPrivilege.Text = "Enable Debug Privilege";
            this.mnuDebugPrivilege.Click += new System.EventHandler(this.mnuDebugPrivilege_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(208, 6);
            // 
            // mnuDumperType
            // 
            this.mnuDumperType.Name = "mnuDumperType";
            this.mnuDumperType.Size = new System.Drawing.Size(211, 22);
            this.mnuDumperType.Text = "DumperType";
            // 
            // fbdlgDumped
            // 
            this.fbdlgDumped.Description = "Select a directory to save dumps";
            this.fbdlgDumped.UseDescriptionForTitle = true;
            // 
            // ProcessesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(933, 637);
            this.Controls.Add(this.lvwProcesses);
            this.Controls.Add(this.mnuMain);
            this.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this.Icon = global::ExtremeDumper.Forms.Resources.Icon;
            this.MainMenuStrip = this.mnuMain;
            this.Name = "ProcessesForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.mnuProcessContext.ResumeLayout(false);
            this.mnuMain.ResumeLayout(false);
            this.mnuMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView lvwProcesses;
        private System.Windows.Forms.MenuStrip mnuMain;
        private System.Windows.Forms.ToolStripMenuItem mnuOptions;
        private System.Windows.Forms.ToolStripMenuItem mnuDebugPrivilege;
        private System.Windows.Forms.ToolStripMenuItem mnuDumperType;
        private System.Windows.Forms.ColumnHeader chProcessName;
        private System.Windows.Forms.ColumnHeader chProcessId;
        private System.Windows.Forms.ColumnHeader chProcessPath;
        private System.Windows.Forms.ContextMenuStrip mnuProcessContext;
        private System.Windows.Forms.ToolStripMenuItem mnuDumpProcess;
        private System.Windows.Forms.ToolStripMenuItem mnuViewModules;
        private System.Windows.Forms.ToolStripMenuItem mnuOnlyDotNetProcess;
        private System.Windows.Forms.ToolStripMenuItem mnuRefreshProcessList;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private Ookii.Dialogs.WinForms.VistaFolderBrowserDialog fbdlgDumped;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem mnuInjectDll;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem mnuGotoLocation;
    }
}

