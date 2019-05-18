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
        public string SpeechText { get; set; } = "";
        public string NotificationText { get; set; } = "";


        public Phrase()
        {
        }
    }
}