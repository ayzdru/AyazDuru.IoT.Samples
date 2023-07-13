using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AyazDuru.IoT.MAUI.Models
{
    public class ESP32Log : INotifyPropertyChanged
    {
        private string _logMessage;
        public string LogMessage
        {
            get
            {
                return this._logMessage;
            }

            set
            {
                if (value != this._logMessage)
                {
                    this._logMessage = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
