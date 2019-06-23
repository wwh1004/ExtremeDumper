using System.Diagnostics;
using System.Windows.Forms;

namespace ExtremeDumper.Forms {
	internal partial class AboutForm : Form {
		public AboutForm() {
			InitializeComponent();
			Text = $"About {Application.ProductName}";
			lblTextProductName.Text = Application.ProductName;
			lblVersion.Text += $" {Application.ProductVersion}";
		}

		private void lkbGithub_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
			Process.Start("https://github.com/wwh1004/ExtremeDumper");
		}
	}
}
