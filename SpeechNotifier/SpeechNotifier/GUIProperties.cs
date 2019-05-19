﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Drawing;

namespace SpeechNotifier
{
    public class GUIProperties : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public GUIProperties()
        {
        }

        private Visibility phraseGridVisibility = Visibility.Hidden;

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

        private Brush currentPersonColour = Brushes.Red;

        public Brush CurrentPersonColour
        {
            get
            {
                return currentPersonColour;
            }
            set
            {
                currentPersonColour = value;
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