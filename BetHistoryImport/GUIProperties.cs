using BetHistoryImport.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace BetHistoryImport
{
    public class GUIProperties : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public GUIProperties()
        {
           
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

       

        private Thickness viewMargin = new Thickness(0);

        public Thickness ViewMargin
        {
            get => viewMargin;
            set
            {
                viewMargin = value;
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

        private List<string> competitionTypes = new List<string>();

        public List<string> CompetitionTypes
        {
            get => competitionTypes;
            set
            {
                competitionTypes = value;
                NotifyPropertyChanged();
            }
        }

        private List<SelectionDisplay> selectionToDisplay = new List<SelectionDisplay>();

        public List<SelectionDisplay> SelectionToDisplay
        {
            get => selectionToDisplay;
            set
            {
                selectionToDisplay = value;
                NotifyPropertyChanged();
            }
        }

       

        private TimeSpan loggingInterval = TimeSpan.FromMinutes(360);

        public TimeSpan LoggingInterval
        {
            get => loggingInterval;
            set
            {
                loggingInterval = value;
                NotifyPropertyChanged();
            }
        }

        private TimeSpan shortLoggingInterval = TimeSpan.FromMinutes(20);

        public TimeSpan ShortLoggingInterval
        {
            get => shortLoggingInterval;
            set
            {
                shortLoggingInterval = value;
                NotifyPropertyChanged();
            }
        }

        private TimeSpan autoRefreshInterval = TimeSpan.FromMinutes(20);

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

        private bool virtualize = false;

        public bool Virtualize
        {
            get => virtualize;
            set
            {
                virtualize = value;
                NotifyPropertyChanged();
            }
        }

        private List<string> sports = new List<string>();

        public List<string> Sports
        {
            get => sports;
            set
            {
                sports = value;
                NotifyPropertyChanged();
            }
        }

        private double currentBalance = 0;

        public double CurrentBalance
        {
            get => currentBalance;
            set
            {
                currentBalance = value;
                NotifyPropertyChanged();
            }
        }

        private double currentExposure = 0;

        public double CurrentExposure
        {
            get => currentExposure;
            set
            {
                currentExposure = value;
                NotifyPropertyChanged();
            }
        }

        private double totalBalance = 0;

        public double TotalBalance
        {
            get => totalBalance;
            set
            {
                totalBalance = value;
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

        private double betAmount = 0;

        public double BetAmount
        {
            get => betAmount;
            set
            {
                betAmount = value;
                NotifyPropertyChanged();
            }
        }

        private string bettingResultType = "All";

        public string BettingResultType
        {
            get => bettingResultType;
            set
            {
                bettingResultType = value;
                NotifyPropertyChanged();
            }
        }

        private double minBetLevel = 0;

        public string sport = "Mixed Martial Arts";
        public string Sport 
        {
            get => sport;
            set
            {
                sport = value;
                NotifyPropertyChanged();
            }
        }
        public double MinBetLevel
        {
            get => minBetLevel;
            set
            {
                minBetLevel = value;
                NotifyPropertyChanged();
            }
        }

        private double maxBetLevel = 0;

        public double MaxBetLevel
        {
            get => maxBetLevel;
            set
            {
                maxBetLevel = value;
                NotifyPropertyChanged();
            }
        }

        private double betLimit = 100;

        public double Betlimit
        {
            get => betLimit;
            set
            {
                betLimit = value;
                NotifyPropertyChanged();
            }
        }

        private List<string> resultSelectionList = new List<string>();

        public List<string> ResultSelectionList
        {
            get => resultSelectionList;
            set
            {
                resultSelectionList = value;
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

        private int eventTypeId = 7524;

        public int EventTypeId
        {
            get => eventTypeId;
            set
            {
                eventTypeId = value;
                NotifyPropertyChanged();
            }
        }

        private string filePath = @"C:\Users\jewar\Downloads\OtherSportsRaw";
        
        public string FilePath
        {
            get => filePath;
            set
            {
                filePath = value;
                NotifyPropertyChanged();
            }
        }

        private string resultType = "MATCH_ODDS";

        public string ResultType
        {
            get => resultType;
            set
            {
                resultType = value;
                NotifyPropertyChanged();
            }
        }

        private string mainMessage = "";

        public string MainMessage
        {
            get => mainMessage;
            set
            {
                mainMessage = value;
                NotifyPropertyChanged();
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}