using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using SpeechNotifierWin10.Classes;
using System.Speech.Recognition;
using Windows.UI.Notifications;
using System.Windows;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SpeechNotifierWin10
{

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        private GUIProperties myGuiProperties;
        public string FileName { get; } = @"C:\Users\Public\Documents\phrases.xml";
        public SpeechRecognitionEngine Rec { get; set; }

        public Choices GrammarList { get; set; } = new Choices();
        public List<Phrase> PhraseList { get; private set; }

        public CustomXmlSerializer Serializer { get; set; }

        public MainPage()
        {
            if (Rec == null)
            {
                Rec = new SpeechRecognitionEngine();
            }

            myGuiProperties = new GUIProperties();

            Serializer = new CustomXmlSerializer(FileName, typeof(List<Phrase>));

            DataContext = myGuiProperties;
            Rec.RequestRecognizerUpdate();
            Rec.SpeechRecognized += Rec_SpeechRecognized;
            Rec.SpeechHypothesized += Rec_SpeechHypothesized;
            Rec.SpeechDetected += Rec_SpeechDetected;

            Rec.SetInputToDefaultAudioDevice();

            //ToastNotificationManager.CreateToastNotifier().Show(new ToastNotification(new Windows.Data.Xml.Dom.XmlDocument())

            InitializeComponent();

            Intialise();
        }

        private void Rec_SpeechDetected(object sender, SpeechDetectedEventArgs e) => Console.WriteLine("Detected Speech sounds");

        private void Rec_SpeechHypothesized(object sender, SpeechHypothesizedEventArgs e) => Console.WriteLine("Maybe found speech");

        public void Rec_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            var phrase = PhraseList.FirstOrDefault(x => x.SpeechText == e.Result.Text);
            if (phrase != null)
            {

                Notificationer.ShowNotification("Speech has been recognized", phrase.NotificationText);

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
            Rec.RecognizeAsyncStop();
            NotifierChk.IsOn = false;
            PhraseList = Serializer.Deserialize() as List<Phrase>;
            if (PhraseList == null)
            {
                PhraseList = new List<Phrase>();
            }
            else
            {
                PhraseSelectorCmbo.Items.Clear();
                foreach (Phrase p in PhraseList.Where(x => x.SpeechText != ""))
                {
                    PhraseSelectorCmbo.Items.Add(p);
                    GrammarList.Add(p.SpeechText);
                }
                Grammar grammar = new Grammar(new GrammarBuilder(GrammarList));
                Rec.UnloadAllGrammars();
                Rec.LoadGrammar(grammar);
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
                myGuiProperties.PhraseGridVisibility = Visibility.Collapsed;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            bool saved = false;

            if (myGuiProperties.CurrentSpeechText == "Please Enter Phrase Here..." && myGuiProperties.CurrentSpeechText.Trim() == "")
            {
                MessageBox.Show("Please edit the text in the box");
            }
            else if (PhraseSelectorCmbo.SelectedIndex == -1)
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
                    Rec.RecognizeAsync(RecognizeMode.Multiple);
                }
                else
                {
                    Rec.RecognizeAsyncStop();
                }
            }
            catch
            {
                MessageBox.Show("Could not toggle notification");
                NotifierChk.IsOn = (bool)!NotifierChk.IsOn;
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
