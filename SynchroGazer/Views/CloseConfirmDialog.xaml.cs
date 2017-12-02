using System.Windows;

namespace Baku.SynchroGazer
{
    /// <summary>
    /// CloseConfirmDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class CloseConfirmDialog : Window
    {
        public CloseConfirmDialog()
        {
            InitializeComponent();
        }

        private void OnOKClick(object sender, RoutedEventArgs e)
        {
            ClosingOperationType = ClosingOperationTypes.Close;
            DialogResult = true;
        }

        private void OnOnlyMinimizeClick(object sender, RoutedEventArgs e)
        {
            ClosingOperationType = ClosingOperationTypes.OnlyMinimize;
            DialogResult = false;
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            ClosingOperationType = ClosingOperationTypes.Cancel;
            DialogResult = false;
        }

        public ClosingOperationTypes ClosingOperationType { get; private set; }
            = ClosingOperationTypes.Cancel;
    }

    public enum ClosingOperationTypes
    {
        Close,
        OnlyMinimize,
        Cancel
    }
}
