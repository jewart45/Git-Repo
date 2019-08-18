using Microsoft.Graph;
using Microsoft.Toolkit.Services.MicrosoftGraph;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AttaGlance
{
    public class CalendarPageViewModel
    {
        private GraphServiceClient GraphClient = MicrosoftGraphService.Instance.GraphProvider;

        public ObservableCollection<Calendar> CalendarList { get; private set; } = new ObservableCollection<Calendar>();

        public CalendarPage Page { get; private set; }

        public ObservableCollection<CalendarGroup> CalendarGroups { get; set; } = new ObservableCollection<CalendarGroup>();
        public CalendarGroup SelectedGroup { get; set; }
        public IReactiveCommand RefreshCmd { get; private set; }

        public CalendarPageViewModel(CalendarPage page)
        {
            Page = page;
            SetupCommands();
            SetUpPageAsync(Page);
        }

        internal async void SetCurrentCalendarsAsync(string s)
        {
            SetLoadingGrid(true);
            var calendar = CalendarGroups.Where(x => x.Name == s).FirstOrDefault();
            if(calendar == null)
            {
                return;
            }
            var page = await GraphClient.Me.CalendarGroups[calendar.Id].Calendars.Request()
                        .GetAsync();

            CalendarList.Clear();
            foreach (var cal in page.CurrentPage)
            {
                    var pp = await GraphClient.Me.Calendars[cal.Id].Events.Request()
                        .Select("subject,organizer,start,end")
                    .OrderBy("createdDateTime DESC")
                    .GetAsync();
                
                var evList = new List<Event>();
                foreach (var ev in pp.CurrentPage)
                {
                    evList.Add(new Event(DateTime.Parse(ev.Start.DateTime), DateTime.Parse(ev.End.DateTime), ev.Subject, ev.Body?.ToString()));
                }
                //set calendar
                var cal1 = new Calendar(cal.Id, cal.Name)
                {
                    EventsList = evList
                };
                CalendarList.Add(cal1);
            }

            Page.SetGrid(CalendarList.ToList());
            SetLoadingGrid(false);
        }

        private void SetupCommands()
        {
            //Set up commands
            RefreshCmd = ReactiveCommand.CreateFromTask(async () =>
            {
                SetLoadingGrid(true);
                await SetUpPageAsync(Page);
                SetLoadingGrid(false);
            });

        }


        private void SetLoadingGrid(bool v) => Page.SetLoading(v);

        private async Task SetUpPageAsync(CalendarPage page)
        {

            await SetUpCalendarBackendAsync();
            page.SetGrid(CalendarList.ToList());
        }

        private async Task SetUpCalendarBackendAsync()
        {
            
            try
            {
                var me = await GraphClient.Me.Request()
                                       .Select("displayName,id,mail,calendars")
                                       .GetAsync();

                
                // Get the events

                //Get My events
                try
                {
                    var k = await GraphClient.Me.Calendars.Request().Select("owner,name,id").GetAsync();
                    var k1 = await GraphClient.Me.CalendarGroups.Request().Select("name,id").GetAsync();

                    foreach(var ev in k.CurrentPage)
                    {
                        var pp = await GraphClient.Me.Calendars[ev.Id].Events.Request()
                            .Select("subject,organizer,start,end")
                        .OrderBy("createdDateTime DESC")
                        .GetAsync();
                    }

                    CalendarGroups = new ObservableCollection<CalendarGroup>(k1.CurrentPage);
                    Page.SetGroups(CalendarGroups.Select(x => x.Name).ToList());
                    SelectedGroup = CalendarGroups.FirstOrDefault();
                   // foreach (var ev in k1.CurrentPage)
                   // {
                   //     var pp = await GraphClient.Me.CalendarGroups[ev.Id].Calendars.Request()
                   //     //    .Select("subject,organizer,start,end")
                   //     //.OrderBy("createdDateTime DESC")
                   //     .GetAsync();
                   // }

                   // var events = await GraphClient.Me.Events.Request()
                   //.Select("subject,organizer,start,end")
                   //.OrderBy("createdDateTime DESC")
                   //.GetAsync();

                   
                   // var evList = new List<Event>();
                   // //go through events from user to create calendar
                   // foreach (var ev in events.CurrentPage)
                   // {
                   //     evList.Add(new Event(DateTime.Parse(ev.Start.DateTime), DateTime.Parse(ev.End.DateTime), ev.Subject, ev.Body?.ToString()));
                   // }
                   // //set calendar
                   // var cal = new Calendar(me.Id, me.DisplayName)
                   // {
                   //     EventsList = evList
                   // };
                   // CalendarList.Add(cal);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    //empty catch
                }

                var users1 = await GraphClient.Users.Request()
                                    .Select("displayName,id,mail,calendars,outlook,calendarview")
                                    .GetAsync();
                var userlist = users1.CurrentPage.ToList();
                //foreach (var u in userlist.Where(x=>x.DisplayName != me.DisplayName))
                //{
                //    try
                //    {
                //        var adress = new EmailAddress();
                //        adress.Address = u.Mail;
                //        //var ab = new Attendee() { EmailAddress = adress, Type = AttendeeType.Required };
                //        //var s = new List<Attendee>();
                //        //s.Add(ab);
                //        //GetAvailabilityDetails();
                //        var check = await GraphClient.Users[u.Id].Calendar.Request().GetAsync();
                //        var pp = await GraphClient.Users[u.Id].Events.Request()
                //            .Select("subject,organizer,start,end")
                //        .OrderBy("createdDateTime DESC")
                //        .GetAsync();
                //        var evList = new List<Event>();
                //        //go through events from user to create calendar
                //        foreach (var ev in pp.CurrentPage)
                //        {
                //            evList.Add(new Event(DateTime.Parse(ev.Start.DateTime), DateTime.Parse(ev.End.DateTime), ev.Subject, ev.Body?.ToString()));
                //        }
                //        //set calendar
                //        var cal = new Calendar(u.Id, u.DisplayName)
                //        {
                //            EventsList = evList
                //        };
                //        CalendarList.Add(cal);
                //    }
                //    catch (Exception ex)
                //    {
                //        Console.WriteLine(ex.ToString());
                //        //empty catch
                //    }

                //}

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }
    }
}
