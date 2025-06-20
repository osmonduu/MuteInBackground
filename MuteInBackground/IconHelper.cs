using Microsoft.Win32;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MuteInBackground
{
    /// <summary>
    /// Gets the full filesystem path for a running process' executable and its associated icon.
    /// </summary>
    internal static class IconHelper
    {
        // OpenProcess -> Opens an existing local process object; returns process handle
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr OpenProcess(
            int dwDesiredAccess,    // process access rights we want (PROCESS_QUERY_LIMITED_INFORMATION)
            bool bInheritHandle,    // whether child processes should inherit handle 
            int dwProcessId         // pid of process we are opening
        );

        // Used for OpenProcess' 'dwDesiredAcess' parameter
        const int PROCESS_QUERY_LIMITED_INFORMATION = 0x1000;

        // QueryfullProcessImageName -> Given process handle, write full .exe path into buffer
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool QueryFullProcessImageName(
            IntPtr hProcess,            // output process handle from OpenProcess
            int dwFlags,                // flags for path format
            StringBuilder lpExeName,    // string buffer
            ref int lpdwSize            // input: buffer size, output: returned actual string length
        );

        // CloseHandle -> Free process handle from OpenProcess
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CloseHandle(IntPtr hObject);

        /// <summary>
        /// Open process by pid and find its full .exe path. Returns null if can't open process handle.
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public static string GetExecutablePath(int pid)
        {
            // Open the process with limited rights with pid
            IntPtr hProc = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, false, pid);
            if (hProc == IntPtr.Zero) return null;  // if can't read, return null

            // Allocate 1KB buffer and call QueryFullPrcesImageName to get full path
            try
            {
                var sb = new StringBuilder(1024);
                int size = sb.Capacity;
                if (QueryFullProcessImageName(hProc, 0, sb, ref size))
                    return sb.ToString();
            }
            // Always close the handle 
            finally { CloseHandle(hProc); }

            return null;
        }

        // SHGetFileInfo -> Gets information about object in the file system, such as file, folder, directory, or drive root.
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        static extern IntPtr SHGetFileInfo(
            string pszPath,         // null-terminated string containing path and file name
            uint dwFileAttributes,  // file attribute flags
            ref SHFILEINFO psfi,    // pointer to SHFILEINFO struct to receive file information    
            uint cbFileInfo,        // size, in bytes, of psfi
            uint uFlags             // flags to specify file information to recieve
        );

        // Used for SHGetFileInfo's 'uFlags' parameter 
        const uint SHGFI_ICON = 0x100;
        const uint SHGFI_LARGEICON = 0x0;   // use 0x1 for small icon

        // SHFILEINFO -> contains information about a file object; populated by SHGetFileInfo
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct SHFILEINFO {
            public IntPtr   hIcon;          // handle to icon that represents file
            public int      iIcon;          // index of icon image within system image list
            public uint     dwAttributes;   // attributes of file object
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string   szDisplayName;  // name of file as it appears in Windows Shell
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string   szTypeName;     // describes type of file
        }

        // DestroyIcon -> free icon handle from SHFILEINFO struct 
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool DestroyIcon(IntPtr hIcon);

        public static Icon GetIconForProcess(int pid)
        {
            // Get process filepath
            string path = GetExecutablePath(pid);
            if (!string.IsNullOrEmpty(path))
            {
                // Get file info with icon handler and data
                var shfi = new SHFILEINFO();
                IntPtr h = SHGetFileInfo(
                    path,
                    0,
                    ref shfi,
                    (uint)Marshal.SizeOf(shfi),
                    SHGFI_ICON | SHGFI_LARGEICON
                );
                // Extract, cast, and clone the icon from the handler
                if (h != IntPtr.Zero)
                {
                    var ico = (Icon)Icon.FromHandle(shfi.hIcon).Clone();
                    // Make sure to dispose of the original handler afterwards
                    DestroyIcon(shfi.hIcon);
                    return ico;
                }
            }
            // Fallback if anything failed
            return SystemIcons.Application;
        }

    }
}
