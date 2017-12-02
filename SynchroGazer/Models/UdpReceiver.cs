using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Baku.SynchroGazer
{
    public class UdpReceiver : IDisposable
    {
        private static readonly string GazeActionSelectHeader = "GazeActionSelect:";

        public UdpReceiver(SynchroGazerVolatileSetting setting)
        {
            _setting = setting;
            Task.Run(() => ReceiveThread(_cts.Token));
        }
        private readonly SynchroGazerVolatileSetting _setting;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        private void OnMessage(string message)
        {
            if (message.StartsWith(GazeActionSelectHeader))
            {
                string id = message.Substring(GazeActionSelectHeader.Length);
                if (id == "0")
                {
                    _setting.ActionType = GazeActionTypes.SingleClick;
                }
                else if (id == "1")
                {
                    _setting.ActionType = GazeActionTypes.DoubleClick;
                }
                else if (id == "2")
                {
                    _setting.ActionType = GazeActionTypes.CenterClick;
                }
            }

        }

        private async void ReceiveThread(CancellationToken token)
        {
            int port = SettingFile.Instance.Setting.PortUnityToWpf;
            using (var udpClient = new UdpClient(new IPEndPoint(IPAddress.Loopback, port)))
            {
                udpClient.Client.ReceiveTimeout = 1000;
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        var recv = await udpClient.ReceiveAsync();
                        string message = Encoding.UTF8.GetString(recv.Buffer);
                        OnMessage(message);
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show(ex.Message);
                        //ログやるとスレッド違反で死にます
                        //Debug.Log(ex.Message);
                        //タイムアウト無視したいので握りつぶす方向で。
                    }
                }
            }
        }

        public void Dispose()
        {
            _cts.Cancel();
        }
    }
}
