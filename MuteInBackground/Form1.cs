using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;

namespace MuteInBackground
{
    public partial class Form1 : Form
    {
        // GetForegroundWindow() -> returns HWND of the active window
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        // GetwindowThreadProcessId(hwnd, out processId) -> returns thread ID, outputs process ID
        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        private MMDeviceEnumerator deviceEnum;
        private MMDevice defaultDevice;
        private AudioSessionManager sessionManager;

        private Timer focusTimer;                   // WinForms Timer for polling rate
        private List<string> mutedProcessNames;     // List of process names to auto-mute
        private string lastForegroundMonitored = null;  // Used to track the foreground app to stop repeated COM calls

        public Form1()
        {
            InitializeComponent();
            mutedProcessNames = new List<string>();
            InitAudio();
        }

        /// <summary>
        /// StartFocusPolling calls OnFocusCheck every 500ms.
        /// </summary>
        private void StartFocusPolling()
        {
            if (focusTimer == null)
            {
                focusTimer = new Timer();
                focusTimer.Interval = 500;
                focusTimer.Tick += OnFocusCheck;
            }
            focusTimer.Start();
        }

        /// <summary>
        /// StopFocuPolling stops the focusTimer.
        /// </summary>
        private void StopFocusPolling()
        {
            focusTimer?.Stop();
        }

        /// <summary>
        /// OnFocusCheck gets the current forground app and mutes/unmutes it depending if it is on the monitored apps list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFocusCheck(object sender, EventArgs e)
        {
            // Get window currently in the foreground
            IntPtr hwnd = GetForegroundWindow();
            if (hwnd == IntPtr.Zero) return;    // no window is found

            // Get the process ID from the HWND
            GetWindowThreadProcessId(hwnd, out uint pid);
            if (pid == 0) return;

            // Convert the pid to a process name
            string procName;
            try { procName = Process.GetProcessById((int)pid).ProcessName.ToLowerInvariant(); }
            catch { return; } // process might have exited

            //DEBUG
            lblDebug.Text = string.Join(", ", mutedProcessNames);

            bool isMonitored = mutedProcessNames.Contains(procName);

            // Skip is foreground app is monitored and stayed the same
            if (isMonitored && procName == lastForegroundMonitored) return;
            // Skip if foreground app is not monitored and all monitored apps are already muted
            if (!isMonitored && lastForegroundMonitored == null) return;

            // Update foreground state
            lastForegroundMonitored = isMonitored ? procName : null;

            if (isMonitored)
            {
                UnmuteProcessAudio(procName);
                lblStatus.Text = $"Unmuted: {procName}";
                // Any other process that is not in the foreground (background), mute it
                foreach (var other in mutedProcessNames.Where(n => n != procName))
                    MuteProcessAudio(other);
            }
            else
            {
                // If foreground is not monitored, mute all the monitored apps
                foreach (var procToMute in mutedProcessNames)
                    MuteProcessAudio(procToMute);
                // Update "Status" label
                lblStatus.Text = mutedProcessNames.Count > 0 ? $"Muting: {string.Join(", ", mutedProcessNames)}" : "No apps to mute";
            }
        }

        /// <summary>
        /// InitAudio initiates all the objects used to interact with windows audio API.
        /// </summary>
        private void InitAudio()
        {
            // Create enumerator
            deviceEnum = new MMDeviceEnumerator();
            // Get the default playback device (DataFlow.Render) for multimedia
            defaultDevice = deviceEnum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            // Get the AudioSessionManager from the device
            sessionManager = defaultDevice.AudioSessionManager;
        }

        /// <summary>
        /// MuteProcessAudio mutes the process by name.
        /// </summary>
        /// <param name="processName"></param>
        private void MuteProcessAudio(string processName)
        {
            if (string.IsNullOrWhiteSpace(processName)) return;

            var sessions = sessionManager.Sessions; // SessionCollection of AudioSessionControl
            for (int i = 0; i < sessions.Count; i++)
            {
                AudioSessionControl sess = sessions[i];
                using (sess)
                {
                    // Read PID of this session
                    int sessPid = (int)sess.GetProcessID;
                    if (sessPid == 0) continue;   // skip system sound session

                    string p;
                    try { p = Process.GetProcessById(sessPid).ProcessName.ToLowerInvariant(); }
                    catch { continue; }   // continue if process has exited

                    // Mute the process if it is unmuted
                    if (p.Equals(processName, StringComparison.OrdinalIgnoreCase) && !sess.SimpleAudioVolume.Mute)
                        sess.SimpleAudioVolume.Mute = true;
                }
            }
        }

