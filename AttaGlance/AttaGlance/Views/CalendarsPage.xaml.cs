using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace AttaGlance
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CalendarsPage : Page
    {
        public CalendarsPage()
        {
            this.InitializeComponent();
            this.DataContext = new CalendarsPageViewModel(this);
            

            if ((App.Current as App).IsAuthenticated)
            {
                DataTemplate calendarsTemplate = new DataTemplate();
            }



        }

        public void Refresh()
        {

        }

        internal void SetGrid(List<Calendar> calendarList)
        {
            foreach(var col in dataGrid.Columns)
            {
                var k = col;
            }
            //var col1 = new DataGridTemplateColumn();
            //FrameworkElementFactory factory1 = new FrameworkElementFactory(typeof(APButton.UserControl1));
            //DataTemplate cellTemplate1 = new DataTemplate();
            //cellTemplate1.VisualTree = factory1;
            //col1.CellTemplate = cellTemplate1;
            //MyDataGrid.Columns.Add(col1);

        }
    }
}
