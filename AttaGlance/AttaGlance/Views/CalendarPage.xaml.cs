using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
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
            peopleStack.Width = 100;
            viewStack.Width = 100;
            var datePanel = new DockPanel() { Name="datePanel", Width=100, LastChildFill=false };
            var b1 = new Border { BorderBrush = new SolidColorBrush(Colors.Gray), BorderThickness = new Thickness(2), Margin = new Thickness(-1) };
            b1.Child = new TextBlock { Text = "Date", FontSize = 25, HorizontalAlignment = HorizontalAlignment.Center };

            DockPanel.SetDock(b1, Dock.Top);
            datePanel.Children.Add(b1);
            peopleStack.Children.Add(datePanel);
            foreach (var c in calendarList)
            {
                var b = new Border { BorderBrush = new SolidColorBrush(Colors.Gray), BorderThickness = new Thickness(2), Margin = new Thickness(-1) };
                b.Child = new TextBlock { Tag = c, Text = c.Name, FontSize = 25, HorizontalAlignment = HorizontalAlignment.Center };
          
                DockPanel.SetDock(b, Dock.Top);
                var panel = new DockPanel() { Name = c.Name + "_Dock", Width = 200, LastChildFill = false };
                panel.Children.Add(b);
                peopleStack.Children.Add(panel);
                peopleStack.Width += panel.Width;
                viewStack.Width += panel.Width;
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
                                    var eb = new EventBlock(ev.Start, ev.End, ev.Subject, ev.Description) { Height=75 };
                                    DockPanel.SetDock(eb, Dock.Top);
                                    p.Children.Add(eb);
                                }
                            }
                            else
                            {
                                var eeb = new EmptyEventBlock() { Height = 75 };
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

        private void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            if(viewStack.Width >= 100)
            {
                viewStack.Width -= 0.1 * viewStack.Width;
                viewStack.Height -= 0.1 * viewStack.Width;
                ZoomOut_Btn.IsEnabled = true;
            }
            else
            {
                ZoomIn_Btn.IsEnabled = true;
            }
            
        }

        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            if (viewStack.Width <= 5000)
            {
                viewStack.Width += 0.1 * viewStack.Width;
                viewStack.Height += 0.1 * viewStack.Width;
                ZoomIn_Btn.IsEnabled = true;
            }
            else
            {
                ZoomOut_Btn.IsEnabled = true;
            }
        }

        private void ZoomOut_Btn_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
