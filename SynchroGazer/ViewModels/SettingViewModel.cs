using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace Baku.SynchroGazer
{
    public class SettingViewModel : ViewModelBase, IDisposable
    {
        //NOTE: コレの寿命はModelと一致する(ハズ)
        public SettingViewModel()
        {
            _settingFile = SettingFile.Instance;
            Load();
            _settingFile.SettingFileContentChanged += (_, __) => Load();
            _isInitiaizing = false;
        }
        private readonly SettingFile _settingFile;

        public LanguageTypes Language => (LanguageTypes)LanguageNumber;
        public Characters Character => (Characters)CharacterNumber;
        public CharacterLocations Location => (CharacterLocations)LocationNumber;

        #region Properties

        public IEnumerable<int> ScreenNumbers
            => Enumerable.Range(1, UnityProcessController.GetNumberOfScreen());
        
        private int _languageNumber = 0;
        public int LanguageNumber
        {
            get => _languageNumber;
            set
            {
                if (SetValue(ref _languageNumber, value))
                {
                    Save();
                }
            }
        }

        public bool IsLanguageJapanese
        {
            get => (Language == LanguageTypes.Japanese);
            set
            {
                if (!IsLanguageJapanese && value)
                {
                    LanguageNumber = (int)LanguageTypes.Japanese;
                    NotifyPropertyChanged(nameof(IsLanguageEnglish));
                    NotifyPropertyChanged();
                }
            }
        }
        public bool IsLanguageEnglish
        {
            get => (Language == LanguageTypes.English);
            set
            {
                if (!IsLanguageEnglish && value)
                {
                    LanguageNumber = (int)LanguageTypes.English;
                    NotifyPropertyChanged(nameof(IsLanguageJapanese));
                    NotifyPropertyChanged();
                }
            }
        }
        
        private int _characterNumber = 0;
        public int CharacterNumber
        {
            get => _characterNumber;
            set
            {
                if (SetValue(ref _characterNumber, value))
                {
                    Save();
                }
            }
        }

        private int _screenNumber = 0;
        public int ScreenNumber
        {
            get => _screenNumber;
            set
            {
                if (SetValue(ref _screenNumber, value))
                {
                    Save();
                }
            }
        }

        //NOTE: VMライクじゃないが面倒なので適当に。

        private int _windowSizeRate = 100;
        public int WindowSizeRate
        {
            get => _windowSizeRate;
            set
            {
                if (SetValue(ref _windowSizeRate, value))
                {
                    Save();
                }
            }
        }

        private int _locationNumber = 0;
        public int LocationNumber
        {
            get => _locationNumber;
            set
            {
                if (SetValue(ref _locationNumber, value))
                {
                    Save();
                }
            }
        }

        public bool IsLocationLeft
        {
            get => Location == CharacterLocations.Left;
            set
            {
                if (!IsLocationLeft && value)
                {
                    LocationNumber = (int)CharacterLocations.Left;
                    NotifyPropertyChanged(nameof(IsLocationRight));
                    NotifyPropertyChanged();
                }
            }
        }
        public bool IsLocationRight
        {
            get => Location == CharacterLocations.Right;
            set
            {
                if (!IsLocationRight && value)
                {
                    LocationNumber = (int)CharacterLocations.Right;
                    NotifyPropertyChanged(nameof(IsLocationLeft));
                    NotifyPropertyChanged();
                }
            }
        }

        private bool _showLargeMarkAlways = false;
        public bool ShowLargeMarkAlways
        {
            get => _showLargeMarkAlways;
            set
            {
                if (SetValue(ref _showLargeMarkAlways, value))
                {
                    Save();
                }
            }
        }
        private bool _showFixTargetMark = true;
        public bool ShowFixTargetMark
        {
            get => _showFixTargetMark;
            set
            {
                if (SetValue(ref _showFixTargetMark, value))
                {
                    Save();
                }
            }
        }
        private bool _showAnimationFeedback = true;
        public bool ShowAnimationFeedback
        {
            get => _showAnimationFeedback;
            set
            {
                if (SetValue(ref _showAnimationFeedback, value))
                {
                    Save();
                }
            }
        }

        private int _mouseDoubleClickIntervalMillisec = 16;
        public int MouseDoubleClickIntervalMillisec
        {
            get => _mouseDoubleClickIntervalMillisec;
            set
            {
                if (value <= 1) return;

                if (SetValue(ref _mouseDoubleClickIntervalMillisec, value))
                {
                    Save();
                }
            }
        }

        private bool _isReverseMode = false;
        public bool IsReverseMode
        {
            get => _isReverseMode;
            set
            {
                if (SetValue(ref _isReverseMode, value))
                {
                    Save();
                }
            }
        }

        private bool _allowOnlyWink = false;
        public bool AllowOnlyWink
        {
            get => _allowOnlyWink;
            set
            {
                if (SetValue(ref _allowOnlyWink, value))
                {
                    Save();
                }
            }
        }

        private int _udpPortWpfToUnity = 58231;
        public int UdpPortWpfToUnity
        {
            get => _udpPortWpfToUnity;
            set
            {
                if (SetValue(ref _udpPortWpfToUnity, value))
                {
                    Save();
                }
            }
        }

        private int _udpPortUnityToWpf = 58232;
        public int UdpPortUnityToWpf
        {
            get => _udpPortUnityToWpf;
            set
            {
                if (SetValue(ref _udpPortUnityToWpf, value))
                {
                    Save();
                }
            }
        }
        

        #endregion

        private bool _isInitiaizing = true;
        private void Save()
        {
            if (_isInitiaizing)
            {
                return;
            }

            _settingFile.Setting.Language = Language;

            _settingFile.Setting.Character = Character;
            _settingFile.Setting.Location = Location;
            _settingFile.Setting.ScreenNumber = ScreenNumber;
            _settingFile.Setting.WindowSizeRate = WindowSizeRate;

            _settingFile.Setting.ShowLargeMarkAlways = ShowLargeMarkAlways;
            _settingFile.Setting.ShowFixTargetMark = ShowFixTargetMark;
            _settingFile.Setting.ShowAnimationFeedback = ShowAnimationFeedback;

            _settingFile.Setting.MouseDoubleClickIntervalMillisec = MouseDoubleClickIntervalMillisec;
            _settingFile.Setting.IsReverseMode = IsReverseMode;
            _settingFile.Setting.AllowOnlyWink = AllowOnlyWink;

            _settingFile.Setting.PortWpfToUnity = UdpPortWpfToUnity;
            _settingFile.Setting.PortUnityToWpf = UdpPortUnityToWpf;

            _settingFile.Save();
        }

        private void Load()
        {
            LanguageNumber = (int)_settingFile.Setting.Language;

            CharacterNumber = (int)_settingFile.Setting.Character;
            LocationNumber = (int)_settingFile.Setting.Location;
            ScreenNumber = _settingFile.Setting.ScreenNumber;
            WindowSizeRate = _settingFile.Setting.WindowSizeRate;

            ShowLargeMarkAlways = _settingFile.Setting.ShowLargeMarkAlways;
            ShowFixTargetMark = _settingFile.Setting.ShowFixTargetMark;
            ShowAnimationFeedback = _settingFile.Setting.ShowAnimationFeedback;

            MouseDoubleClickIntervalMillisec = _settingFile.Setting.MouseDoubleClickIntervalMillisec;
            IsReverseMode = _settingFile.Setting.IsReverseMode;
            AllowOnlyWink = _settingFile.Setting.AllowOnlyWink;

            UdpPortWpfToUnity = _settingFile.Setting.PortWpfToUnity;
            UdpPortUnityToWpf = _settingFile.Setting.PortUnityToWpf;
        }

        private ICommand _disposeCommand;
        public ICommand DisposeCommand
            => _disposeCommand ?? (_disposeCommand = new ActionCommand(Dispose));

        public void Dispose()
            => _settingFile?.Dispose();

    }
}
