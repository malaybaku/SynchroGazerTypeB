using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Baku.SynchroGazer
{
    public static class ClickCommandExecutor
    {        
        public static async Task DoMouseActionAsync(
            double x, double y, MouseActionTypes action, int doubleClickIntervalMillisec
            )
        {
            if (action == MouseActionTypes.None)
            {
                return;
            }

            System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)x, (int)y);

            //specify flags
            uint flag = GetMouseEventFlag(action);
            if (flag == 0)
            {
                return;
            }

            var input = new MouseInput()
            {
                TypeTag = InputStructConsts.INPUT_MOUSE,
                //Dx = (int)x,
                //Dy = (int)y,
                Dx = 0,
                Dy = 0,
                MouseData = 0,
                Flags = flag,
                Time = 0,
                ExtraInfo = 0
            };

            //処理に入るのが確定した時点で遅延させる。理由は視点アイコンが消えてマウスと干渉しなくなるまで最低1フレーム欲しいから。
            await Task.Delay(16);

            SendInputOneTime(input);

            //ダブルクリックの場合だけ続けて2クリック目飛ばす
            switch (action)
            {
                case MouseActionTypes.LeftDoubleClick:
                case MouseActionTypes.RightDoubleClick:
                    await Task.Delay(doubleClickIntervalMillisec);
                    SendInputOneTime(input);
                    break;
                default:
                    break;
            }
        }

        private static void SendInputOneTime(MouseInput input)
        {
            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = Marshal.AllocHGlobal(Marshal.SizeOf<MouseInput>());
                Marshal.StructureToPtr(input, ptr, false);
                SendInput(1, ptr, Marshal.SizeOf<MouseInput>());
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        private static uint GetMouseEventFlag(MouseActionTypes action)
        {
            switch (action)
            {
                case MouseActionTypes.LeftSingleClick:
                case MouseActionTypes.LeftDoubleClick:
                    return MouseEventFlags.MOUSEEVENTF_LEFTDOWN | MouseEventFlags.MOUSEEVENTF_LEFTUP;
                case MouseActionTypes.RightSingleClick:
                case MouseActionTypes.RightDoubleClick:
                    return MouseEventFlags.MOUSEEVENTF_RIGHTDOWN | MouseEventFlags.MOUSEEVENTF_RIGHTUP;
                case MouseActionTypes.CenterClick:
                    return MouseEventFlags.MOUSEEVENTF_MIDDLEDOWN | MouseEventFlags.MOUSEEVENTF_MIDDLEUP;
                default:
                    return 0;
            }
        }

        #region Native

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, uint dwExtraInfo);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern int SendInput(int inputs, IntPtr pInputs, int size);

        //Mouse actions

        static class InputStructConsts
        {
            public const int INPUT_MOUSE = 0;
            public const int INPUT_KEYBOARD = 1;
            public const int INPUT_HARDWARE = 2;
        }

        static class MouseEventFlags
        {
            //NOTE: Win32APIで使われてる名称で書き写す(ググラビリティの為に)

            public const int MOUSEEVENTF_MOVE = 0x01;

            public const int MOUSEEVENTF_LEFTDOWN = 0x02;
            public const int MOUSEEVENTF_LEFTUP = 0x04;
            public const int MOUSEEVENTF_RIGHTDOWN = 0x08;
            public const int MOUSEEVENTF_RIGHTUP = 0x10;
            public const int MOUSEEVENTF_MIDDLEDOWN = 0x20;
            public const int MOUSEEVENTF_MIDDLEUP = 0x40;

            public const int MOUSEEVENTF_XDOWN = 0x80;
            public const int MOUSEEVENTF_XUP = 0x100;
            //note: when using wheel event, use dwData to set 
            public const int MOUSEEVENTF_WHEEL = 0x800;
            public const int MOUSEEVENTF_HWHEEL = 0x1000;

            public const int MOUSEEVENTF_NOCOALESCE = 0x2000;
            public const int MOUSEEVENTF_VIRTUALDESK = 0x4000;
            public const int MOUSEEVENTF_ABSOLUTE = 0x8000;

        }

        #endregion
    }
    
    [StructLayout (LayoutKind.Sequential)]
    public struct MouseInput
    {
        public uint TypeTag;
        public int Dx;
        public int Dy;
        public uint MouseData;
        public uint Flags;
        public uint Time;
        //本当はポインタだけど使わないからバイトサイズだけ合わす
        public uint ExtraInfo;
    }


    public enum MouseActionTypes
    {
        None,
        LeftSingleClick,
        LeftDoubleClick,
        RightSingleClick,
        RightDoubleClick,
        CenterClick,
    }



}
