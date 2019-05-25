using MMABettingModule.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace MMABettingModule
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
            get => navigationVisibility;
            set
            {
                navigationVisibility = value;
                NotifyPropertyChanged();
            }
        }

        private Brush themeColour = Brushes.AntiqueWhite;

        public Brush ThemeColour
        {
            get => themeColour;
            set
            {
                themeColour = value;
                NotifyPropertyChanged();
            }
        }

        private List<string> resultTypes = new List<string>();

        public List<string> ResultTypes
        {
            get => resultTypes;
            set
            {
                resultTypes = value;
                NotifyPropertyChanged();
            }
        }

        private TimeSpan loggingInterval = TimeSpan.FromHours(6);

        public TimeSpan LoggingInterval
        {
            get => loggingInterval;
            set
            {
                loggingInterval = value;
                NotifyPropertyChanged();
            }
        }

        private TimeSpan autoRefreshInterval = TimeSpan.FromHours(3);

        public TimeSpan AutoRefreshInterval
        {
            get => autoRefreshInterval;
            set
            {
                autoRefreshInterval = value;
                NotifyPropertyChanged();
            }
        }

        private List<ComboBoxColourItem> buttonBackGrounds = new List<ComboBoxColourItem>();

        public List<ComboBoxColourItem> ButtonBackgroundColours
        {
            get => buttonBackGrounds;
            set
            {
                buttonBackGrounds = value;
                NotifyPropertyChanged();
            }
        }

        private bool userLoggedIn = false;

        public bool UserLoggedIn
        {
            get => userLoggedIn;
            set
            {
                userLoggedIn = value;
                NotifyPropertyChanged();
            }
        }

        private bool virtualise = false;

        public bool Virtualise
        {
            get => virtualise;
            set
            {
                virtualise = value;
                NotifyPropertyChanged();
            }
        }

        private int reloginTimes = 0;

        public int ReloginTimes
        {
            get => reloginTimes;
            set
            {
                reloginTimes = value;
                NotifyPropertyChanged();
            }
        }

        private System.Windows.Media.Brush buttonBackgroundColour = System.Windows.Media.Brushes.LightGray;

        public System.Windows.Media.Brush ButtonBackgroundColour
        {
            get => buttonBackgroundColour;
            set
            {
                buttonBackgroundColour = value;
                NotifyPropertyChanged();
            }
        }

        private string loggedUserName = "";

        public string LoggedUserName
        {
            get => loggedUserName;
            set
            {
                loggedUserName = value;
                NotifyPropertyChanged();
            }
        }

        private double betAmount { get; set; }

        public double BetAmount
        {
            get => betAmount;
            set
            {
                betAmount = value;
                NotifyPropertyChanged();
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}