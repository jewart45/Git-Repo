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
        public EventBlockViewModel()
        {

        }
    }
}
