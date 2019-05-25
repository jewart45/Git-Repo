using System;
using System.Collections.Generic;
using System.Windows;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Drawing;
using BoxingBettingModule.Classes;

namespace BoxingBettingModule
{
    public class GUIProperties : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public GUIProperties()
        {
            ButtonBackgroundColours.Add(new ComboBoxColourItem { Content = nameof(System.Windows.Media.Brushes.LightGray), Value = System.Windows.Media.Brushes.LightGray });
            ButtonBackgroundColours.Add(new ComboBoxColourItem { Content = nameof(System.Windows.Media.Brushes.DeepPink), Value = System.Windows.Media.Brushes.DeepPink });
            ButtonBackgroundColours.Add(new ComboBoxColourItem { Content = nameof(System.Windows.Media.Brushes.SteelBlue), Value = System.Windows.Media.Brushes.SteelBlue });
            ButtonBackgroundColours.Add(new ComboBoxColourItem { Content = nameof(System.Windows.Media.Brushes.SpringGreen), Value = System.Windows.Media.Brushes.SpringGreen });
            ButtonBackgroundColours.Add(new ComboBoxColourItem { Content = nameof(System.Windows.Media.Brushes.MintCream), Value = System.Windows.Media.Brushes.MintCream });
            ButtonBackgroundColours.Add(new ComboBoxColourItem { Content = nameof(System.Windows.Media.Brushes.Gray), Value = System.Windows.Media.Brushes.Gray });
        }

        private Visibility navigationVisibility = Visibility.Hidden;

        public Visibility NavigationVisibility
        {
            get
            {
                return navigationVisibility;
            }
            set
            {
                navigationVisibility = value;
                NotifyPropertyChanged();
            }
        }

        private Brush themeColour = Brushes.AntiqueWhite;

        public Brush ThemeColour
        {
            get
            {
                return themeColour;
            }
            set
            {
                themeColour = value;
                NotifyPropertyChanged();
            }
        }

        private List<string> resultTypes = new List<string>();

        public List<string> ResultTypes
        {
            get
            {
                return resultTypes;
            }
            set
            {
                resultTypes = value;
                NotifyPropertyChanged();
            }
        }

        private TimeSpan loggingInterval = TimeSpan.FromMinutes(30);

        public TimeSpan LoggingInterval
        {
            get
            {
                return loggingInterval;
            }
            set
            {
                loggingInterval = value;
                NotifyPropertyChanged();
            }
        }

        private TimeSpan autoRefreshInterval = TimeSpan.FromMinutes(30);

        public TimeSpan AutoRefreshInterval
        {
            get
            {
                return autoRefreshInterval;
            }
            set
            {
                autoRefreshInterval = value;
                NotifyPropertyChanged();
            }
        }

        private List<ComboBoxColourItem> buttonBackGrounds = new List<ComboBoxColourItem>();

        public List<ComboBoxColourItem> ButtonBackgroundColours
        {
            get
            {
                return buttonBackGrounds;
            }
            set
            {
                buttonBackGrounds = value;
                NotifyPropertyChanged();
            }
        }

        private bool userLoggedIn = false;

        public bool UserLoggedIn
        {
            get
            {
                return userLoggedIn;
            }
            set
            {
                userLoggedIn = value;
                NotifyPropertyChanged();
            }
        }

        private int reloginTimes = 0;

        public int ReloginTimes
        {
            get
            {
                return reloginTimes;
            }
            set
            {
                reloginTimes = value;
                NotifyPropertyChanged();
            }
        }

        private System.Windows.Media.Brush buttonBackgroundColour = System.Windows.Media.Brushes.LightGray;

        public System.Windows.Media.Brush ButtonBackgroundColour
        {
            get
            {
                return buttonBackgroundColour;
            }
            set
            {
                buttonBackgroundColour = value;
                NotifyPropertyChanged();
            }
        }

        private string loggedUserName = "";

        public string LoggedUserName
        {
            get
            {
                return loggedUserName;
            }
            set
            {
                loggedUserName = value;
                NotifyPropertyChanged();
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}