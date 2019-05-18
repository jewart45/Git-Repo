using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SpeechNotifier.Classes
{
    public class Phrase
    {
        public string Text { get; set; }

        public Phrase(string text)
        {
            Text = text;
        }

        public Phrase()
        {
            Text = "";
        }
    }
}