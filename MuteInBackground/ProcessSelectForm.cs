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
using System.IO;

namespace MuteInBackground
{
    public partial class ProcessSelectForm : Form
    {
        // Used to pass the selected process and icon to the main form
        public ListViewItem SelectedProcess { get; private set; }
        public ImageList SessionImageList => this.imageListSelectProc;

        /// <summary>
        /// Takes AudioSessionControl passed from handler (new session getting created while the form is open)
        /// and adds the process to the ListView.
        /// </summary>
        /// <param name="s"></param>
        public void AddSessionControl(IAudioSessionControl s)
        {
            // Wrap IAudioSessionControl with AudioSessionControl to expose IAudioSessionControl2 functionality
            var wrapper = new AudioSessionControl(s);

            if (wrapper.State == AudioSessionState.AudioSessionStateExpired) return;

            var pid = (int)wrapper.GetProcessID;
            if (pid == 0) return;

            Process proc;
            try { proc = Process.GetProcessById(pid); }
            catch { return; }

            // Add item to the ListView
            AddListViewItem(proc);
        }

        /// <summary>
        /// Adds the process display friendly name and icon to the ListView.
        /// </summary>
        /// <param name="proc"></param>
        private void AddListViewItem(Process proc)
        {
            // Get display friendly name with version info (if possible)
            int pid = proc.Id;
            string exePath = IconHelper.GetExecutablePath(pid);
            string displayName = Path.GetFileNameWithoutExtension(exePath);  // fallback
            if (!string.IsNullOrEmpty(exePath))
            {
                try
                {
                    var vi = FileVersionInfo.GetVersionInfo(exePath);
                    displayName = !string.IsNullOrWhiteSpace(vi.FileDescription) ? vi.FileDescription : displayName;
                }
                catch { /* stick with file name otherwise */ }
            }

            // Pull the icon
            Icon icon = IconHelper.GetIconForProcess(pid);
            string key = string.IsNullOrEmpty(exePath) ? "generic" : exePath.ToLowerInvariant();

            // Add icon to ImageList
            if (!imageListSelectProc.Images.ContainsKey(key))
                imageListSelectProc.Images.Add(key, icon.ToBitmap());

            // Create and add ListViewItem
            exePath = exePath ?? proc.ProcessName;  // if exePath is null, fallback on process name
            string exeName = Path.GetFileName(exePath) ?? proc.ProcessName; // if exeName fails because exePath is a process name, fallback on process name
            var item = new ListViewItem(displayName)
            {
                ImageKey = key,
                Tag = exeName.ToLowerInvariant()
            };
            lvSessions.Items.Add(item);
        }

        /// <summary>
        /// Initializes the ProcessSelectForm form and populates the ListView with the current active sessions.
        /// </summary>
        /// <param name="sessionManager"></param>
        /// <param name="sessions"></param>
        public ProcessSelectForm(
            AudioSessionManager sessionManager,
            List<AudioSessionControl> sessions
        )
        {
            InitializeComponent();

            // Populate the ListView with processes and their respective icons
            foreach (var s in sessions)
            {
                Process proc = GetProcessSafe(s.GetProcessID);
                if (proc == null) continue;

                // Get the processes name and icon and add to the ListView
                AddListViewItem(proc);
            }

            btnSelect.Enabled = false;
            // Whenever an item is selected or deselected, run the lambda function -> enable/disable select button
            lvSessions.SelectedIndexChanged += (s, e) =>
            {
                btnSelect.Enabled = (lvSessions.SelectedItems.Count > 0);
            };
        }

        /// <summary>
        /// Gets Process by pid and bypasses ArgumentException; returning null instead.
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        private Process GetProcessSafe(uint pid)
        {
            try { return Process.GetProcessById((int)pid); }
            catch (ArgumentException) { return null; }
        }

        /// <summary>
        /// Saves selected process in "SelectedProcess" public variable and closes the form.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSelect_Click(object sender, EventArgs e)
        {
            // Disable the "Select" button if no process is selected from the list
            if (lvSessions.SelectedItems[0] == null) return;
            // Extract selected item process name and pass to public variable
            SelectedProcess = lvSessions.SelectedItems[0];
            DialogResult = DialogResult.OK;
            // Close the form after selecting the process
            Close();
        }

        /// <summary>
        /// Closes the form without doing anything.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
