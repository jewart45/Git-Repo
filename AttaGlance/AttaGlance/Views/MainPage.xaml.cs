using System;
using Microsoft.Toolkit.Services.MicrosoftGraph;
using Windows.UI.Xaml.Controls;
using System.Linq;
using Microsoft.Graph;
using System.Collections.Generic;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace AttaGlance
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            // Initialize auth state to false
            SetAuthState(false);
            // Load OAuth settings
            var oauthSettings = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView("OAuth");
            var appId = oauthSettings.GetString("AppId");
            var scopes = oauthSettings.GetString("Scopes");

            if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(scopes))
            {
                Notification.Show("Could not load OAuth Settings from resource file.");
            }
            else
            {
                // Initialize Graph
                MicrosoftGraphService.Instance.AuthenticationModel = MicrosoftGraphEnums.AuthenticationModel.V2;
                MicrosoftGraphService.Instance.Initialize(appId,
                    MicrosoftGraphEnums.ServicesToInitialize.UserProfile,
                    scopes.Split(' '));

                // Navigate to HomePage.xaml
                RootFrame.Navigate(typeof(HomePage));
            }
        }


        private void SetAuthState(bool isAuthenticated)
        {
            (App.Current as App).IsAuthenticated = isAuthenticated;
            // Toggle controls that require auth
            Calendar.IsEnabled = isAuthenticated;
        }

        private async void NavView_ItemInvokedAsync(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            string invokedItem ="";
            if (args.InvokedItem is NavigationViewItem v)
            {
                invokedItem = v.Content as string;
            }
            else
            {
                invokedItem = args.InvokedItem as string;
            }


            switch (invokedItem.ToLower())
            {
                case "calendar":
                    RootFrame.Navigate(typeof(CalendarPage));
                    break;
                case "calendars":
                    RootFrame.Navigate(typeof(CalendarsPage));
                    break;
                case "home":
                case "settings":
                    RootFrame.Navigate(typeof(HomePage));
                    break;
                default:
                    RootFrame.Navigate(typeof(HomePage));
                    break;
            }
        }

        private async System.Threading.Tasks.Task SetUpEventsAsync()
        {
            var graphClient = MicrosoftGraphService.Instance.GraphProvider;
            try
            {
                // Get the events
                var events = await graphClient.Me.Events.Request()
                    .Select("subject,organizer,start,end")
                    .OrderBy("createdDateTime DESC")
                    .GetAsync();

                var users1 = await graphClient.Users.Request()
                                    .Select("displayName,id,mail,calendar")
                                    .GetAsync();
                var userlist = users1.CurrentPage.ToList();
                foreach (var u in userlist)
                {
                    try
                    {
                        var adress = new EmailAddress();
                        adress.Address = u.Mail;
                        //var ab = new Attendee() { EmailAddress = adress, Type = AttendeeType.Required };
                        //var s = new List<Attendee>();
                        //s.Add(ab);
                        //GetAvailabilityDetails();
                        var pp = await graphClient.Users[u.Id].Events.Request()
                            .Select("subject,organizer,start,end")
                        .OrderBy("createdDateTime DESC")
                        .GetAsync();
                        var evList = new List<Event>();
                        //go through events from user to create calendar
                        foreach(var ev in pp.CurrentPage)
                        {
                            evList.Add(new Event(DateTime.Parse(ev.Start.DateTime), DateTime.Parse(ev.End.DateTime), ev.Subject, ev.Body?.ToString()));
                        }
                        //set calendar
                        var cal = new Calendar(u.Id, u.DisplayName)
                        {
                            EventsList = evList
                        };
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        //empty catch
                    }
                }

                            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            //base.OnNavigatedTo(e);
        }

        private void Login_SignInCompleted(object sender, Microsoft.Toolkit.Uwp.UI.Controls.Graph.SignInEventArgs e)
        {
            // Set the auth state
            SetAuthState(true);
            // Reload the home page
            RootFrame.Navigate(typeof(HomePage));
        }

        private void Login_SignOutCompleted(object sender, EventArgs e)
        {
            // Set the auth state
            SetAuthState(false);
            // Reload the home page
            RootFrame.Navigate(typeof(HomePage));

        }
    }
}
    
