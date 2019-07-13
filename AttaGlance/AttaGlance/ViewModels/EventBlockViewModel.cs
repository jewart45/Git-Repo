using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.UI;
using Windows.UI.Xaml;

namespace AttaGlance
{
    public class EventBlockViewModel
    {
        public IReactiveProperty<Visibility> BlockVisibility { get; } = new ReactiveProperty<Visibility>(Visibility.Collapsed);

        public IReactiveProperty<Color> Background { get; } = new ReactiveProperty<Color>(Colors.White); 
        public IReactiveProperty<string> Subject { get; } = new ReactiveProperty<string>("No Subject");

        public IReactiveProperty<string> Description { get; } = new ReactiveProperty<string>("");
        public EventBlockViewModel(string subject = "No Subject", string description = "")
        {
            Subject.Value = subject;
            Description.Value = description;
        }
    }
}
