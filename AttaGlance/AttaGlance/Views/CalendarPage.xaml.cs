using Microsoft.Toolkit.Uwp.UI.Controls;
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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace AttaGlance
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CalendarPage : Page
    {
        public CalendarPage()
        {
            this.DataContext = new CalendarPageViewModel(this);
            this.InitializeComponent();

            if ((App.Current as App).IsAuthenticated)
            {
                DataTemplate calendarTemplate = new DataTemplate();
            }



        }

        public void Refresh()
        {

        }

        internal void SetGrid(List<Calendar> calendarList)
        {
            peopleStack.Children.Clear();
            var datePanel = new DockPanel();
            var t = new TextBlock { Text = "Date", FontSize = 25, HorizontalAlignment = HorizontalAlignment.Center };
            DockPanel.SetDock(t, Dock.Top);
            datePanel.Children.Add(t);
            peopleStack.Children.Add(datePanel);
            foreach (var c in calendarList)
            {
                var p = new TextBlock { Tag = c, Text = c.Name, FontSize = 25, HorizontalAlignment = HorizontalAlignment.Center };
                DockPanel.SetDock(p, Dock.Top);
                datePanel.Children.Add(p);
                var panel = new DockPanel();
                panel.Children.Add(p);
                peopleStack.Children.Add(panel);
            }
            for(int i = 0; i < 15; i++)
            {

            }
        }
    }
}
