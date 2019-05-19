using System;
using System.Collections.Generic;
using System.Windows;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace SpeechNotifierWinApp.Classes
{
    public class GUIProperties : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public GUIProperties()
        {
        }

        private Visibility phraseGridVisibility = Visibility.Collapsed;

        public Visibility PhraseGridVisibility
        {
            get
            {
                return phraseGridVisibility;
            }
            set
            {
                phraseGridVisibility = value;
                NotifyPropertyChanged();
            }
        }


        private string currentSpeechText = "";

        public string CurrentSpeechText
        {
            get
            {
                return currentSpeechText;
            }
            set
            {
                currentSpeechText = value;
                NotifyPropertyChanged();
            }
        }

        private string currentResponseText = "";

        public string CurrentResponseText {
            get {
                return currentResponseText;
            }
            set {
                currentResponseText = value;
                NotifyPropertyChanged();
            }
        }



        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}