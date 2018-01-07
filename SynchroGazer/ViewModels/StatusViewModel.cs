using System;
using System.ComponentModel;
using System.Windows;

namespace Baku.SynchroGazer
{
    public class StatusViewModel : ViewModelBase, IDisposable
    {
        public StatusViewModel(SettingFile setting)
        {
            _setting = setting;

            if (!DesignerProperties.GetIsInDesignMode(Application.Current.MainWindow))
            {
                Status = new SynchroGazerStatus();
            }

            VolatileSetting = new SynchroGazerVolatileSetting();


            ShowLargeMarkAlways = _setting.Setting.ShowLargeMarkAlways;
            ShowFixTargetMark = 
                (_setting.Setting.ShowFixTargetMark && Status.FixDisplayPosition);
            ShowAnimationFeedback = _setting.Setting.ShowAnimationFeedback;

            Status.PropertyChanged += OnStatusChanged;
            _setting.FileSaved += OnSettingChanged;
            _setting.SettingFileContentChanged += OnSettingChanged;
        }

        private readonly SettingFile _setting;
        public SynchroGazerStatus Status { get; }
        public SynchroGazerVolatileSetting VolatileSetting { get; }

        //NOTE: 位置情報のうち、スクリーン座標とかUI表示のとこだけ再計算したいので直す
        private double _previewMarkWindowLeft = 0;
        public double PreviewMarkWindowLeft
        {
            get => _previewMarkWindowLeft;
            set
            {
                if (SetValue(ref _previewMarkWindowLeft, value))
                {
                    NotifyPropertyChanged(nameof(X));
                }
            }
        }

        private double _previewMarkWindowTop = 0;
        public double PreviewMarkWindowTop
        {
            get => _previewMarkWindowTop;
            set
            {
                if (SetValue(ref _previewMarkWindowTop, value))
                {
                    NotifyPropertyChanged(nameof(Y));
                }
            }
        }

        public double X => Status.DisplayX / DpiChecker.DpiFactorX - PreviewMarkWindowLeft;
        public double Y => Status.DisplayY / DpiChecker.DpiFactorY - PreviewMarkWindowTop;


        private bool _showLargeMarkAlways = false;
        public bool ShowLargeMarkAlways
        {
            get => _showLargeMarkAlways;
            private set => SetValue(ref _showLargeMarkAlways, value);
        }

        private bool _showFixTargetMark = false;
        public bool ShowFixTargetMark
        {
            get => _showFixTargetMark;
            set => SetValue(ref _showFixTargetMark, value);
        }

        private bool _showAnimationFeedback = false;
        public bool ShowAnimationFeedback
        {
            get => _showAnimationFeedback;
            set => SetValue(ref _showAnimationFeedback, value);
        }

        private void OnStatusChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Status.DisplayX))
            {
                NotifyPropertyChanged(nameof(X));
            }
            else if (e.PropertyName == nameof(Status.DisplayY))
            {
                NotifyPropertyChanged(nameof(Y));
            }
            else if (e.PropertyName == nameof(Status.FixDisplayPosition))
            {
                ShowFixTargetMark = 
                    (Status.FixDisplayPosition && _setting.Setting.ShowFixTargetMark);
            }
        }

        private void OnSettingChanged(object sender, EventArgs e)
        {
            ShowLargeMarkAlways = _setting.Setting.ShowLargeMarkAlways;
            ShowFixTargetMark =
                (_setting.Setting.ShowFixTargetMark && Status.FixDisplayPosition);
            ShowAnimationFeedback = _setting.Setting.ShowAnimationFeedback;
        }

        public void Dispose()
        {
            Status?.Dispose();
        }
    }
}
