using System.Threading.Tasks;
using System.Windows;

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