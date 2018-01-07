using System;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Baku.SynchroGazer
{
    public class WindowsEventReceiver
    {
        private static readonly string GazeActionSelectHeader = "GazeActionSelect:";

        public WindowsEventReceiver(SynchroGazerVolatileSetting setting)
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

        private void ReceiveThread(CancellationToken token)
        {
            //int port = SettingFile.Instance.Setting.PortUnityToWpf;
            using (var eventHandle = new EventWaitHandle(
                false, EventResetMode.AutoReset, WindowsEventProtocol.EventHandleUnityToWpf
                ))
            {
                //udpClient.Client.ReceiveTimeout = 1000;
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        bool eventHappened = eventHandle.WaitOne(1000);
                        if (!eventHappened)
                        {
                            continue;
                        }

                        string message = GetStringFromMemoryMappedFile();
                        OnMessage(message);
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show(ex.Message);
                    }
                }
            }
        }

        private static string GetStringFromMemoryMappedFile()
        {
            try
            {
                using (var mmf = MemoryMappedFile.OpenExisting(
                    WindowsEventProtocol.MemoryMappedFileUnityToWpf
                    ))
                using (var acc = mmf.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read))
                {
                    int len = acc.ReadInt32(0);
                    //無いとは思うが露骨に数がおかしいのは無視
                    len = Math.Max(len, 1024);
                    var data = new byte[len];
                    acc.ReadArray(0, data, 0, data.Length);
                    return Encoding.UTF8.GetString(data);
                }
            }
            catch(Exception ex)
            {
                Debug.Write($"{ex.Message}: {ex.StackTrace}");
                return "";
            }
        }

        public void Dispose()
        {
            _cts.Cancel();
        }
    }
}
