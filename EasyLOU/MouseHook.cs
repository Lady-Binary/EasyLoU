using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace EasyLOU
{
    public enum MouseEventType
    {
        NONE,
        LEFT,
        RIGHT,
        WHEEL
    }

    public enum MouseScrollType
    {
        UP = 1,
        DOWN = -1
    }

    public delegate bool MouseEventCallback(MouseEventType type, int x, int y);
    public delegate bool MouseScrollEventCallback(MouseScrollType type);
    public class MouseHook
    {
        internal const int WH_MOUSE_LL = 14;
        internal const int WM_LBUTTONDOWN = 0x0201;
        internal const int WM_RBUTTONDOWN = 0x0204;

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        public delegate IntPtr LowLevelProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelProc lpfn, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll")]
        internal static extern bool UnhookWindowsHookEx(IntPtr hhk);
        [DllImport("user32.dll")]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("kernel32.dll")]
        internal static extern IntPtr GetModuleHandle(string lpModuleName);
        [DllImport("user32.dll")]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, int dwData, int dwExtraInfo);
        [DllImport("user32.dll")]
        public static extern IntPtr WindowFromPoint(POINT Point);
        [DllImport("User32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);
        [DllImport("User32.dll")]
        public static extern int GetWindowTextLength(IntPtr hWnd);
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        [DllImport("user32.dll")]
        public static extern IntPtr CopyIcon(IntPtr hIcon);
        [DllImport("user32.dll")]
        public static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);
        [DllImport("user32.dll")]
        public static extern IntPtr LoadCursorFromFile(string lpFileName);
        [DllImport("user32.dll")]
        public static extern bool SetSystemCursor(IntPtr hcur, uint id);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SystemParametersInfo(int uiAction, int uiParam, IntPtr pvParam, int fWinIni);

        private static IntPtr _hookID = IntPtr.Zero;

        private static LowLevelProc _proc;

        public static event MouseEventCallback MouseDown;

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            int intw = (int)wParam;
            if (nCode >= 0 &&
                intw == WM_LBUTTONDOWN || intw == WM_RBUTTONDOWN)
            {
                var hookStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
                int x = hookStruct.pt.x, y = hookStruct.pt.y;
                bool res = true;
                switch ((int)wParam)
                {
                    case WM_LBUTTONDOWN:
                        res = MouseDown?.Invoke(MouseEventType.LEFT, x, y) ?? true;
                        break;
                    case WM_RBUTTONDOWN:
                        res = MouseDown?.Invoke(MouseEventType.RIGHT, x, y) ?? true;
                        break;
                }
                if (!res)
                    return (IntPtr)1;
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        public static bool HookStart()
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                var handle = GetModuleHandle("user32");
                _proc = HookCallback;
                _hookID = SetWindowsHookEx(WH_MOUSE_LL, _proc, handle, 0);
                return _hookID != IntPtr.Zero;
            }
        }

        public static void HookEnd()
        {
            UnhookWindowsHookEx(_hookID);
        }
    }
}