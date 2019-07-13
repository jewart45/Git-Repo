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
    public class CalendarViewModel
    {
        public IReactiveProperty<Visibility> BlockVisibility { get; } = new ReactiveProperty<Visibility>(Visibility.Collapsed);

        public IReactiveProperty<Color> Background { get; } = new ReactiveProperty<Color>(Colors.White); 
        public IReactiveProperty<string> Name { get; } = new ReactiveProperty<string>("User");

        public IReactiveProperty<string> Id { get; } = new ReactiveProperty<string>("");

        public List<Event> Events { get; set; } = new List<Event>();
        public CalendarViewModel(string name = "User", string id = "")
        {
            Name.Value = name;
            Id.Value = id;
        }
    }
}
