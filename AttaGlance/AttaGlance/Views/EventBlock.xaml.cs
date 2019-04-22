using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace AttaGlance
{
    public sealed partial class EventBlock : UserControl
    {
        public EventBlock()
        {
            this.InitializeComponent();
        }

        private void Grid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {

        }
    }
}
