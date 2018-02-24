using System.Windows.Forms;

namespace ExtremeDumper.Forms
{
    internal static class MessageBoxStub
    {
        public static void Show(string text, MessageBoxIcon icon)
        {
            MessageBox.Show(text, Application.ProductName, MessageBoxButtons.OK, icon);
        }

        public static DialogResult Show(string text, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            return MessageBox.Show(text, Application.ProductName, buttons, icon);
        }
    }
}
