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
    
    public partial class MuteInBackgroundForm : Form
    {
        // GetForegroundWindow -> returns HWND of the active window
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        // GetwindowThreadProcessId -> returns thread ID, outputs process ID
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

        public MuteInBackgroundForm()
        {
            InitializeComponent();
            mutedProcessNames = new List<string>();
            InitAudio();

            // Listen for any new audio sessions
            sessionManager.OnSessionCreated += SessionManager_OnSessionCreated;

            // Read stored RunAtStartup for consistency
            if (Properties.Settings.Default.RunAtStartup)
                StartupHelper.UpdateStartupShortcut(true);
        }

        private void SessionManager_OnSessionCreated(object sender, IAudioSessionControl newSession)
        {
            // Wrap IAudioSessionControl to get IAudioSessionControl2 functionality
            var wrapper = new AudioSessionControl(newSession);
            int pid = (int)wrapper.GetProcessID;
            if (pid == 0) return;

            string procName;
            try { procName = Process.GetProcessById(pid).ProcessName.ToLowerInvariant(); }
            catch { return; }

            if ((mutedProcessNames.Contains(procName)) && (GetCurrentForegroundProcessName() != procName))
            {
                BeginInvoke((Action)(() => wrapper.SimpleAudioVolume.Mute = true));
            }
        }

        private string GetCurrentForegroundProcessName()
        {
            // Get foreground window handle and get its pid
            IntPtr hwnd = GetForegroundWindow();
            GetWindowThreadProcessId(hwnd, out uint pid);
            if (pid == 0) return null;

            // Get process name from pid
            string procName = null;
            try { procName = Process.GetProcessById((int)pid).ProcessName.ToLowerInvariant(); }
            catch { }

            return procName;
        }


        /// <summary>
        /// Callback method Win32 calls whenever the foreground window changes.
        /// </summary>
        /// <param name="hWinEventHook"></param>
        /// <param name="eventType"></param>
        /// <param name="hwnd"></param>
        /// <param name="idObject"></param>
        /// <param name="idChild"></param>
        /// <param name="dwEventThread"></param>
        /// <param name="dwmsEventTime"></param>
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
            // P/Invoke HandleForegroundChange in order to work in UI thread and access UI variables.
            BeginInvoke(new Action(() => HandleForegroundChange(hwnd)));
        }

        private static readonly HashSet<string> _skipProcs = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "explorer",
                "ShellExperienceHost",
                "SearchUI"
            };

        /// <summary>
        /// Gets the foreground window process and mutes/unmutes
        /// the process based on if it is on the monitored apps list (mutedProcessNames).
        /// </summary>
        /// <param name="hwnd"></param>
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
        /// Initiates all the objects used to interact with windows audio API.
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
        /// Mutes the process by name.
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
        /// Unmutes the process by name.
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
        /// Restores all audio by unmuting all monitored sessions.
        /// </summary>
        private void UnmuteAllMonitoredApps()
        {
            foreach (string name in mutedProcessNames)
                UnmuteProcessAudio(name);
        }

        /// <summary>
        /// When checked, install windows event hook to call WinEventProc() whenever the foreground window changes.
        /// Otherwise, remove the hook if it exists and unmute all the apps that were previously monitored.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkEnableAutoMute_CheckedChanged(object sender, EventArgs e)
        {
            if (chkEnableAutoMute.Checked)
            {
                // Create and store delegate instance in variable so it isn't garbage collected
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

        /// <summary>
        /// User chooses an application from ProcessSelectForm and it is added to the list of monitored apps.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

            // Show process selection form with only up-to-date audio sessions
            using (var dlg = new ProcessSelectForm(tempManager, activeSessions))
            {
                // Create handler and subscribe to SessionManager.OnSessionCreated before showing dialog
                AudioSessionManager.SessionCreatedDelegate dialogHandler = (s, newSession) =>
                {
                    Invoke((Action)( () => 
                    {
                        dlg.AddSessionControl(newSession);
                    })
                    );
                };
                sessionManager.OnSessionCreated += dialogHandler;

                // Show the dialog
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    // Get the selected process from the dialog
                    ListViewItem selectedItem = dlg.SelectedProcess;
                    // Avoid adding duplicates to the monitored apps list and form UI
                    if (!mutedProcessNames.Contains(selectedItem.Text))
                    {
                        // Add to monitored apps list and immediately mute
                        mutedProcessNames.Add(selectedItem.Tag.ToString());
                        MuteProcessAudio(selectedItem.Tag.ToString());

                        // Copy any new items from the dlg.ImageListSelectProc to ImageListMain
                        foreach (string key in dlg.SessionImageList.Images.Keys)
                        {
                            if (!imageListMain.Images.ContainsKey(key))
                                imageListMain.Images.Add(key, dlg.SessionImageList.Images[key]);
                        }
                        // Clone the ListViewItem so it isn't owned by dlg.lvSessions
                        var cloneLVItem = (ListViewItem)selectedItem.Clone();
                        lvApps.Items.Add(cloneLVItem);
                    }
                }
                // Unsubscribe from event after dialog is closed
                sessionManager.OnSessionCreated -= dialogHandler;
            }
            // Dispose of temporary audio objects before exiting
            tempDevice.Dispose();
            tempEnum.Dispose();
        }

        /// <summary>
        /// Removes the selected item from the UI list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRemove_Click(object sender, EventArgs e)
        {
            // Do nothing if no app is selected
            if (lvApps.SelectedItems.Count == 0) return;

            // Unmute immediately before removing from monitored apps list and form UI
            ListViewItem lvItem = lvApps.SelectedItems[0];
            string procName = lvItem.Tag.ToString();
            UnmuteProcessAudio(procName);

            mutedProcessNames.Remove(procName);
            lvApps.Items.Remove(lvItem);
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

            // Unmute all monitored processes before closing
            UnmuteAllMonitoredApps();

            // Undo the hook so Windows stops calling
            if (_winHook != IntPtr.Zero)
            {
                UnhookWinEvent(_winHook);
                _winHook = IntPtr.Zero;
            }

            // Clean up audio session subscription
            sessionManager.OnSessionCreated -= SessionManager_OnSessionCreated;
            // Call base so any other cleanup can run
            base.OnFormClosing(e);
        }

        /// <summary>
        /// Takes pid and returns a process name if valid; otherwise bypass ArgumentException and return null.
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        private string GetProcessNameSafe(uint pid)
        {
            try { return Process.GetProcessById((int)pid).ProcessName; }
            catch (ArgumentException) { return null; }
        }

        /// <summary>
        /// Shows the application when the user selects the "Show" option in the context menu
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
        /// Closes the application when the user selects the "Exit" option in the context menu
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
        /// Shows the application when the user double-clicks the tray icon.
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
        /// Shows the SettingsForm dialog when the user clicks the settings button.
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
    /// Used to add or remove app from Windows registry key to automatically run on startup.
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
