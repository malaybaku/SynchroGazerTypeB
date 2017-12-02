using System;
using System.Diagnostics;
using System.Windows.Input;

namespace Baku.SynchroGazer
{
    public class LicenseViewModel : ViewModelBase
    {
        public LicenseViewModel()
        {
            ShowUrlCommand = new ActionCommand(parameter =>
            {
                if (parameter is Uri uri)
                {
                    Process.Start(uri.AbsoluteUri);
                }
            });
        }

        public ICommand ShowUrlCommand { get; }
    }
}
