using Microsoft.Toolkit.Uwp.UI.Controls;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
        private CalendarPageViewModel ViewModel;
        private int daysShown = 15;

        public CalendarPage()
        {
            ViewModel = new CalendarPageViewModel(this);
            this.DataContext = ViewModel;
            this.InitializeComponent();

            if ((App.Current as App).IsAuthenticated)
            {
                DataTemplate calendarTemplate = new DataTemplate();
            }

            this.ObservableForProperty(x => x.GroupsCombo.SelectedItem).Where(x => x.Value != null).Subscribe(s =>
            {
                ViewModel.SetCurrentCalendarsAsync(s.Value as string);
            });



        }

        public void Refresh()
        {

        }


        internal void SetGrid(List<Calendar> calendarList)
        {
            peopleStack.Children.Clear();
            peopleStack.ColumnDefinitions.Clear();
            peopleStack.RowDefinitions.Clear();
            peopleStack.Width = 100;
            viewStack.Width = 100;
            peopleStack.Height = 75;
            viewStack.Height = 75;
            var datePanel = new Grid() { Name="datePanel", Width=100, Height = 500 };

            AddGridColumn(peopleStack, 0);
            //var splitter = new GridSplitter() { Width = 10};
            //Grid.SetColumn(splitter, peopleStack.ColumnDefinitions.Count - 1);
            //AddGridColumn(peopleStack);

            //peopleStack.Children.Add(splitter);

            Grid.SetColumn(datePanel, 0);
            var b1 = new Border { BorderBrush = new SolidColorBrush(Colors.Gray), BorderThickness = new Thickness(2), Margin = new Thickness(-1) };
            b1.Child = new TextBlock { Text = "Date", FontSize = 25, HorizontalAlignment = HorizontalAlignment.Center };

            //DockPanel.SetDock(b1, Dock.Top);
            datePanel.Children.Add(b1);
            peopleStack.Children.Add(datePanel);
            foreach (var c in calendarList)
            {
                AddGridColumn(peopleStack, 0);
               // var splitter2 = new GridSplitter() { Width = 10 };
               // Grid.SetColumn(splitter2, peopleStack.ColumnDefinitions.Count - 1);
              //  AddGridColumn(peopleStack);
                var b = new Border { BorderBrush = new SolidColorBrush(Colors.Gray), BorderThickness = new Thickness(2), Margin = new Thickness(-1) };
                b.Child = new TextBlock { Tag = c, Text = c.Name, FontSize = 25, HorizontalAlignment = HorizontalAlignment.Center };
          
                //DockPanel.SetDock(b, Dock.Top);
                var panel = new Grid() { Name = c.Name + "_Dock", Width = 200, Height=500 };
                Grid.SetColumn(panel, peopleStack.ColumnDefinitions.Count - 1);
                //panel.Children.Add(new GridSplitter());
                panel.Children.Add(b);
                peopleStack.Children.Add(panel);
                peopleStack.Width += panel.Width;
                viewStack.Width += panel.Width;
            }
            for(int i = 0; i < daysShown; i++)
            {
                //Date Block
                AddGridRow(peopleStack, 0);
                AddGridRow(datePanel, 0);
                datePanel.Height += 75;
                peopleStack.Height += 75;
                viewStack.Height += 75;
                var date = DateTime.Now.Date.AddDays(i);
                var dlb = new DateLabelBlock(date) { Width = 100 };
                Grid.SetRow(dlb, i + 1);
               // DockPanel.SetDock(dlb, Dock.Top);
                datePanel.Children.Add(dlb);

                //each other block in row
                foreach(var panel in peopleStack.Children.Where(x=>x != datePanel))
                {
                    if (panel is Grid p)
                    {
                        AddGridRow(p, 0);
                        var cal = calendarList.FirstOrDefault(x => x.Name + "_Dock" == (panel as Grid).Name);
                        if (cal != null)
                        {
                            var eventsThatDay = cal.EventsList.Where(x => x.CheckIfDayIntersects(date)).ToList();
                            var stack = new StackPanel() { Height = 75, VerticalAlignment = VerticalAlignment.Top };
                            Grid.SetRow(stack, i + 1);

                            foreach (var ev in eventsThatDay)
                                {
                                    var eb = new EventBlock(ev.Start, ev.End, ev.Subject, ev.Description) { Height = 50, VerticalAlignment = VerticalAlignment.Top };
                                    
                                    stack.Children.Add(eb);
                                }
                                var eeb = new EmptyEventBlock() {
                                    Date = date.Date,
                                    Attendee = new Microsoft.Graph.Attendee()
                                    {
                                        EmailAddress = new Microsoft.Graph.EmailAddress()
                                        {
                                            Address = cal.Email,
                                            Name = cal.Name
                                        }
                                    }
                                };
                                //DockPanel.SetDock(eeb, Dock.Top);
                                //Grid.SetRow(eeb, i + 1);
                            stack.Children.Add(eeb);
                            p.Children.Add(stack);
                               
                            
                        }
                        else
                        {

                        }
                        p.Height += 75;
                    }
                }
            }
        }

        private void AddGridColumn(Grid g, double width = 0)
        {
            var colDefinition = new ColumnDefinition();
            colDefinition.Width = width > 0 ? new GridLength(width) : GridLength.Auto;
            g.ColumnDefinitions.Add(colDefinition);
        }
        private void AddGridRow(Grid g, double height = 0)
        {
            var rowDefinition = new RowDefinition();
            rowDefinition.Height = height > 0 ? new GridLength(height) : GridLength.Auto;
            g.RowDefinitions.Add(rowDefinition);
        }

        internal void SetGroups(List<string> list)
        {
            GroupsCombo.ItemsSource = list;
            GroupsCombo.SelectedIndex = 0;
        }

        internal void SetLoading(bool v) => LoadingRing.Visibility = v ? Visibility.Visible : Visibility.Collapsed;

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
