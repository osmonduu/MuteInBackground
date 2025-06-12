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

        // WinEvent constants for foreground window check
        private const uint EVENT_SYSTEM_FOREGROUND = 0x0003;
        private const uint WINEVENT_OUTOFCONTEXT = 0;
        private const uint OBJID_WINDOW = 0;
        private const uint WINEVENT_SKIPOWNPROCESS = 0X0002;

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

        private List<string> mutedProcessNames;     // List of process names to auto-mute
        private string lastForegroundMonitored = null;  // Used to track the foreground app to stop repeated COM calls

        public Form1()
        {
            InitializeComponent();
            mutedProcessNames = new List<string>();
            InitAudio();
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

            string procName;
            try { procName = Process.GetProcessById((int)pid).ProcessName.ToLowerInvariant(); }
            catch { return; }   // process exited before we can get its name

            if (_skipProcs.Contains(procName))
                return;    // ignore any OS shell windows

            // Do nothing if window was handled already and nothing changed
            if (procName == lastForegroundMonitored) return;

            lastForegroundMonitored = procName;

            // If the process is in the monitored list, unmute it and mute all the others
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
                // Create delegate instance so it isn't garbage collected
                _winDelegate = new WinEventDelegate(WinEventProc);

                // Install hook so WinEventProc() is called whenever EVENT_SYSTEM_FOREGROUND happens
                _winHook = SetWinEventHook(
                    EVENT_SYSTEM_FOREGROUND,
                    EVENT_SYSTEM_FOREGROUND,
                    IntPtr.Zero,
                    _winDelegate,   // callback
                    0,
                    0,
                    WINEVENT_OUTOFCONTEXT | WINEVENT_SKIPOWNPROCESS
                );

                // Force call to handler once to be in a known state
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

        // Clean up the hook when the form closes
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Undo the hook so Windows stops calling
            if (_winHook != IntPtr.Zero)
                UnhookWinEvent(_winHook);
            base.OnFormClosing(e);
        }

        // DEBUG methods
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
        // DEBUG methods
        private string GetProcessNameSafe(uint pid)
        {
            try { return Process.GetProcessById((int)pid).ProcessName; }
            catch (ArgumentException) { return null; }
        }
    }
}
