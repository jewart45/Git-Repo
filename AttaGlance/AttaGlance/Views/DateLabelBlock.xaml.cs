using Reactive.Bindings;
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
    public sealed partial class DateLabelBlock : UserControl
    {
        public DateTime Time { get; set; }

        public string Label { get; set; } = "January 1 1999";
        public DateLabelBlock()
        {
            this.InitializeComponent();
        }

        public DateLabelBlock(DateTime dt)
        {
            this.InitializeComponent();
            Time = dt;
            Label = dt.ToString("dd MMM yyyy");
            labelTxt.Text = Label;

        }
    }
}
