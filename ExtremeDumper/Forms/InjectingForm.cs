using System.Windows.Forms;

namespace ExtremeDumper.Forms
{
    internal partial class InjectingForm : Form
    {
        private uint _processId;

        public InjectingForm(uint processId, string processName)
        {
            InitializeComponent();
            Text = $"将DLL注入到进程{processName}(ID={processId.ToString()})";
            _processId = processId;
        }
    }
}
