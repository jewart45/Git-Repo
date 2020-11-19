using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TaskInstanceQuery
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Settings Settings = new Settings();
        SettingsProperties SettingsProperties;
        string settingsFilePath = @"C:\Users\Public\Documents\TaskQuerySettings.xml";
        public MainWindow()
        {
            InitializeComponent();
            LoadSettings(settingsFilePath, Settings);
            SettingsProperties.PropertyChanged += SettingsProperties_PropertyChanged;
        }

        private void SettingsProperties_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "ProcessToCheck":
                    Settings.ProcessToCheck = SettingsProperties.ProcessToCheck;
                    break;
                case "ServersToQuery":
                    Settings.ServersToQuery = SettingsProperties.ServersToQuery;
                    break;
                case "CheckLocal":
                    Settings.CheckLocal = SettingsProperties.CheckLocal;
                    break;
                default:
                    break;
            }
            SaveSettings(Settings, settingsFilePath);
        }

        private void SaveSettings(Settings settings, string settingsFilePath)
        {
            var serl = new CustomXmlSerializer(settingsFilePath, settings.GetType());
            serl.Serialize(settings);
        }

        private void LoadSettings(string settingsFilePath, Settings s)
        {
            var serl = new CustomXmlSerializer(settingsFilePath, s.GetType());
            try
            {
                if (!File.Exists(settingsFilePath))
                {
                    serl.Serialize(s);
                }
                else
                {
                    Settings = (Settings)serl.Deserialize();
                }
            }
            catch
            {
                Console.WriteLine("Unable to load settings, creating fresh settings");
                Settings = new Settings();
            }
            SettingsProperties = new SettingsProperties(Settings);

            this.DataContext = SettingsProperties;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //clear out children
            processStackPanel.Children.Clear();

            var servers = SettingsProperties.CheckLocal ? new string[1] {"localhost" } : SettingsProperties.ServersToQuery.Replace(" ", string.Empty).Split(',');
            var processes = SettingsProperties.ProcessToCheck.Replace(" ", string.Empty).ToLower().Split(',');
            foreach (var serv in servers)
            {
                Process[] processlist;
                try
                {
                    if (SettingsProperties.CheckLocal)
                    {
                        processlist = Process.GetProcesses();
                    }
                    else
                    {
                        processlist = Process.GetProcesses(serv);
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Could not show processes, check machine name or permissions", "Error finding processes");
                    break;
                }
                
               

                foreach (Process theprocess in processlist)
                {
                    if (processes.Any(x => theprocess.ProcessName.ToLower().Contains(x)))
                    {
                        var str = $"{serv}: {theprocess.MachineName} : {theprocess.ProcessName}";
                        processStackPanel.Children.Add(new Label() { Content = str, HorizontalContentAlignment = HorizontalAlignment.Left });

                    }
                }
                if(processStackPanel.Children.Count <= 0)
                {
                    processStackPanel.Children.Add(new Label() { Content = "No processes found with that name", HorizontalContentAlignment = HorizontalAlignment.Left, Foreground = Brushes.Red });

                }

                lblProcessCount.Content = $"Total Processes Found : {processStackPanel.Children.Count}";
                
            }

            
        }
    }
}
