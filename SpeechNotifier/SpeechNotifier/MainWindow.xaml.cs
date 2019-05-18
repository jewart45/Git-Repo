using SpeechNotifier.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Recognition;
using System.Windows;
using System.Windows.Controls;

namespace SpeechNotifier
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GUIProperties myGuiProperties;
        public string fileName { get; } = @"C:\Users\Public\Documents\phrases.xml";
        public SpeechRecognitionEngine rec { get; set; }

        public Choices grammarList { get; set; } = new Choices();
        public List<Phrase> PhraseList { get; private set; }

        public CustomXmlSerializer Serializer { get; set; }
        

        public MainWindow()
        {
            if (rec == null)
            {
                rec = new SpeechRecognitionEngine();
            }

            myGuiProperties = new GUIProperties();

            Serializer = new CustomXmlSerializer(fileName, typeof(List<Phrase>));

            DataContext = myGuiProperties;
            rec.RequestRecognizerUpdate();
            rec.SpeechRecognized += Rec_SpeechRecognized;
            rec.SpeechHypothesized += Rec_SpeechHypothesized;
            rec.SpeechDetected += Rec_SpeechDetected;

            rec.SetInputToDefaultAudioDevice();

            InitializeComponent();

            Intialise();
        }

        private void Rec_SpeechDetected(object sender, SpeechDetectedEventArgs e) => Console.WriteLine("Detected Speech sounds");

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
                    grammarList.Add(p.SpeechText);
                }
                Grammar grammar = new Grammar(new GrammarBuilder(grammarList));
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

        private void PhraseSelectorCmbo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (PhraseSelectorCmbo.SelectedItem != null && myGuiProperties.CurrentSpeechText != (PhraseSelectorCmbo.SelectedItem as Phrase).SpeechText)
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
                rec.RecognizeAsync(RecognizeMode.Multiple);
            }
            else
            {
                rec.RecognizeAsyncStop();
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
    }
}