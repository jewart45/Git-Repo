using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SportsBettingModule.Templates
{
    /// <summary>
    /// Interaction logic for DataCell.xaml
    /// </summary>
    public partial class DataCell : Window
    {
        private string text = "#####";
        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                text = value;
                UpdateColour();
            }
        }
        public DataCell()
        {
            InitializeComponent();
            contentLbl.Content = Text;
            UpdateColour();
        }

        private void UpdateColour()
        {
            Task.Run(async () =>
            {
                backgroundGrid.Opacity = 1;
                for (double i = 0; i <= 0; i = i + 0.01)
                {
                    backgroundGrid.Opacity = 1 - i;
                    await Task.Delay(10);
                }
            });
            
        }
    }
}
