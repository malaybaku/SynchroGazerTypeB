using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;

namespace Baku.SynchroGazer
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            var language = SettingFile.Instance.Setting.Language;
            Properties.Resources.Culture = (language == LanguageTypes.Japanese) ?
                CultureInfo.GetCultureInfo("ja-JP") :
                CultureInfo.GetCultureInfo("en-US");

            InitializeComponent();
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            var source = PresentationSource.FromVisual(this);
            if (source != null)
            {
                DpiChecker.DpiFactorX = source.CompositionTarget.TransformToDevice.M11;
                DpiChecker.DpiFactorY = source.CompositionTarget.TransformToDevice.M22;
            }

        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            var operationType = ShowClosingDialog();
            switch(operationType)
            {
                case ClosingOperationTypes.OnlyMinimize:
                    e.Cancel = true;
                    WindowState = WindowState.Minimized;
                    break;
                case ClosingOperationTypes.Cancel:
                    e.Cancel = true;
                    break;
                default:
                    break;
            }
        }

        private void OnShowClosingDialog(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private ClosingOperationTypes ShowClosingDialog()
        {
            var dialog = new CloseConfirmDialog()
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
            };

            dialog.ShowDialog();
            return dialog.ClosingOperationType;
        }
    }
}
