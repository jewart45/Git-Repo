using System;
using Microsoft.Toolkit.Services.MicrosoftGraph;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.Graph;

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

        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            var invokedItem = args.InvokedItem as string;

            switch (invokedItem.ToLower())
            {
                case "calendar":
                    SetUpEventsAsync();
                    //RootFrame.Navigate(typeof(CalendarPage));
                    break;
                case "home":
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
                    var adress = new EmailAddress();
                    adress.Address = u.Mail;
                    var ab = new Attendee() { EmailAddress = adress, Type = AttendeeType.Required };
                    var s = new List<Attendee>();
                    s.Add(ab);
                    //GetAvailabilityDetails();
                    var pp = graphClient.Users[u.Id].FindMeetingTimes(s).Request();

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
    
