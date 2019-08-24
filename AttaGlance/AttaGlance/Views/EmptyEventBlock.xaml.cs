using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace AttaGlance
{
    public sealed partial class EmptyEventBlock : UserControl
    {
        public Attendee Attendee { get; set; }
        public DateTime Date { get; set; }
        public EmptyEventBlock()
        {
            this.InitializeComponent();
            MainGrid.PointerEntered += PointerEnter;
            MainGrid.PointerExited += PointerExited;
            
        }

        public EmptyEventBlock(Attendee at)
        {
            this.InitializeComponent();
            MainGrid.PointerEntered += PointerEnter;
            MainGrid.PointerExited += PointerExited;
            Attendee = at;
        }

        private void PointerExited(object sender, PointerRoutedEventArgs e)
        {
            //Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Hand, 0);
            MainGrid.Background = new SolidColorBrush(Colors.Transparent);
        }

        private void PointerEnter(object sender, PointerRoutedEventArgs e)
        {
            //Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
            MainGrid.Background = new SolidColorBrush(Colors.LightGray);
        }

        private void TextBlock_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Hand, 0);
            txtBlock.Foreground = new SolidColorBrush(Colors.DarkBlue);
        }

        private void TextBlock_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
            txtBlock.Foreground = new SolidColorBrush(Colors.Blue);

        }

        private async void TextBlock_PointerPressedAsync(object sender, PointerRoutedEventArgs e)
        {

            ContentDialog meetingRequest = new MeetingRequest(Attendee, Date);
            ContentDialogResult result = await meetingRequest.ShowAsync();
            

        }
    }
}
