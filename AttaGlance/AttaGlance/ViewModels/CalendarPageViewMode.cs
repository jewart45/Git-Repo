using Microsoft.Graph;
using Microsoft.Toolkit.Services.MicrosoftGraph;
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
        public List<Calendar> CalendarList { get; private set; } = new List<Calendar>();

        public CalendarPage Page { get; private set; }

        public CalendarPageViewModel(CalendarPage page)
        {
            Page = page;
            SetUpPageAsync(Page);
        }

        private async void SetUpPageAsync(CalendarPage page)
        {

            await SetUpEventsAsync();
            page.SetGrid(CalendarList);
        }

        private async Task SetUpEventsAsync()
        {
            var graphClient = MicrosoftGraphService.Instance.GraphProvider;
            try
            {
                var me = await graphClient.Me.Request()
                                       .Select("displayName,id,mail,calendars")
                                       .GetAsync();

                
                // Get the events

                //Get My events
                try
                {
                    var k = await graphClient.Me.Calendars.Request().Select("owner,name,id").GetAsync();
                    var k1 = await graphClient.Me.CalendarGroups.Request().Select("name,id").GetAsync();

                    foreach(var ev in k.CurrentPage)
                    {
                        var pp = await graphClient.Me.Calendars[ev.Id].Events.Request()
                            .Select("subject,organizer,start,end")
                        .OrderBy("createdDateTime DESC")
                        .GetAsync();
                    }
                    foreach (var ev in k1.CurrentPage)
                    {
                        var pp = await graphClient.Me.CalendarGroups[ev.Id].Calendars.Request()
                        //    .Select("subject,organizer,start,end")
                        //.OrderBy("createdDateTime DESC")
                        .GetAsync();
                    }

                    var events = await graphClient.Me.Events.Request()
                   .Select("subject,organizer,start,end")
                   .OrderBy("createdDateTime DESC")
                   .GetAsync();

                   
                    var evList = new List<Event>();
                    //go through events from user to create calendar
                    foreach (var ev in events.CurrentPage)
                    {
                        evList.Add(new Event(DateTime.Parse(ev.Start.DateTime), DateTime.Parse(ev.End.DateTime), ev.Subject, ev.Body?.ToString()));
                    }
                    //set calendar
                    var cal = new Calendar(me.Id, me.DisplayName)
                    {
                        EventsList = evList
                    };
                    CalendarList.Add(cal);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    //empty catch
                }

                var users1 = await graphClient.Users.Request()
                                    .Select("displayName,id,mail,calendars,outlook,calendarview")
                                    .GetAsync();
                var userlist = users1.CurrentPage.ToList();
                foreach (var u in userlist.Where(x=>x.DisplayName != me.DisplayName))
                {
                    try
                    {
                        var adress = new EmailAddress();
                        adress.Address = u.Mail;
                        //var ab = new Attendee() { EmailAddress = adress, Type = AttendeeType.Required };
                        //var s = new List<Attendee>();
                        //s.Add(ab);
                        //GetAvailabilityDetails();
                        var check = await graphClient.Users[u.Id].Calendar.Request().GetAsync();
                        var pp = await graphClient.Users[u.Id].Events.Request()
                            .Select("subject,organizer,start,end")
                        .OrderBy("createdDateTime DESC")
                        .GetAsync();
                        var evList = new List<Event>();
                        //go through events from user to create calendar
                        foreach (var ev in pp.CurrentPage)
                        {
                            evList.Add(new Event(DateTime.Parse(ev.Start.DateTime), DateTime.Parse(ev.End.DateTime), ev.Subject, ev.Body?.ToString()));
                        }
                        //set calendar
                        var cal = new Calendar(u.Id, u.DisplayName)
                        {
                            EventsList = evList
                        };
                        CalendarList.Add(cal);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        //empty catch
                    }

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }
    }
}
