using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Controls;

namespace TaskInstanceQuery
{
 
    public class SettingsProperties : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public SettingsProperties(Settings settings)
        {
            this.ProcessToCheck = settings.ProcessToCheck;
            ServersToQuery = settings.ServersToQuery;
            CheckLocal = settings.CheckLocal;
        }
        public SettingsProperties()
        {

        }

        private string processToCheck = "example.exe";

        public string ProcessToCheck
        {
            get => processToCheck;
            set
            {
                processToCheck = value;
                NotifyPropertyChanged();
            }
        }



        private string serversToQuery = "localhost";

        public string ServersToQuery
        {
            get => serversToQuery;
            set
            {
                serversToQuery = value;
                NotifyPropertyChanged();
            }
        }
        private bool checkLocal = true;

        public bool CheckLocal
        {
            get => checkLocal;
            set
            {
                checkLocal = value;
                NotifyPropertyChanged();
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
