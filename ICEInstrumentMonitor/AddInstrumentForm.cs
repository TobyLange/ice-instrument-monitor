using System;
using System.Windows.Forms;

namespace ICEInstrumentMonitor
{
    public partial class AddInstrumentForm : Form
    {
        public AddInstrumentForm()
        {
            InitializeComponent();

            UpdateControls();
        }

        private void textBoxTicker_TextChanged(object sender, EventArgs e)
        {
            UpdateControls();
        }

        private void UpdateControls()
        {
            buttonOK.Enabled = textBoxTicker.TextLength > 0;
        }
    }
}
