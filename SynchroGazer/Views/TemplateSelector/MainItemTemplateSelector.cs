using System.Windows;
using System.Windows.Controls;

namespace Baku.SynchroGazer
{
    public class MainItemTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            string resourceName =
                (item is StatusViewModel) ? "StatusViewModelDataTemplate" :
                (item is SettingViewModel setting) ? "SettingViewModelDataTemplate" :
                (item is LicenseViewModel) ? "LicenseViewModelDataTemplate" :
                "";

            if (!string.IsNullOrEmpty(resourceName))
            {
                return Application.Current.FindResource(resourceName) as DataTemplate;
            }

            return base.SelectTemplate(item, container);
        }

    }
}
