namespace ExtremeDumper.Forms;

partial class LoaderHookForm {
	/// <summary>
	/// Required designer variable.
	/// </summary>
	private System.ComponentModel.IContainer components = null;

	/// <summary>
	/// Clean up any resources being used.
	/// </summary>
	/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
	protected override void Dispose(bool disposing) {
		if (disposing && (components != null)) {
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	#region Windows Form Designer generated code

	/// <summary>
	/// Required method for Designer support - do not modify
	/// the contents of this method with the code editor.
	/// </summary>
	private void InitializeComponent() {
            this.btSelectAssembly = new System.Windows.Forms.Button();
            this.tbAssemblyPath = new System.Windows.Forms.TextBox();
            this.btnRun = new System.Windows.Forms.Button();
            this.odlgSelectAssembly = new System.Windows.Forms.OpenFileDialog();
            this.SuspendLayout();
            // 
            // btSelectAssembly
            // 
            this.btSelectAssembly.Location = new System.Drawing.Point(12, 47);
            this.btSelectAssembly.Name = "btSelectAssembly";
            this.btSelectAssembly.Size = new System.Drawing.Size(172, 27);
            this.btSelectAssembly.TabIndex = 9;
            this.btSelectAssembly.Text = "Select Assembly...";
            this.btSelectAssembly.UseVisualStyleBackColor = true;
            this.btSelectAssembly.Click += new System.EventHandler(this.btSelectAssembly_Click);
            // 
            // tbAssemblyPath
            // 
            this.tbAssemblyPath.Location = new System.Drawing.Point(12, 13);
            this.tbAssemblyPath.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tbAssemblyPath.Name = "tbAssemblyPath";
            this.tbAssemblyPath.Size = new System.Drawing.Size(856, 27);
            this.tbAssemblyPath.TabIndex = 8;
            this.tbAssemblyPath.TextChanged += new System.EventHandler(this.tbAssemblyPath_TextChanged);
            // 
            // btnRun
            // 
            this.btnRun.Location = new System.Drawing.Point(455, 47);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(172, 27);
            this.btnRun.TabIndex = 10;
            this.btnRun.Text = "Run With Hook";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // LoaderHookForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(878, 86);
            this.Controls.Add(this.btnRun);
            this.Controls.Add(this.btSelectAssembly);
            this.Controls.Add(this.tbAssemblyPath);
            this.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this.Icon = global::ExtremeDumper.Forms.Resources.Icon;
            this.Name = "LoaderHookForm";
            this.Text = "Loader Hook";
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.LoaderHookForm_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.LoaderHookForm_DragEnter);
            this.ResumeLayout(false);
            this.PerformLayout();

	}

	#endregion

	private System.Windows.Forms.Button btSelectAssembly;
	private System.Windows.Forms.TextBox tbAssemblyPath;
	private System.Windows.Forms.Button btnRun;
	private System.Windows.Forms.OpenFileDialog odlgSelectAssembly;
}
