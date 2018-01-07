using System;
using System.Text;
using System.Threading;
using System.ComponentModel;
using System.IO.MemoryMappedFiles;

namespace Baku.SynchroGazer
{
    public class WindowsEventSender : IDisposable
    {
        public WindowsEventSender(SynchroGazerVolatileSetting setting, SynchroGazerStatus status)
        {
            _mmf = MemoryMappedFile.CreateOrOpen(
                WindowsEventProtocol.MemoryMappedFileWpfToUnity,
                WindowsEventProtocol.MemoryMappedFileByteLength
                );

            _eventHandle = new EventWaitHandle(
                false, EventResetMode.AutoReset, WindowsEventProtocol.EventHandleWpfToUnity
                );

            setting.PropertyChanged += OnSettingChanged;
            status.BlinkActionHappened += OnBlinkAction;
        }

        private MemoryMappedFile _mmf;
        private EventWaitHandle _eventHandle;

        private void OnBlinkAction(object sender, BlinkActionEventArgs e)
        {
            if (!(sender is SynchroGazerStatus status))
            {
                return;
            }

            //無効化したBlink送るの禁止！
            if (e.ActionType == BlinkActionTypes.Blink &&
                SettingFile.Instance.Setting.AllowOnlyWink)
            {
                return;
            }

            SendMessage($"BlinkAction:{e.ActionType}");
        }
        private void OnSettingChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(sender is SynchroGazerVolatileSetting setting))
            {
                return;
            }

            if (e.PropertyName == nameof(setting.GazeActionNumber))
            {
                SendMessage($"GazeActionSelect:{setting.GazeActionNumber}");
            }
        }
        private void SendMessage(string message)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            using (var acc = _mmf.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Write))
            {
                acc.Write(0, bytes.Length);
                acc.WriteArray(4, bytes, 0, bytes.Length);
            }
            _eventHandle.Set();
        }

        public void Dispose()
        {
            _mmf.Dispose();
            _eventHandle.Dispose();
        }
    }
}
