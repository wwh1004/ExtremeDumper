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
            this.lvwFunctions = new System.Windows.Forms.ListView();
            this.chFunctionName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chFunctionAddress = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chOrdinal = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.mnuFunctionsContext = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuRefreshFunctionList = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuForceFalse = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuForceTrue = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuForceReturn = new System.Windows.Forms.ToolStripMenuItem();
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
            this.chFunctionName.Text = "导出函数名";
            // 
            // chFunctionAddress
            // 
            this.chFunctionAddress.Text = "导出函数地址";
            // 
            // chOrdinal
            // 
            this.chOrdinal.Text = "导出函数序号";
            // 
            // mnuFunctionsContext
            // 
            this.mnuFunctionsContext.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuRefreshFunctionList,
            this.toolStripSeparator1,
            this.mnuForceFalse,
            this.mnuForceTrue,
            this.mnuForceReturn});
            this.mnuFunctionsContext.Name = "mnuModulesContext";
            this.mnuFunctionsContext.Size = new System.Drawing.Size(173, 98);
            // 
            // mnuRefreshFunctionList
            // 
            this.mnuRefreshFunctionList.Name = "mnuRefreshFunctionList";
            this.mnuRefreshFunctionList.Size = new System.Drawing.Size(172, 22);
            this.mnuRefreshFunctionList.Text = "刷新导出函数列表";
            this.mnuRefreshFunctionList.Click += new System.EventHandler(this.mnuRefreshFunctionList_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(169, 6);
            // 
            // mnuForceFalse
            // 
            this.mnuForceFalse.Name = "mnuForceFalse";
            this.mnuForceFalse.Size = new System.Drawing.Size(172, 22);
            this.mnuForceFalse.Text = "强制返回False";
            this.mnuForceFalse.Click += new System.EventHandler(this.mnuForceFalse_Click);
            // 
            // mnuForceTrue
            // 
            this.mnuForceTrue.Name = "mnuForceTrue";
            this.mnuForceTrue.Size = new System.Drawing.Size(172, 22);
            this.mnuForceTrue.Text = "强制返回True";
            this.mnuForceTrue.Click += new System.EventHandler(this.mnuForceTrue_Click);
            // 
            // mnuForceReturn
            // 
            this.mnuForceReturn.Name = "mnuForceReturn";
            this.mnuForceReturn.Size = new System.Drawing.Size(172, 22);
            this.mnuForceReturn.Text = "强制直接返回";
            this.mnuForceReturn.Click += new System.EventHandler(this.mnuForceReturn_Click);
            // 
            // FunctionsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(933, 637);
            this.Controls.Add(this.lvwFunctions);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
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
        private System.Windows.Forms.ToolStripMenuItem mnuForceFalse;
        private System.Windows.Forms.ToolStripMenuItem mnuForceTrue;
        private System.Windows.Forms.ToolStripMenuItem mnuForceReturn;
    }
}