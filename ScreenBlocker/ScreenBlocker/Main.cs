using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ScreenBlocker
{
    public partial class Main : Form
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowDisplayAffinity(IntPtr hWnd, int dwAffinity);

        private const int WDA_MONITOR = 1; 
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        public Main()
        {
            InitializeComponent();
            SetWindowDisplayAffinityForAllMonitors();
            _hookID = SetHook(_proc);
        }

        private void Main_Load(object sender, EventArgs e)
        {
            
        }

        private void SetWindowDisplayAffinityForAllMonitors()
        {
            try
            {
                foreach (Screen screen in Screen.AllScreens)
                {
                    SetWindowDisplayAffinity(this.Handle, WDA_MONITOR);
                }
            }
            catch
            {
                
            }
        }

        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (var curProcess = System.Diagnostics.Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN))
            {
                int vkCode = Marshal.ReadInt32(lParam);
                if (vkCode == 44)
                {
                    return (IntPtr)1;
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private delegate IntPtr LowLevelKeyboardProc(
            int nCode, IntPtr wParam, IntPtr lParam);

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            UnhookWindowsHookEx(_hookID);
            base.OnFormClosed(e);
        }
    }
}
