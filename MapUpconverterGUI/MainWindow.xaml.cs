using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;

namespace MapUpconverterGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string toolFolder;
        public MainWindow()
        {
            InitializeComponent();

            Title = "Map Upconverter GUI v" + Version.SharedVersion;

            toolFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly()?.Location) ?? "";

            MapUpconverter.Settings.Load(toolFolder);

            InputDir.Text = MapUpconverter.Settings.InputDir;
            OutputDir.Text = MapUpconverter.Settings.OutputDir;

            ConvertOnSaveCheckbox.IsChecked = MapUpconverter.Settings.ConvertOnSave;

            MapName.Text = MapUpconverter.Settings.MapName;

            EpsilonDir.Text = MapUpconverter.Settings.EpsilonDir;
            PatchName.Text = MapUpconverter.Settings.EpsilonPatchName;
            WDTFileDataID.Text = MapUpconverter.Settings.RootWDTFileDataID.ToString();

            SaveButton.IsEnabled = false;
        }

        private void InputDirButton_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new OpenFolderDialog
            {
                Title = "Select input folder (e.g. Noggit project folder)",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer)
            };

            if (folderDialog.ShowDialog() == true)
            {
                InputDir.Text = folderDialog.FolderName;
                ResetSaveButton();
            }
        }

        private void OutputDirButton_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new OpenFolderDialog
            {
                Title = "Select output folder (e.g. Epsilon patch directory)",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer)
            };

            if (folderDialog.ShowDialog() == true)
            {
                OutputDir.Text = folderDialog.FolderName;
                ResetSaveButton();
            }
        }

        private void EpsilonDirButton_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new OpenFolderDialog
            {
                Title = "Select Epsilon.exe folder",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer)
            };

            if (folderDialog.ShowDialog() == true)
            {
                var folderName = folderDialog.FolderName;

                if (!File.Exists(Path.Combine(folderName, "Epsilon.exe")))
                {
                    MessageBox.Show("Epsilon.exe not found in this folder, please select the correct folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    EpsilonDir.Text = "";
                }
                else
                {
                    EpsilonDir.Text = folderName;
                    ResetSaveButton();
                }
            }
        }

        private void InputDir_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            MapUpconverter.Settings.InputDir = InputDir.Text;
            ResetSaveButton();
        }

        private void OutputDir_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            MapUpconverter.Settings.OutputDir = OutputDir.Text;
            ResetSaveButton();
        }

        private void MapName_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            MapUpconverter.Settings.MapName = MapName.Text;
            ResetSaveButton();
        }

        private void EpsilonDir_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            MapUpconverter.Settings.EpsilonDir = EpsilonDir.Text;
            ResetSaveButton();
        }

        private void PatchName_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            MapUpconverter.Settings.EpsilonPatchName = PatchName.Text;
            ResetSaveButton();
        }

        private void WDTFileDataID_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if(uint.TryParse(WDTFileDataID.Text.Trim(), out var cleanedID))
            {
                WDTFileDataID.Text = cleanedID.ToString();
                MapUpconverter.Settings.RootWDTFileDataID = cleanedID;
                ResetSaveButton();
            }
            else
            {
                WDTFileDataID.Text = "";
            }
        }

        private void Checkbox_Changed(object sender, RoutedEventArgs e)
        {
            MapUpconverter.Settings.ConvertOnSave = ConvertOnSaveCheckbox.IsChecked == true;
            ResetSaveButton();
        }

        private void WDTFileDataID_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !uint.TryParse(e.Text, out _);
        }

        private void MapName_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsLetterOrDigit);
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo() { UseShellExecute = true, FileName = e.Uri.AbsoluteUri });
            e.Handled = true;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
        }

        private void SaveSettings()
        {
            MapUpconverter.Settings.Save(toolFolder);
            SaveButton.Content = "Settings saved!";
            SaveButton.IsEnabled = false;
        }

        private void ResetSaveButton()
        {
            SaveButton.Content = "Save settings";
            SaveButton.IsEnabled = true;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();

            Process.Start(new ProcessStartInfo() { UseShellExecute = true, FileName = Path.Combine(toolFolder, "MapUpconverter.exe") });
            e.Handled = true;
        }
    }
}