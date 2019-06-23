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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProcessesForm));
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
            this.mnuDumperCore = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuUseMegaDumper = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuUseMetaDumper = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuHelp = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.fbdlgDumped = new System.Windows.Forms.FolderBrowserDialog();
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
            resources.ApplyResources(this.lvwProcesses, "lvwProcesses");
            this.lvwProcesses.FullRowSelect = true;
            this.lvwProcesses.Name = "lvwProcesses";
            this.lvwProcesses.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.lvwProcesses.UseCompatibleStateImageBehavior = false;
            this.lvwProcesses.View = System.Windows.Forms.View.Details;
            this.lvwProcesses.Resize += new System.EventHandler(this.lvwProcesses_Resize);
            // 
            // chProcessName
            // 
            resources.ApplyResources(this.chProcessName, "chProcessName");
            // 
            // chProcessId
            // 
            resources.ApplyResources(this.chProcessId, "chProcessId");
            // 
            // chProcessPath
            // 
            resources.ApplyResources(this.chProcessPath, "chProcessPath");
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
            resources.ApplyResources(this.mnuProcessContext, "mnuProcessContext");
            // 
            // mnuDumpProcess
            // 
            this.mnuDumpProcess.Name = "mnuDumpProcess";
            resources.ApplyResources(this.mnuDumpProcess, "mnuDumpProcess");
            this.mnuDumpProcess.Click += new System.EventHandler(this.mnuDumpProcess_Click);
            // 
            // mnuRefreshProcessList
            // 
            this.mnuRefreshProcessList.Name = "mnuRefreshProcessList";
            resources.ApplyResources(this.mnuRefreshProcessList, "mnuRefreshProcessList");
            this.mnuRefreshProcessList.Click += new System.EventHandler(this.mnuRefreshProcessList_Click);
            // 
            // mnuViewModules
            // 
            this.mnuViewModules.Name = "mnuViewModules";
            resources.ApplyResources(this.mnuViewModules, "mnuViewModules");
            this.mnuViewModules.Click += new System.EventHandler(this.mnuViewModules_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
            // 
            // mnuOnlyDotNetProcess
            // 
            this.mnuOnlyDotNetProcess.Checked = true;
            this.mnuOnlyDotNetProcess.CheckOnClick = true;
            this.mnuOnlyDotNetProcess.CheckState = System.Windows.Forms.CheckState.Checked;
            this.mnuOnlyDotNetProcess.Name = "mnuOnlyDotNetProcess";
            resources.ApplyResources(this.mnuOnlyDotNetProcess, "mnuOnlyDotNetProcess");
            this.mnuOnlyDotNetProcess.Click += new System.EventHandler(this.mnuOnlyDotNetProcess_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            resources.ApplyResources(this.toolStripSeparator3, "toolStripSeparator3");
            // 
            // mnuInjectDll
            // 
            this.mnuInjectDll.Name = "mnuInjectDll";
            resources.ApplyResources(this.mnuInjectDll, "mnuInjectDll");
            this.mnuInjectDll.Click += new System.EventHandler(this.mnuInjectDll_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            resources.ApplyResources(this.toolStripSeparator4, "toolStripSeparator4");
            // 
            // mnuGotoLocation
            // 
            this.mnuGotoLocation.Name = "mnuGotoLocation";
            resources.ApplyResources(this.mnuGotoLocation, "mnuGotoLocation");
            this.mnuGotoLocation.Click += new System.EventHandler(this.mnuGotoLocation_Click);
            // 
            // mnuMain
            // 
            this.mnuMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuOptions,
            this.mnuHelp});
            resources.ApplyResources(this.mnuMain, "mnuMain");
            this.mnuMain.Name = "mnuMain";
            // 
            // mnuOptions
            // 
            this.mnuOptions.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuDebugPrivilege,
            this.toolStripSeparator1,
            this.mnuDumperCore});
            this.mnuOptions.Name = "mnuOptions";
            resources.ApplyResources(this.mnuOptions, "mnuOptions");
            // 
            // mnuDebugPrivilege
            // 
            this.mnuDebugPrivilege.Name = "mnuDebugPrivilege";
            resources.ApplyResources(this.mnuDebugPrivilege, "mnuDebugPrivilege");
            this.mnuDebugPrivilege.Click += new System.EventHandler(this.mnuDebugPrivilege_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            // 
            // mnuDumperCore
            // 
            this.mnuDumperCore.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuUseMegaDumper,
            this.mnuUseMetaDumper});
            this.mnuDumperCore.Name = "mnuDumperCore";
            resources.ApplyResources(this.mnuDumperCore, "mnuDumperCore");
            // 
            // mnuUseMegaDumper
            // 
            this.mnuUseMegaDumper.Checked = true;
            this.mnuUseMegaDumper.CheckState = System.Windows.Forms.CheckState.Checked;
            this.mnuUseMegaDumper.Name = "mnuUseMegaDumper";
            resources.ApplyResources(this.mnuUseMegaDumper, "mnuUseMegaDumper");
            this.mnuUseMegaDumper.Click += new System.EventHandler(this.mnuUseMegaDumper_Click);
            // 
            // mnuUseMetaDumper
            // 
            this.mnuUseMetaDumper.Name = "mnuUseMetaDumper";
            resources.ApplyResources(this.mnuUseMetaDumper, "mnuUseMetaDumper");
            this.mnuUseMetaDumper.Click += new System.EventHandler(this.mnuUseMetaDumper_Click);
            // 
            // mnuHelp
            // 
            this.mnuHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuAbout});
            this.mnuHelp.Name = "mnuHelp";
            resources.ApplyResources(this.mnuHelp, "mnuHelp");
            // 
            // mnuAbout
            // 
            this.mnuAbout.Name = "mnuAbout";
            resources.ApplyResources(this.mnuAbout, "mnuAbout");
            this.mnuAbout.Click += new System.EventHandler(this.mnuAbout_Click);
            // 
            // fbdlgDumped
            // 
            resources.ApplyResources(this.fbdlgDumped, "fbdlgDumped");
            // 
            // MainForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lvwProcesses);
            this.Controls.Add(this.mnuMain);
            this.Icon = global::ExtremeDumper.Forms.Resources.Icon;
            this.MainMenuStrip = this.mnuMain;
            this.Name = "MainForm";
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
        private System.Windows.Forms.ToolStripMenuItem mnuHelp;
        private System.Windows.Forms.ToolStripMenuItem mnuAbout;
        private System.Windows.Forms.ToolStripMenuItem mnuDebugPrivilege;
        private System.Windows.Forms.ToolStripMenuItem mnuDumperCore;
        private System.Windows.Forms.ToolStripMenuItem mnuUseMegaDumper;
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
        private System.Windows.Forms.FolderBrowserDialog fbdlgDumped;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem mnuInjectDll;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem mnuGotoLocation;
        private System.Windows.Forms.ToolStripMenuItem mnuUseMetaDumper;
    }
}

