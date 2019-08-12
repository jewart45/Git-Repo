
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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
            ViewModel = new CalendarsPageViewModel(this);
            this.DataContext = ViewModel;
            

            if ((App.Current as App).IsAuthenticated)
            {
                DataTemplate calendarsTemplate = new DataTemplate();
            }



        }

        public CalendarsPageViewModel ViewModel { get; private set; }

        public void Refresh() => ViewModel.SetUpPageAsync();

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
