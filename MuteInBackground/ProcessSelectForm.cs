using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;

namespace MuteInBackground
{
    public partial class ProcessSelectForm : Form
    {
        public string SelectedProcessName { get; private set; }

        public void AddSessionControl(IAudioSessionControl s)
        {
            // Wrap IAudioSessionControl with AudioSessionControl to expose IAudioSessionControl2 functionality
            var wrapper = new AudioSessionControl(s);

            if (wrapper.State == AudioSessionState.AudioSessionStateExpired) return;

            var pid = (int)wrapper.GetProcessID;
            if (pid == 0) return;

            string processName;
            try { processName = Process.GetProcessById(pid).ProcessName; }
            catch { return; }

            string text = $"{processName} (PID {pid})";
            if (!lstSessions.Items.Contains(text))
                lstSessions.Items.Add(text);
        }

        public ProcessSelectForm(
            AudioSessionManager sessionManager,
            List<AudioSessionControl> sessions
        )
        {
            InitializeComponent();

            // Populate the ListBox with process names and pid
            foreach (var s in sessions)
            {
                // Get process name from session pid
                string display = GetProcessNameSafe(s.GetProcessID);
                string itemText = (display != null) ? $"{display} (PID {s.GetProcessID})": $"(PID {s.GetProcessID})";
                lstSessions.Items.Add(itemText);
            }

            btnSelect.Enabled = false;
            // Whenever an item is selected or deselected, run the lambda function -> enable/disable select button
            lstSessions.SelectedIndexChanged += (s, e) =>
            {
                btnSelect.Enabled = (lstSessions.SelectedItem != null);
            };
        }

        private string GetProcessNameSafe(uint pid)
        {
            try { return Process.GetProcessById((int)pid).ProcessName; }
            catch (ArgumentException) { return null; }
        }

        private void ProcessSelectForm_Load(object sender, EventArgs e)
        {

        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            // Disable the "Select" button if no process is selected from the list
            if (lstSessions.SelectedItem == null) return;
            // Extract the process name and pid
            string text = lstSessions.SelectedItem.ToString();
            // Isolate the name from the pid
            SelectedProcessName = text.Split(' ')[0];
            DialogResult = DialogResult.OK;
            // Close the form after selecting the process
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
