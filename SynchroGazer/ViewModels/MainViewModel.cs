using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace Baku.SynchroGazer
{
    public class MainViewModel : ViewModelBase, IDisposable
    {
        public MainViewModel()
        {
            Status = new StatusViewModel(SettingFile.Instance);
            _contents = new ViewModelBase[]
            {
                Status,
                Setting,
                License,
            };
            CurrentContent = _contents[0];

            if (Status.Status != null)
            {
                _mouseController = new MouseController(
                    SettingFile.Instance,
                    Status.VolatileSetting,
                    Status.Status
                    );

                _udpSender = new UdpSender(
                    Status.VolatileSetting,
                    Status.Status
                    );
                _udpReceiver = new UdpReceiver(Status.VolatileSetting);
            }

            Application.Current.MainWindow.Loaded += (_, __) =>
            {
                ShowPreviewMark();

                if (!DesignerProperties.GetIsInDesignMode(Application.Current.MainWindow))
                {
                    _unityProcessController = new UnityProcessController();
                }
            };
        }

        private void ShowPreviewMark()
        {
            var mark = new PreviewMark()
            {
                //Ownerつけない: 最小化が同期しちゃうからダメ
                DataContext = Status
            };
            Application.Current.MainWindow.Closed += (_, __) => mark.Close();

            _mouseController.MouseActionStart += (_, e) =>
            {
                switch (e.ActionType)
                {
                    case MouseActionTypes.LeftSingleClick:
                    case MouseActionTypes.RightSingleClick:
                    case MouseActionTypes.CenterClick:
                        mark.Dispatcher.Invoke(() =>
                        {
                            mark.SingleClickOn(e.X - Status.PreviewMarkWindowLeft, e.Y - Status.PreviewMarkWindowTop);
                        });
                        break;
                    case MouseActionTypes.LeftDoubleClick:
                    case MouseActionTypes.RightDoubleClick:
                        mark.Dispatcher.Invoke(() =>
                        {
                            mark.DoubleClickOn(e.X - Status.PreviewMarkWindowLeft, e.Y - Status.PreviewMarkWindowTop);
                        });
                        break;
                    default:
                        //何もしない
                        return;
                }
            };

            mark.Show();
        }

        private readonly UdpSender _udpSender;
        private readonly UdpReceiver _udpReceiver;
        private readonly MouseController _mouseController;
        private UnityProcessController _unityProcessController;

        public SettingViewModel Setting { get; } = new SettingViewModel();
        public StatusViewModel Status { get; }
        //ライセンス情報はVM化しなくてもいいが絵的に統一感出るので…。
        public LicenseViewModel License { get; } = new LicenseViewModel();


        private ViewModelBase[] _contents;

        private ViewModelBase _currentContent = null;
        public ViewModelBase CurrentContent
        {
            get => _currentContent;
            private set
            {
                if (_currentContent != value)
                {
                    _currentContent = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private int _selectedIndex = 0;
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                bool changed = SetValue(ref _selectedIndex, value);
                if (changed && value >= 0 && value < _contents.Length)
                {
                    CurrentContent = _contents[value];
                }
            }
        }


        private ICommand _disposeCommand;
        public ICommand DisposeCommand
            => _disposeCommand ?? (_disposeCommand = new ActionCommand(Dispose));

        public void Dispose()
        {
            Setting.Dispose();
            Status.Dispose();
            _udpReceiver?.Dispose();
            _unityProcessController?.Dispose();
        }
    }
}
