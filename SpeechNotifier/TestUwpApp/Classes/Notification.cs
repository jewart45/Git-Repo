using System;
using Windows.UI.Notifications;

namespace SpeechNotifierWin10.Classes
{
    public static class Notificationer
    {
        public static void ShowNotification(string title, string content)
        {
            //ToastVisual visual = new ToastVisual()
            //{
            //    BindingGeneric = new ToastBindingGeneric()
            //    {
            //        Children =
            //    {
            //        new AdaptiveText()
            //        {
            //            Text = title
            //        },

            //        new AdaptiveText()
            //        {
            //            Text = content
            //        },
            //    },
            //    }
            //};

            //ToastContent toastContent = new ToastContent()
            //{
            //    Visual = visual,

            //    // Arguments when the user taps body of toast
            //    Launch = new QueryString()
            //    {
            //        { "action", "viewConversation" }

            //    }.ToString()
            //};

            //// And create the toast notification
            //var toast = new ToastNotification(toastContent.GetXml());

            //toast.ExpirationTime = DateTime.Now.AddMinutes(10);
            //toast.Group = "Speech Notification";

            //ToastNotificationManager.CreateToastNotifier().Show(toast);
        }
    }
}