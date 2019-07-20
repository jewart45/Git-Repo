using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AttaGlance
{
    public class CalendarPageViewModel
    {
        public List<Calendar> CalendarList { get; private set; }

        public CalendarPage Page { get; private set; }

        public CalendarPageViewModel(CalendarPage page)
        {
            Page = page;
            SetUpPage(Page);
        }

        private void SetUpPage(CalendarPage page)
        {
            page.SetGrid(CalendarList);
        }
    }
}
