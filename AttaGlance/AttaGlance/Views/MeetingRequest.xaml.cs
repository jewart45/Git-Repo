using Microsoft.Graph;
using Microsoft.Toolkit.Services.MicrosoftGraph;
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

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace AttaGlance
{
    public sealed partial class MeetingRequest : ContentDialog
    {
        private GraphServiceClient GraphClient = MicrosoftGraphService.Instance.GraphProvider;
        public MeetingRequest()
        {
            Emails.Add(new Attendee
            {
                EmailAddress = new EmailAddress
                {
                    Name = "Josh Ewart",
                    Address = "j.ewart@aicsolutions.com"
                },
                Type = AttendeeType.Required
            });
            this.InitializeComponent();
            this.DataContext = this;
        }

        public List<Attendee> Emails { get; set; } = new List<Attendee>();
        public string Subject { get; set; }
        public string Body { get; set; }
        public TimeSpan From { get; set; }
        public TimeSpan To { get; set; }
        public string Location { get; set; }

        public MeetingRequest(Attendee ea, DateTime date)
        {
            this.InitializeComponent();
            Emails.Add(new Attendee
            {
                EmailAddress = new EmailAddress
                {
                    Name = "Josh Ewart",
                    Address = "j.ewart@aicsolutions.com"
                },
                Type = AttendeeType.Required
            });
            datePicker.Date = date;

            //Emails.Add(ea);

            this.DataContext = this;
        }

        public Event CreatedEvent { get; set; }

        private async void ContentDialog_PrimaryButtonClickAsync(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            DateTime from = GetFrom();
            DateTime to = GetTo();
            var @event = new Microsoft.Graph.Event
            {
                Subject = Subject ?? "",
                Body = new ItemBody
                {
                    ContentType = BodyType.Html,
                    Content = Body ?? ""
                },
                Start = new DateTimeTimeZone
                {
                    DateTime = from.ToString(),
                    TimeZone = TimeZoneInfo.Local.StandardName.ToString()
                },
                End = new DateTimeTimeZone
                {
                    DateTime = to.ToString(),
                    TimeZone = TimeZoneInfo.Local.StandardName.ToString()
                },
                Location = new Location
                {
                    DisplayName = Location
                },
                Attendees = Emails as IEnumerable<Attendee>
            };

            await GraphClient.Me.Calendar.Events
                .Request()
                .AddAsync(@event);

        }

        private DateTime GetFrom()
        {
            return datePicker.Date.GetValueOrDefault().Date.Add(fromPicker.Time);
        }
        private DateTime GetTo()
        {
            return datePicker.Date.GetValueOrDefault().Date.Add(toPicker.Time);
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var at = new Attendee()
            {
                EmailAddress = new EmailAddress()
                {
                    Address = "Email",
                    Name = "Name"
                },
                Type = AttendeeType.Required

            };
        }
    }
}
