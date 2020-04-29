using BetHistoryImport.Classes;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BetHistoryImport
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var fileContent = string.Empty;
            var filePath = string.Empty;

            using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\Users\\jewar\\Downloads\\BASIC\\2020";
                openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    //Get the path of specified file
                    filePath = openFileDialog.FileName;

                    //Read the contents of the file into a stream
                    var fileStream = openFileDialog.OpenFile();

                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        
                        fileContent = reader.ReadToEnd();
                    }
                }
            }
            var content = fileContent.Split('\n');
            List<ImportPoint> objList = new List<ImportPoint>();

            var ev = JsonConvert.DeserializeObject<ImportEvent>(content.First());

            foreach (var line in content.Skip(1))
            {
                var converted = JsonConvert.DeserializeObject<ImportPoint>(line);
                objList.Add(converted);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void filteredListLbl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void allSelectionsLbl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void CompetitionTypeSelectorCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
