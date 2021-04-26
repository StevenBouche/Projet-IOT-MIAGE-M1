using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace ControlerRobot.Models
{
    public abstract  class VModelBase : INotifyPropertyChanged
    {

        private Dispatcher Dispatcher;

        public VModelBase(Dispatcher dispatcher)
        {
            Dispatcher = dispatcher;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(field, newValue))
            {

                field = newValue;

                if (Dispatcher.CheckAccess())
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                }
                else
                {
                    Dispatcher.BeginInvoke(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
                }
                return true;
            }
            return false;
        }

    }
}
