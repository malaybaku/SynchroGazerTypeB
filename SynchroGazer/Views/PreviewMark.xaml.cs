using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace Baku.SynchroGazer
{
    /// <summary>
    /// PreviewMark.xaml の相互作用ロジック
    /// </summary>
    public partial class PreviewMark : Window
    {
        public PreviewMark()
        {
            InitializeComponent();
        }

        public void SingleClickOn(double x, double y)
        {
            Canvas.SetLeft(MouseEffect, x);
            Canvas.SetTop(MouseEffect, y);
            MouseEffect.SingleClick();
        }

        public void DoubleClickOn(double x, double y)
        {
            Canvas.SetLeft(MouseEffect, x);
            Canvas.SetTop(MouseEffect, y);
            MouseEffect.DoubleClick();
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            IntPtr hWnd = ((HwndSource)HwndSource.FromVisual(this)).Handle;
            int exStyle = Win32Api.GetWindowLong(hWnd, Win32Api.GWL_EXSTYLE);

            Win32Api.SetWindowLong(
                hWnd,
                Win32Api.GWL_EXSTYLE,
                exStyle | Win32Api.WS_EX_TRANSPARENT
                );
        }

    }


}
