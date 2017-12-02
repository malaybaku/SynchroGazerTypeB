using System.Text;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System;

namespace Baku.SynchroGazer
{
    public class UdpSender : IDisposable
    {
        public UdpSender(SynchroGazerVolatileSetting setting, SynchroGazerStatus status)
        {
            setting.PropertyChanged += OnSettingChanged;
            status.BlinkActionHappened += OnBlinkAction;
            _udpClient = new UdpClient();
            _endPoint = new IPEndPoint(IPAddress.Loopback, SettingFile.Instance.Setting.PortWpfToUnity);
        }
        private readonly UdpClient _udpClient;
        private readonly IPEndPoint _endPoint;

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
            _udpClient.Send(bytes, bytes.Length, _endPoint);
        }

        public void Dispose()
        {
            _udpClient.Dispose();
        }
    }
}
