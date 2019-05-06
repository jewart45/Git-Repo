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
    public class EventDetailsViewModel
    {
        public IReactiveProperty<string> EventHeading { get; } = new ReactiveProperty<string>();
        public IReactiveProperty<string> EventSubHeading { get; } = new ReactiveProperty<string>();
        public IReactiveProperty<string> EventDescription { get; } = new ReactiveProperty<string>();

        public EventDetailsViewModel()
        {

        }

        public EventDetailsViewModel(string heading, string subHeading, string description)
        {
            EventHeading.Value = heading;
            EventSubHeading.Value = subHeading;
            EventDescription.Value = description;
        }
    }
}
