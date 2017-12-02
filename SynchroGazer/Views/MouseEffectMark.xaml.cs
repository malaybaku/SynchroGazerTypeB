using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Baku.SynchroGazer
{
    /// <summary>
    /// MouseEffectMark.xaml の相互作用ロジック
    /// </summary>
    public partial class MouseEffectMark : UserControl
    {
        public MouseEffectMark()
        {
            InitializeComponent();
        }

        public void SingleClick()
            => (FindResource("SingleClick") as Storyboard)?.Begin();

        public void DoubleClick()
            => (FindResource("DoubleClick") as Storyboard)?.Begin();

    }
}
