using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Baku.SynchroGazer
{
    public class UnityProcessController : IDisposable
    {
        public UnityProcessController()
        {
            StartUnity();
            Task.Run(() => DoInitialWindowUpdate());

            var initialSetting = SettingFile.Instance.Setting;
            _windowSizeRate = initialSetting.WindowSizeRate;
            _location = initialSetting.Location;
            _screenNumber = initialSetting.ScreenNumber;

            SettingFile.Instance.FileSaved += OnSettingChanged;
            SettingFile.Instance.SettingFileContentChanged += OnSettingChanged;
        }

        public static int GetNumberOfScreen()
        {
            return Screen.AllScreens.Length;
        }

        private int _windowSizeRate = 0;
        private CharacterLocations _location = CharacterLocations.Left;

        private int _targetProcessId = 0;

        private int _screenNumber = 1;

        private void StartUnity()
        {
            //すでに起動済みなら無視
            if (_targetProcessId != 0 && 
                Process.GetProcessById(_targetProcessId) != null)
            {
                return;
            }

            var p = new Process();

            p.StartInfo.WorkingDirectory = Path.Combine(
                Path.GetDirectoryName(
                    Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)
                    ),
                "Unity"
                );
            p.StartInfo.FileName = Path.Combine(
                p.StartInfo.WorkingDirectory, "SynchroGazer.exe"
                );
            p.StartInfo.UseShellExecute = false;
            p.Start();

            _targetProcessId = p.Id;
        }

        private void UpdateWindowPosition(SynchroGazerSetting setting)
        {
            Process p = null;
            try
            {
                p = Process.GetProcessById(_targetProcessId);
            }
            catch(ArgumentException)
            {
                //終了済みプロセス指定すると起きる -> そのままガードで弾いて無視
            }

            if (p == null)
            {
                return;
            }

            IntPtr hWnd = p.MainWindowHandle;
            if (hWnd == IntPtr.Zero)
            {
                return;
            }

            //NOTE: ほんとうはディスプレイ番号とかも設定から引っ張ってこないとダメか？
            var screen = 
                (setting.ScreenNumber > 0 && 
                 setting.ScreenNumber <= Screen.AllScreens.Length) ?
                Screen.AllScreens[setting.ScreenNumber - 1] :
                Screen.PrimaryScreen;
            var screenBound = screen.WorkingArea;

            int width = 400 * setting.WindowSizeRate / 100;

            switch (setting.Location)
            {
                case CharacterLocations.Left:
                    MoveWindow(hWnd, screenBound.Left, screenBound.Bottom - width, width, width, false);
                    break;
                case CharacterLocations.Right:
                    MoveWindow(hWnd, screenBound.Right - width, screenBound.Bottom - width, width, width, false);
                    break;
                default:
                    break;
            }

        }

        private void OnSettingChanged(object sender, EventArgs e)
        {
            var setting = SettingFile.Instance.Setting;
            if (_windowSizeRate != setting.WindowSizeRate ||
                _location != setting.Location ||
                _screenNumber != setting.ScreenNumber
                )
            {
                _windowSizeRate = setting.WindowSizeRate;
                _location = setting.Location;
                _screenNumber = setting.ScreenNumber;
                UpdateWindowPosition(SettingFile.Instance.Setting);
            }
        }

        private async void DoInitialWindowUpdate()
        {
            while (Process.GetProcessById(_targetProcessId) != null)
            {
                await Task.Delay(500);
                var p = Process.GetProcessById(_targetProcessId);
                if (p.MainWindowHandle != IntPtr.Zero)
                {
                    UpdateWindowPosition(SettingFile.Instance.Setting);
                    SetWindowAttribute(p.MainWindowHandle);
                    return;
                }
            }
        }

        private void SetWindowAttribute(IntPtr hWnd)
        {
            int exStyle = Win32Api.GetWindowLong(hWnd, Win32Api.GWL_EXSTYLE);

            exStyle = exStyle | Win32Api.WS_EX_TRANSPARENT | Win32Api.WS_EX_LAYERED | Win32Api.WS_EX_TOPMOST;
            //exStyle = exStyle & Win32Api.WS_EX_APPWINDOW_DISABLE;

            Win32Api.SetWindowLong(
                hWnd,
                Win32Api.GWL_EXSTYLE,
                exStyle
                );

            //Zオーダーの修正
            SetWindowPos(hWnd, (IntPtr)HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
        }

        public const int SWP_NOMOVE = 0x0002;
        public const int SWP_NOSIZE = 0x0001;
        public const int HWND_TOPMOST = -1;

        [DllImport("user32.dll")]
        private static extern bool MoveWindow(IntPtr hWnd, int x, int y, int width, int height, bool repaint);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr insertAfter, int x, int y, int cx, int cy, int uFlags);

        public void Dispose()
        {
            try
            {
                var p = (Process.GetProcessById(_targetProcessId));
                if (p != null)
                {
                    p.Kill();
                }
            }
            catch(ArgumentException)
            {
                //終了済みプロセスのId指定したりすると発生するが想定内なので潰してOK
            }
            finally
            {
                _targetProcessId = 0;
            }
        }
    }
}
