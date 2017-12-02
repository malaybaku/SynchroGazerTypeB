using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Baku.SynchroGazer
{
    public class NotifiableBase : INotifyPropertyChanged
    {
        protected bool SetValue<T>(ref T target, T value, [CallerMemberName]string pname = "")
            where T : IEquatable<T>
        {
            if (!target.Equals(value))
            {
                target = value;
                NotifyPropertyChanged(pname);
                return true;
            }
            return false;
        }

        protected void NotifyPropertyChanged([CallerMemberName]string pname = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(pname));

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
