using SpeechNotifier.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Recognition;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using ToggleSwitch;

namespace SpeechNotifier
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GUIProperties myGuiProperties;
        public string FileName = System.AppDomain.CurrentDomain.BaseDirectory + "phrases.xml";
        private Timer timeoutTimer = new Timer(1000);

        public SpeechRecognitionEngine rec { get; set; }

        public Choices GrammarList { get; set; } = new Choices();
        public List<Phrase> PhraseList { get; private set; }

        public CustomXmlSerializer Serializer { get; set; }


        public MainWindow()
        {
            this.Title = "Speech Notifier";
            if (rec == null)
            {
                rec = new SpeechRecognitionEngine();
            }
            myGuiProperties = new GUIProperties();

            Serializer = new CustomXmlSerializer(FileName, typeof(List<Phrase>));

            DataContext = myGuiProperties;
            rec.RequestRecognizerUpdate();
            rec.SpeechRecognized += Rec_SpeechRecognized;
            rec.SpeechHypothesized += Rec_SpeechHypothesized;
            rec.SpeechDetected += Rec_SpeechDetected;

            rec.SetInputToDefaultAudioDevice();
            timeoutTimer.Elapsed += TimeoutTimer_Elapsed;
            InitializeComponent();

            Intialise();
        }

        private void InvokeUI(Action a) => Application.Current.Dispatcher.Invoke(a);

        private void TimeoutTimer_Elapsed(object sender, ElapsedEventArgs e) => InvokeUI(()=> detectionIndicator.Fill = System.Windows.Media.Brushes.White);
        private void Rec_SpeechDetected(object sender, SpeechDetectedEventArgs e) => ToggleDetectionColour();
        private void ToggleDetectionColour()
        {
            timeoutTimer.Start();
            detectionIndicator.Fill = System.Windows.Media.Brushes.Green;
        }

        private void Rec_SpeechHypothesized(object sender, SpeechHypothesizedEventArgs e) => Console.WriteLine("Maybe found speech");

        public void Rec_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            App appl = (Application.Current as App);
            var phrase = PhraseList.FirstOrDefault(x => x.SpeechText == e.Result.Text);
            if(phrase != null)
            {
                appl._notifyIcon.ShowBalloonTip(1, "Speech Recognized", phrase.NotificationText, System.Windows.Forms.ToolTipIcon.Info);

            }
            //MessageBox.Show("Found some speech: " + e.Result.Text);
            Console.WriteLine("Speech Found");
        }

        private void Intialise()
        {
            GetPhrases(PhraseSelectorCmbo);

            PhraseSelectorCmbo.SelectedIndex = -1;
        }

        private void GetPhrases(ComboBox phraseSelectorCmbo)
        {
            rec.RecognizeAsyncStop();
            NotifierChk.IsChecked = false;
            PhraseList = Serializer.Deserialize() as List<Phrase>;
            if (PhraseList == null)
            {
                PhraseList = new List<Phrase>();
            }
            else
            {
                PhraseSelectorCmbo.Items.Clear();
                foreach (Phrase p in PhraseList.Where(x=>x.SpeechText != ""))
                {
                    PhraseSelectorCmbo.Items.Add(p);
                    GrammarList.Add(p.SpeechText);
                }
                Grammar grammar = new Grammar(new GrammarBuilder(GrammarList));
                rec.UnloadAllGrammars();
                rec.LoadGrammar(grammar);
            }
        }

        private void AddPhrase_Click(object sender, RoutedEventArgs e)
        {
            PhraseSelectorCmbo.SelectedIndex = -1;
            myGuiProperties.CurrentSpeechText = "Enter phrase you wish to recognize here...";
            myGuiProperties.CurrentResponseText = "Enter notification text when seech is recognized...";
            myGuiProperties.PhraseGridVisibility = Visibility.Visible;
        }

        private void DeletePhrase_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult res = MessageBox.Show("Are you sure you wish to delete this phrase?", "Confirmation", MessageBoxButton.YesNoCancel);
            if (res == MessageBoxResult.Yes)
            {
                Phrase p = PhraseList.Where(x => x == PhraseSelectorCmbo.SelectedItem as Phrase).FirstOrDefault();
                if (p != null)
                {
                    PhraseList.Remove(p);
                    Serializer.Serialize(PhraseList);
                    GetPhrases(PhraseSelectorCmbo);
                    MessageBox.Show("Phrase Deleted, Run Notifier with checkbox.");
                }
            }
        }

        private void PhraseSelectorCmbo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PhraseSelectorCmbo.SelectedIndex != -1)
            {
                Phrase phrase = PhraseSelectorCmbo.SelectedItem as Phrase;
                myGuiProperties.CurrentSpeechText = phrase.SpeechText;
                myGuiProperties.CurrentResponseText = phrase.NotificationText;
                myGuiProperties.PhraseGridVisibility = Visibility.Visible;
            }
            else
            {
                myGuiProperties.PhraseGridVisibility = Visibility.Hidden;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            bool saved = false;

            if (myGuiProperties.CurrentSpeechText == "Please Enter Phrase Here..." && myGuiProperties.CurrentSpeechText.Trim() == "")
            {
                MessageBox.Show("Please edit the text in the box");
            }
            else if(PhraseSelectorCmbo.SelectedIndex == -1)
            {
                Phrase newPhrase = new Phrase() { SpeechText = myGuiProperties.CurrentSpeechText, NotificationText = myGuiProperties.CurrentResponseText };
                PhraseList.Add(newPhrase);
                Serializer.Serialize(PhraseList);
                GetPhrases(PhraseSelectorCmbo);
                saved = true;
            }
            else
            {
                Phrase phraseToEdit = PhraseList.FirstOrDefault(x => x == PhraseSelectorCmbo.SelectedItem as Phrase);
                //Save list if not contains
                if (phraseToEdit != null)
                {
                    //Set to current values
                    phraseToEdit.SpeechText = myGuiProperties.CurrentSpeechText;
                    phraseToEdit.NotificationText = myGuiProperties.CurrentResponseText;

                    Serializer.Serialize(PhraseList);
                    GetPhrases(PhraseSelectorCmbo);
                    saved = true;
                }
                else
                {
                    Phrase newPhrase = new Phrase() { SpeechText = myGuiProperties.CurrentSpeechText, NotificationText = myGuiProperties.CurrentResponseText };
                    PhraseList.Add(newPhrase);
                    Serializer.Serialize(PhraseList);
                    GetPhrases(PhraseSelectorCmbo);
                    saved = true;
                }
            }

            if (saved)
            {
                MessageBox.Show("Saved.");
                Intialise();
            }
            else
            {
                MessageBox.Show("Not Saved.");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Serializer.Close();
            Environment.Exit(0);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox box = sender as CheckBox;
            if ((bool)box.IsChecked)
            {
                ToggleNotify(true);
            }
            else
            {
                ToggleNotify(false);
            }
        }

        public void ToggleNotify(bool v)
        {
            try
            {
                if (v)
                {
                    rec.RecognizeAsync(RecognizeMode.Multiple);
                }
                else
                {
                    rec.RecognizeAsyncStop();
                }
            }
            catch
            {
                MessageBox.Show("Could not toggle notification");
                NotifierChk.IsChecked = (bool)!NotifierChk.IsChecked;
            }
            
        }

        private void PhraseTextEdittingBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox k = sender as TextBox;
            if (!PhraseList.Select(x => x.SpeechText).ToList().Contains(k.Text))
            {
                SaveBtn.IsEnabled = true;
            }
            else
            {
                SaveBtn.IsEnabled = false;
            }
        }

        private void HorizontalToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {
            HorizontalToggleSwitch box = sender as HorizontalToggleSwitch;
            if ((bool)box.IsChecked)
            {
                ToggleNotify(true);
            }
            else
            {
                ToggleNotify(false);
            }
        }
    }
}