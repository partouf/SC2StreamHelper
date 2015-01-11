using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SC2StreamHelper
{
    class RemoteKeyboardInput
    {
        //  source: http://www.codeproject.com/Questions/279641/c-sharp-DirectInput-Send-Key

        [DllImport("user32.dll")]
        static extern UInt32 SendInput(UInt32 nInputs, [MarshalAs(UnmanagedType.LPArray, SizeConst = 1)] INPUT[] pInputs, Int32 cbSize);

        [StructLayout(LayoutKind.Sequential)]
        struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public int mouseData;
            public int dwFlags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct KEYBDINPUT
        {
            public short wVk;
            public short wScan;
            public int dwFlags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct HARDWAREINPUT
        {
            public int uMsg;
            public short wParamL;
            public short wParamH;
        }

        [StructLayout(LayoutKind.Explicit)]
        struct INPUT
        {
            [FieldOffset(0)]
            public int type;
            [FieldOffset(4)]
            public MOUSEINPUT mi;
            [FieldOffset(4)]
            public KEYBDINPUT ki;
            [FieldOffset(4)]
            public HARDWAREINPUT hi;
        }

        public const int KEYEVENTF_EXTENDEDKEY = 0x0001;
        public const int KEYEVENTF_KEYDOWN = 0x0000;
        public const int KEYEVENTF_KEYUP = 0x0002;
        public const int KEYEVENTF_UNICODE = 0x0004;
        public const int KEYEVENTF_SCANCODE = 0x0008;

        public const short VK_SHIFT = 0x10;
        public const short VK_CONTROL = 0x11;


        public void Send_SingleKey(short Keycode, int KeyUporDown)
        {
            INPUT[] InputData = new INPUT[1];

            InputData[0].type = 1;
            InputData[0].ki.wScan = Keycode;
            InputData[0].ki.dwFlags = KeyUporDown;
            InputData[0].ki.time = 0;
            InputData[0].ki.wVk = 0;
            InputData[0].ki.dwExtraInfo = IntPtr.Zero;

            SendInput(1, InputData, Marshal.SizeOf(typeof(INPUT)));
        }

        public void Send_NormalKeyPress(short Keycode)
        {
            INPUT[] InputData = new INPUT[2];

            InputData[0].type = 1;
            InputData[0].ki.wScan = 0;
            InputData[0].ki.dwFlags = KEYEVENTF_KEYDOWN;
            InputData[0].ki.time = 0;
            InputData[0].ki.wVk = Keycode;
            InputData[0].ki.dwExtraInfo = IntPtr.Zero;

            InputData[1].type = 1;
            InputData[1].ki.wScan = 0;
            InputData[1].ki.dwFlags = KEYEVENTF_KEYUP;
            InputData[1].ki.time = 0;
            InputData[1].ki.wVk = Keycode;
            InputData[1].ki.dwExtraInfo = IntPtr.Zero;

            SendInput(2, InputData, Marshal.SizeOf(typeof(INPUT)));
        }

        public void Send_CtrlKeyPress(short Keycode)
        {
            uint iNumberOfInputs = 4;
            INPUT[] InputData = new INPUT[iNumberOfInputs];

            InputData[0].type = 1;
            InputData[0].ki.wScan = 0;
            InputData[0].ki.dwFlags = KEYEVENTF_KEYDOWN;
            InputData[0].ki.time = 0;
            InputData[0].ki.wVk = VK_CONTROL;
            InputData[0].ki.dwExtraInfo = IntPtr.Zero;

            InputData[1].type = 1;
            InputData[1].ki.wScan = 0;
            InputData[1].ki.dwFlags = KEYEVENTF_KEYDOWN;
            InputData[1].ki.time = 0;
            InputData[1].ki.wVk = Keycode;
            InputData[1].ki.dwExtraInfo = IntPtr.Zero;

            InputData[2].type = 1;
            InputData[2].ki.wScan = 0;
            InputData[2].ki.dwFlags = KEYEVENTF_KEYUP;
            InputData[2].ki.time = 0;
            InputData[2].ki.wVk = Keycode;
            InputData[2].ki.dwExtraInfo = IntPtr.Zero;

            InputData[3].type = 1;
            InputData[3].ki.wScan = 0;
            InputData[3].ki.dwFlags = KEYEVENTF_KEYUP;
            InputData[3].ki.time = 0;
            InputData[3].ki.wVk = VK_CONTROL;
            InputData[3].ki.dwExtraInfo = IntPtr.Zero;

            SendInput(iNumberOfInputs, InputData, Marshal.SizeOf(typeof(INPUT)));
        }

        public void Send_ShiftKeyPress(short Keycode)
        {
            uint iNumberOfInputs = 4;
            INPUT[] InputData = new INPUT[iNumberOfInputs];

            InputData[0].type = 1;
            InputData[0].ki.wScan = 0;
            InputData[0].ki.dwFlags = KEYEVENTF_KEYDOWN;
            InputData[0].ki.time = 0;
            InputData[0].ki.wVk = VK_SHIFT;
            InputData[0].ki.dwExtraInfo = IntPtr.Zero;

            InputData[1].type = 1;
            InputData[1].ki.wScan = 0;
            InputData[1].ki.dwFlags = KEYEVENTF_KEYDOWN;
            InputData[1].ki.time = 0;
            InputData[1].ki.wVk = Keycode;
            InputData[1].ki.dwExtraInfo = IntPtr.Zero;

            InputData[2].type = 1;
            InputData[2].ki.wScan = 0;
            InputData[2].ki.dwFlags = KEYEVENTF_KEYUP;
            InputData[2].ki.time = 0;
            InputData[2].ki.wVk = Keycode;
            InputData[2].ki.dwExtraInfo = IntPtr.Zero;

            InputData[3].type = 1;
            InputData[3].ki.wScan = 0;
            InputData[3].ki.dwFlags = KEYEVENTF_KEYUP;
            InputData[3].ki.time = 0;
            InputData[3].ki.wVk = VK_SHIFT;
            InputData[3].ki.dwExtraInfo = IntPtr.Zero;

            SendInput(iNumberOfInputs, InputData, Marshal.SizeOf(typeof(INPUT)));
        }

        public void Send_CtrlShiftKeyPress(short VKCode)
        {
            uint iNumberOfInputs = 6;
            uint idx = 0;
            INPUT[] InputData = new INPUT[iNumberOfInputs];

            InputData[idx].type = 1;
            InputData[idx].ki.wScan = 0;
            InputData[idx].ki.dwFlags = KEYEVENTF_KEYDOWN;
            InputData[idx].ki.time = 0;
            InputData[idx].ki.wVk = VK_CONTROL;
            InputData[idx].ki.dwExtraInfo = IntPtr.Zero;

            idx++;
            InputData[idx].type = 1;
            InputData[idx].ki.wScan = 0;
            InputData[idx].ki.dwFlags = KEYEVENTF_KEYDOWN;
            InputData[idx].ki.time = 0;
            InputData[idx].ki.wVk = VK_SHIFT;
            InputData[idx].ki.dwExtraInfo = IntPtr.Zero;

            idx++;
            InputData[idx].type = 1;
            InputData[idx].ki.wScan = 0;
            InputData[idx].ki.dwFlags = KEYEVENTF_KEYDOWN;
            InputData[idx].ki.time = 0;
            InputData[idx].ki.wVk = VKCode;
            InputData[idx].ki.dwExtraInfo = IntPtr.Zero;

            idx++;
            InputData[idx].type = 1;
            InputData[idx].ki.wScan = 0;
            InputData[idx].ki.dwFlags = KEYEVENTF_KEYUP;
            InputData[idx].ki.time = 0;
            InputData[idx].ki.wVk = VKCode;
            InputData[idx].ki.dwExtraInfo = IntPtr.Zero;

            idx++;
            InputData[idx].type = 1;
            InputData[idx].ki.wScan = 0;
            InputData[idx].ki.dwFlags = KEYEVENTF_KEYUP;
            InputData[idx].ki.time = 0;
            InputData[idx].ki.wVk = VK_SHIFT;
            InputData[idx].ki.dwExtraInfo = IntPtr.Zero;

            idx++;
            InputData[idx].type = 1;
            InputData[idx].ki.wScan = 0;
            InputData[idx].ki.dwFlags = KEYEVENTF_KEYUP;
            InputData[idx].ki.time = 0;
            InputData[idx].ki.wVk = VK_CONTROL;
            InputData[idx].ki.dwExtraInfo = IntPtr.Zero;

            SendInput(iNumberOfInputs, InputData, Marshal.SizeOf(typeof(INPUT)));
        }
    }
}
