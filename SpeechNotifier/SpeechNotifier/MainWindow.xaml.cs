using SpeechNotifier.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Speech.Recognition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Xml;
using System.Xml.Serialization;

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
            //Create and load a sample grammar.

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

            appl._notifyIcon.ShowBalloonTip(1, "Speech Found", e.Result.Text, System.Windows.Forms.ToolTipIcon.Info);
            //MessageBox.Show("Found some speech: " + e.Result.Text);
            Console.WriteLine("Speech Found");
        }

        private void MainWindow_SpeechRecognized(object sender, SpeechRecognizedEventArgs e) => MessageBox.Show("Found Speech" + e.Result.Text);

        private void Intialise()
        {
            GetPhrases(PhraseSelectorCmbo);

            PhraseSelectorCmbo.SelectedIndex = 1;
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
                foreach (Phrase p in PhraseList)
                {
                    PhraseSelectorCmbo.Items.Add(p.Text);
                    grammarList.Add(p.Text);
                }
                Grammar grammar = new Grammar(new GrammarBuilder(grammarList));
                rec.UnloadAllGrammars();
                rec.LoadGrammar(grammar);
            }
        }

        private void AddPhrase_Click(object sender, RoutedEventArgs e)
        {
            myGuiProperties.CurrentText = "Please Enter Phrase Here...";
            myGuiProperties.PhraseGridVisibility = Visibility.Visible;
        }

        private void DeletePhrase_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult res = MessageBox.Show("Are you sure you wish to delete this phrase?", "Confirmation", MessageBoxButton.YesNoCancel);
            if (res == MessageBoxResult.Yes)
            {
                Phrase p = PhraseList.Where(x => x.Text == PhraseSelectorCmbo.SelectedItem as string).FirstOrDefault();
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
            if (PhraseSelectorCmbo.SelectedItem != null)
            {
                string phrase = PhraseSelectorCmbo.SelectedItem as string;
                myGuiProperties.CurrentText = phrase;
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

            if (myGuiProperties.CurrentText == "Please Enter Phrase Here..." && myGuiProperties.CurrentText.Trim() == "")
            {
                MessageBox.Show("Please edit the text in the box");
            }
            else
            {
                Phrase newPhrase = new Phrase(myGuiProperties.CurrentText);
                if (!PhraseList.Select(x => x.Text).ToList().Contains(myGuiProperties.CurrentText))
                {
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
            if (!PhraseList.Select(x => x.Text).ToList().Contains(k.Text))
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