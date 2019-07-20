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
            peopleStack.Width = 200;
            var datePanel = new DockPanel() { Name="datePanel", Width=200 };
            var t = new TextBlock { Text = "Date", FontSize = 25, HorizontalAlignment = HorizontalAlignment.Center };
            DockPanel.SetDock(t, Dock.Top);
            datePanel.Children.Add(t);
            peopleStack.Children.Add(datePanel);
            foreach (var c in calendarList)
            {
                var p = new TextBlock { Tag = c, Text = c.Name, FontSize = 25, HorizontalAlignment = HorizontalAlignment.Center };
                DockPanel.SetDock(p, Dock.Top);
                var panel = new DockPanel() { Name = c.Name + "_Dock", Width = 400 };
                panel.Children.Add(p);
                peopleStack.Children.Add(panel);
                peopleStack.Width += panel.Width;
            }
            for(int i = 0; i < 15; i++)
            {
                //Date Block
                var date = DateTime.Now.Date.AddDays(i);
                var dlb = new DateLabelBlock(date);
                DockPanel.SetDock(dlb, Dock.Top);
                datePanel.Children.Add(dlb);

                //each other block in row
                foreach(var panel in peopleStack.Children.Where(x=>x != datePanel))
                {
                    if (panel is DockPanel p)
                    {
                        var cal = calendarList.FirstOrDefault(x => x.Name + "_Dock" == (panel as DockPanel).Name);
                        if (cal != null)
                        {
                            var eventsThatDay = cal.EventsList.Where(x => x.CheckIfDayIntersects(date)).ToList();
                            if (eventsThatDay.Count > 0)
                            {
                                foreach (var ev in eventsThatDay)
                                {
                                    var eb = new EventBlock(ev.Subject, ev.Description) { Height=100 };
                                    DockPanel.SetDock(eb, Dock.Top);
                                    p.Children.Add(eb);
                                }
                            }
                            else
                            {
                                var eeb = new EmptyEventBlock() { Height = 100 };
                                DockPanel.SetDock(eeb, Dock.Top);
                                p.Children.Add(eeb);
                            }
                        }
                        else
                        {

                        }
                    }
                }
            }
        }
    }
}
