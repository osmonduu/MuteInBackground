using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MuteInBackground
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();

            // Load saved settings into the checkboxes
            chkRunAtStartup.Checked = Properties.Settings.Default.RunAtStartup;
            chkMinimizeOnClose.Checked = Properties.Settings.Default.MinimizeOnClose;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // Store current state of checkboxes
            Properties.Settings.Default.RunAtStartup = chkRunAtStartup.Checked;
            Properties.Settings.Default.MinimizeOnClose = chkMinimizeOnClose.Checked;

            // Save on disk
            Properties.Settings.Default.Save();

            // Apply "run at startup"
            StartupHelper.UpdateStartupShortcut(chkRunAtStartup.Checked);

            // Close the settings window
            this.Close();
        }
    }
}
