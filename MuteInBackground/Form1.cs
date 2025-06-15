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

using Microsoft.Win32;
using System.IO;

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

        // WinEvent constants for foreground window check
        private const uint EVENT_SYSTEM_FOREGROUND = 0x0003;
        private const uint WINEVENT_OUTOFCONTEXT = 0;
        private const uint OBJID_WINDOW = 0;
        private const uint WINEVENT_SKIPOWNPROCESS = 0X0002;    // used so that focusing on this application does not mute monitored applications

        // Delegate signature for the WinEvent callback
        private delegate void WinEventDelegate(
            IntPtr  hWinEventHook,
            uint    eventType,
            IntPtr  hwnd,
            int     idObject,
            int     idChild,
            uint    dwEventThread,
            uint    dwmsEventTime
        );

        [DllImport("user32.dll")]
        private static extern IntPtr SetWinEventHook(
            uint eventMin,
            uint eventMax,
            IntPtr hmodWinEventProc,
            WinEventDelegate lpfnWinEventProc,
            uint idProcess,
            uint idThread,
            uint dwFlags
        );

        [DllImport("user32.dll")]
        private static extern bool UnhookWinEvent( IntPtr hWinEventHook );

        private WinEventDelegate    _winDelegate; // keeps callback alive
        private IntPtr              _winHook;   // hook handle needed to unhook

        private MMDeviceEnumerator deviceEnum;
        private MMDevice defaultDevice;
        private AudioSessionManager sessionManager;

        private List<string> mutedProcessNames;     // list of process names to be monitored
        private string lastForegroundMonitored = null;  // used to track the previous foreground app to stop repeated COM calls which causes stuttering audio

        public Form1()
        {
            InitializeComponent();
            mutedProcessNames = new List<string>();
            InitAudio();

            // Read stored RunAtStartup for consistency
            if (Properties.Settings.Default.RunAtStartup)
                StartupHelper.UpdateStartupShortcut(true);
        }

        // Callback method Win32 calls
        private void WinEventProc(
            IntPtr hWinEventHook,
            uint eventType,
            IntPtr hwnd,
            int idObject,
            int idChild,
            uint dwEventThread,
            uint dwmsEventTime
        )
        {
            // Ignore all events unless top window change
            if (idObject != OBJID_WINDOW) return;

            BeginInvoke(new Action(() => HandleForegroundChange(hwnd)));
        }

        private static readonly HashSet<string> _skipProcs = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "explorer",
                "ShellExperienceHost",
                "SearchUI"
            };

        private void HandleForegroundChange(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero) return;

            // Get process ID of the window
            GetWindowThreadProcessId(hwnd, out uint pid);
            if (pid == 0) return;

            // Get process name from pid
            string procName;
            try { procName = Process.GetProcessById((int)pid).ProcessName.ToLowerInvariant(); }
            catch { return; }   // process exited before we can get its name

            // Ignore any OS shell windows
            if (_skipProcs.Contains(procName))
                return;

            // Do nothing if window was handled already and nothing changed
            if (procName == lastForegroundMonitored) return;

            lastForegroundMonitored = procName;

            // If the foreeground process is in the monitored list, unmute it and mute all the others
            if (mutedProcessNames.Contains(procName))
            {
                UnmuteProcessAudio(procName);
                lblStatus.Text = $"Unmuted: {procName}";
                // Mute all other monitored background processes
                foreach (var backgroundProc in mutedProcessNames.Where(n => n != procName))
                    MuteProcessAudio(backgroundProc);
            }
            // Otherwise mute all monitored apps
            else
            {
                foreach (var procs in mutedProcessNames)
                    MuteProcessAudio(procs);
                lblStatus.Text = (mutedProcessNames.Count > 0) ? $"Muting {string.Join(", ", mutedProcessNames)}" : "No apps to mute";
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
        /// UnmuteAllMonitoredApps restores all audio by unmuting all monitored sessions.
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
                // Create and store delegate instance so it isn't garbage collected
                _winDelegate = new WinEventDelegate(WinEventProc);

                // Install hook so WinEventProc() is called whenever EVENT_SYSTEM_FOREGROUND fires
                _winHook = SetWinEventHook(
                    EVENT_SYSTEM_FOREGROUND,
                    EVENT_SYSTEM_FOREGROUND,
                    IntPtr.Zero,
                    _winDelegate,   // callback
                    0,
                    0,
                    WINEVENT_OUTOFCONTEXT | WINEVENT_SKIPOWNPROCESS
                );

                // Force call to handler to be in a known state immediately after enabling
                HandleForegroundChange(GetForegroundWindow());

                lblStatus.Text = "Auto-Mute ENABLED";
            }
            else
            {
                // Remove hook
                if (_winHook != IntPtr.Zero)
                {
                    UnhookWinEvent(_winHook);
                    _winHook = IntPtr.Zero;
                }

                // Restore all app volumes
                UnmuteAllMonitoredApps();

                lblStatus.Text = "Auto-Mute DISABLED";
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            // Initialize new temporary enumerator and session manager to get up-to-date sessions
            var tempEnum = new MMDeviceEnumerator();
            var tempDevice = tempEnum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            var tempManager = tempDevice.AudioSessionManager;

            // Filter and take snapshot of current sessions
            var sessionCollection = tempManager.Sessions;
            var sessions = new List<AudioSessionControl>(sessionCollection.Count);
            for (int i = 0; i < sessionCollection.Count; i++)
            {
                sessions.Add(sessionCollection[i]);
            }
            var activeSessions = sessions
                .Where(s => s.State != AudioSessionState.AudioSessionStateExpired && s.GetProcessID != 0)
                .ToList();

            // Show process selection form with only active audio sessions
            using (var dlg = new ProcessSelectForm(sessionManager, activeSessions))
            {
                // Create handler and subscribe to SessionManager.OnSessionCreated before showing dialog
                AudioSessionManager.SessionCreatedDelegate sessionCreatedHandler = (s, newSession) =>
                {
                    Invoke((Action)( () => 
                    {
                        dlg.AddSessionControl(newSession);
                    })
                    );
                };
                sessionManager.OnSessionCreated += sessionCreatedHandler;

                // Show the dialog
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    // Get the selected process and force lowercase
                    string selected = dlg.SelectedProcessName.ToLowerInvariant();
                    // Avoid adding duplicates to the monitored apps list and form UI
                    if (!mutedProcessNames.Contains(selected))
                    {
                        mutedProcessNames.Add(selected);
                        lstApps.Items.Add(selected);
                    }
                }

                // Unsubscribe from event after dialog is closed
                sessionManager.OnSessionCreated -= sessionCreatedHandler;
            }
            // Dispose of temporary audio objects before exiting
            tempDevice.Dispose();
            tempEnum.Dispose();
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            // Do nothing if no app is selected
            if (lstApps.SelectedItem == null) return;

            // Unmute immediately before removing from monitored apps list and form UI
            string procName = lstApps.SelectedItem.ToString();
            UnmuteProcessAudio(procName);

            mutedProcessNames.Remove(procName);
            lstApps.Items.Remove(procName);
        }

        /// <summary>
        /// Override OnFormClosing to check for "minimize on close"
        /// and additionally clean up the hook when the form closes.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Check for minimize on close 
            bool minimizeOnClose = Properties.Settings.Default.MinimizeOnClose;
            // If user checked "Minimize On Close" AND user clicks 'X' button
            if (minimizeOnClose && e.CloseReason == CloseReason.UserClosing)
            {
                // Cancel closing the form, hide the window, show application icon in tray
                e.Cancel = true;
                this.Hide();
                notifyIcon1.Visible = true;
                return;
            }

            // Undo the hook so Windows stops calling
            if (_winHook != IntPtr.Zero)
            {
                UnhookWinEvent(_winHook);
                _winHook = IntPtr.Zero;
            }
            // Call base so any other cleanup can run
            base.OnFormClosing(e);
        }

        /// <summary>
        /// GetProcessNameSafe takes pid and returns a process name if valid. 
        /// Otherwise bypass ArgumentException and return null.
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        private string GetProcessNameSafe(uint pid)
        {
            try { return Process.GetProcessById((int)pid).ProcessName; }
            catch (ArgumentException) { return null; }
        }

        /// <summary>
        /// tsmShow_Click shows the application when the user selects the "Show" option in the context menu
        /// shown by right-clicking the tray icon when the application is minimized. 
        /// It is a ToolStripMenuItem.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsmShow_Click(object sender, EventArgs e)
        {
            // Unhide window and hide tray icon
            this.Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
        }

        /// <summary>
        /// tsmExit_Click closes the application when the user selects the "Exit" option in the context menu
        /// shown by right-clicking the tray icon when the application is minimized. 
        /// It is a ToolStripMenuItem.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsmExit_Click(object sender, EventArgs e)
        {
            notifyIcon1.Visible = false;    // hide tray icon
            Application.Exit();             // fully close the app
        }

        /// <summary>
        /// notifyicon1_DoubleClick shows the application when the user double-clicks the tray icon.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            // Unhide window and hide tray icon
            this.Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
        }

        /// <summary>
        /// btnSettings_Click shows the SettingsForm dialog when the user clicks the settings button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSettings_Click(object sender, EventArgs e)
        {
            using (var dlg = new SettingsForm())
            {
                dlg.ShowDialog();
            }
        }

        /// <summary>
        /// Debug method to show all sessions
        /// </summary>
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

    }

    /// <summary>
    /// StartupHelper is used to add or remove app from Windows registry key to automatically run on startup.
    /// </summary>
    static class StartupHelper
    {
        // Registery path under HKEY_CURRENT_USER (HKCU) for startup apps
        private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";

        private const string AppName = "MuteInBackground";

        /// <summary>
        /// UpdateStartupShortcut adds or removes the app from startup.
        /// </summary>
        /// <param name="enable"></param>
        public static void UpdateStartupShortcut(bool enable)
        {
            // Open HKCU key settings with write acess. Will automatically block and dispose when done.
            using (var key = Registry.CurrentUser.OpenSubKey(RunKey, true))
            {
                // Enbaling run on launch
                if (enable)
                {
                    // Full path to this app's executable
                    string exePath = Application.ExecutablePath;
                    // Write new string under Run key so Windows will launch at login
                    key.SetValue(AppName, $"\"{exePath}\"");
                }
                // Disabling run on launch
                else
                {
                    // Delete the entry if it exists and don't throw an exception if missing entry
                    key.DeleteValue(AppName, false);
                }
            }
        }
    }
}
