using System;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI;
using Windows.UI.Xaml;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace AttaGlance
{
    public sealed partial class EventBlock : UserControl
    {
        public EventBlock (DateTime start, DateTime end, string Subject = "", string Description = "",string id = null)
        {
            Id = id;
            this.InitializeComponent();
            Properties = new EventBlockViewModel(start, end, Subject, Description);
            this.DataContext = Properties;
            MainGrid.PointerEntered += PointerEnter;
            MainGrid.PointerCaptureLost += PointerExited;
        }


        private void PointerExited(object sender, PointerRoutedEventArgs e)
        {
            //Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Hand, 0);
            MainGrid.Background = new SolidColorBrush(Colors.Blue);
            outsideGrid.Background = new SolidColorBrush(Colors.LightGray);
        }

        private void PointerEnter(object sender, PointerRoutedEventArgs e)
        {
            //Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
            MainGrid.Background = new SolidColorBrush(Colors.DarkRed);
            outsideGrid.Background = new SolidColorBrush(Colors.Transparent);
        }

        public EventBlock()
        {
            Id = "";
            this.InitializeComponent();
            Properties = new EventBlockViewModel(DateTime.Now, DateTime.Now, "New Subject", "New Description");
            this.DataContext = Properties;
        }


        public string Id { get; set; }
        public EventBlockViewModel Properties { get; private set; }

        private void Grid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {

        }
    }
}