        /// <summary>
        /// UnmuteProcessAudio unmutes the process by name.
        /// </summary>
        /// <param name="processName"></param>
        private void UnmuteProcessAudio(string processName)
        {
            if (string.IsNullOrWhiteSpace(processName)) return;

            var sessions = sessionManager.Sessions;
            for (int i = 0; i < sessions.Count; i++)
            {
                AudioSessionControl sess = sessions[i];
                using (sess)
                {
                    int sessPid = (int)sess.GetProcessID;
                    if (sessPid == 0) continue;   // skip system sound session

                    string p;
                    try { p = Process.GetProcessById(sessPid).ProcessName.ToLowerInvariant(); }
                    catch { continue; }  // continue if process has exited

                    // Unmute the process if it is muted
                    if (p.Equals(processName, StringComparison.OrdinalIgnoreCase) && sess.SimpleAudioVolume.Mute)
                        sess.SimpleAudioVolume.Mute = false;
                }
            }
        }

        /// <summary>
        /// UnmuteAllMonitoredApps restores all audio by unmuting all muted sessions.
        /// </summary>
        private void UnmuteAllMonitoredApps()
        {
            foreach (string name in mutedProcessNames)
                UnmuteProcessAudio(name);
        }

        private void chkEnableAutoMute_CheckedChanged(object sender, EventArgs e)
        {
            if (chkEnableAutoMute.Checked)
            {
                StartFocusPolling();
                lblStatus.Text = "Auto-Mute ENABLED";
            }
            else
            {
                StopFocusPolling();
                UnmuteAllMonitoredApps();
                lblStatus.Text = "Auto-Mute DISABLED";
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            // Grab only active audio sessions with real PID
            var sessionCollection = sessionManager.Sessions;
            var sessions = new List<AudioSessionControl>(sessionCollection.Count);
            for (int i = 0; i < sessionCollection.Count; i++)
            {
                sessions.Add(sessionCollection[i]);
            }
            var activeSessions = sessions
                .Where(s => s.State != AudioSessionState.AudioSessionStateExpired && s.GetProcessID != 0)
                .ToList();

            // Show process selection form with only active audio sessions
            using (var dlg = new ProcessSelectForm(activeSessions))
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    // Get the selected process and force lowercase
                    string selected = dlg.SelectedProcessName.ToLowerInvariant();
                    // Avoid adding duplicated to the monitored apps and process list
                    if (!mutedProcessNames.Contains(selected))
                    {
                        mutedProcessNames.Add(selected);
                        lstApps.Items.Add(selected);
                    }
                }
            }
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            // Do nothing if no app is selected
            if (lstApps.SelectedItem == null) return;

            string procName = lstApps.SelectedItem.ToString();
            // Unmute immediately before removing from both lists
            UnmuteProcessAudio(procName);

            mutedProcessNames.Remove(procName);
            lstApps.Items.Remove(procName);
        }

        private void DumpAllSessions()
        {
            var sb = new StringBuilder();
            var raw = sessionManager.Sessions;
            for (int i = 0; i < raw.Count; i++)
            {
                var s = raw[i];
                string procName = s.GetProcessID != 0 ? GetProcessNameSafe(s.GetProcessID) : "(system)";

                sb.AppendLine(
                    $"#{i}: PID = {s.GetProcessID,-6} " +
                    $"State = {s.State,-8} " +
                    $"Name = {procName,-20}" +
                    $"Name = \"{s.DisplayName}\"");
            }
            MessageBox.Show(sb.ToString(), "All Audio Sessions");
        }

        private string GetProcessNameSafe(uint pid)
        {
            try { return Process.GetProcessById((int)pid).ProcessName; }
            catch (ArgumentException) { return null; }
        }
    }
}
