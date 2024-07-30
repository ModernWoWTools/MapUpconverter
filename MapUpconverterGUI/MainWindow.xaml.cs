using MapUpconverter.Utils;
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

        private bool listfileNeedsDownload = true;
        private bool heightInfoNeedsDownload = true;
        private bool groundEffectInfoNeedsDownload = true;
        private bool modelBlobNeedsDownload = true;

        private static bool isRunning = false;

        public MainWindow()
        {
            InitializeComponent();

            Title = "Map Upconverter GUI v" + Version.SharedVersion;

            toolFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly()?.Location) ?? "";

            try
            {
                if (!File.Exists(Path.Combine(toolFolder, "settings.json")))
                    MapUpconverter.Settings.Save(toolFolder);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving new settings file: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown(1);
            }

            try
            {
                MapUpconverter.Settings.Load(toolFolder);

                InputDir.Text = MapUpconverter.Settings.InputDir;
                OutputDir.Text = MapUpconverter.Settings.OutputDir;

                GenerateWDTWDLCheckbox.IsChecked = MapUpconverter.Settings.GenerateWDTWDL;
                ConvertOnSaveCheckbox.IsChecked = MapUpconverter.Settings.ConvertOnSave;

                MapName.Text = MapUpconverter.Settings.MapName;

                EpsilonDir.Text = MapUpconverter.Settings.EpsilonDir;
                PatchName.Text = MapUpconverter.Settings.EpsilonPatchName;
                WDTFileDataID.Text = MapUpconverter.Settings.RootWDTFileDataID.ToString();

                ClientRefreshEnabled.IsChecked = MapUpconverter.Settings.ClientRefresh;
                MapID.Text = MapUpconverter.Settings.MapID.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading settings file, try deleting it and restarting. Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown(1);
            }

            SaveButton.IsEnabled = false;
            StartButton.IsEnabled = false;

            try
            {
                Downloads.Initialize(toolFolder);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error initializing downloads: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown(1);
            }

            try
            {
                CheckRequiredFiles();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error checking required files: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown(1);
            }
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
            if (uint.TryParse(WDTFileDataID.Text.Trim(), out var cleanedID))
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

        private void MapID_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (int.TryParse(MapID.Text.Trim(), out var cleanedID))
            {
                MapID.Text = cleanedID.ToString();
                MapUpconverter.Settings.MapID = cleanedID;
                ResetSaveButton();
            }
            else
            {
                MapID.Text = "";
            }
        }

        private void Checkbox_Changed(object sender, RoutedEventArgs e)
        {
            MapUpconverter.Settings.ConvertOnSave = ConvertOnSaveCheckbox.IsChecked == true;
            ResetSaveButton();
        }

        private void ClientRefreshEnabled_Checked(object sender, RoutedEventArgs e)
        {
            MapUpconverter.Settings.ClientRefresh = ClientRefreshEnabled.IsChecked == true;
            ResetSaveButton();
        }

        private void GenerateWDTWDL_Checked(object sender, RoutedEventArgs e)
        {
            MapUpconverter.Settings.GenerateWDTWDL = GenerateWDTWDLCheckbox.IsChecked == true;
            ResetSaveButton();
        }

        private void WDTFileDataID_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !uint.TryParse(e.Text, out _);
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

        private void CheckRequiredFiles()
        {
            listfileNeedsDownload = !File.Exists(Path.Combine(toolFolder, "meta", "listfile.csv"));
            if (!listfileNeedsDownload)
            {
                var listfileLastWriteTime = File.GetLastWriteTime(Path.Combine(toolFolder, "meta", "listfile.csv"));
                var listfileNeedsUpdate = (listfileLastWriteTime - DateTime.Now).TotalDays > 30;
                if (listfileNeedsUpdate)
                {
                    ListfileLabel.Content = "Listfile exists on disk";
                    ListfileButton.Content = "Update";
                }
                else
                {
                    ListfileLabel.Content = "Listfile exists on disk";
                    ListfileButton.Content = "Redownload";
                }

                ListfileLabel.FontWeight = FontWeights.Normal;
                ListfileButton.FontWeight = FontWeights.Normal;
            }
            else
            {
                ListfileLabel.Content = "Listfile is missing!";
                ListfileLabel.FontWeight = FontWeights.Bold;
                ListfileButton.Content = "Download";
                ListfileButton.FontWeight = FontWeights.Bold;
            }

            heightInfoNeedsDownload = !File.Exists(Path.Combine(toolFolder, "meta", "TextureInfoByFilePath.json"));
            if (!heightInfoNeedsDownload)
            {
                var heightInfoLastWriteTime = File.GetLastWriteTime(Path.Combine(toolFolder, "meta", "TextureInfoByFilePath.json"));
                var heightInfoNeedsUpdate = (heightInfoLastWriteTime - DateTime.Now).TotalDays > 30;
                if (heightInfoNeedsUpdate)
                {
                    HeightInfoLabel.Content = "Height texture info exists on disk";
                    HeightInfoButton.Content = "Update";
                }
                else
                {
                    HeightInfoLabel.Content = "Height texture info exists on disk";
                    HeightInfoButton.Content = "Redownload";
                }

                HeightInfoLabel.FontWeight = FontWeights.Normal;
                HeightInfoButton.FontWeight = FontWeights.Normal;
            }
            else
            {
                HeightInfoLabel.Content = "Height texture info is missing!";
                HeightInfoLabel.FontWeight = FontWeights.Bold;
                HeightInfoButton.Content = "Download";
                HeightInfoButton.FontWeight = FontWeights.Bold;
            }

            groundEffectInfoNeedsDownload = !File.Exists(Path.Combine(toolFolder, "meta", "GroundEffectIDsByTextureFileID.json"));
            if (!groundEffectInfoNeedsDownload)
            {
                var groundEffectInfoLastWriteTime = File.GetLastWriteTime(Path.Combine(toolFolder, "meta", "GroundEffectIDsByTextureFileID.json"));
                var groundEffectInfoNeedsUpdate = (groundEffectInfoLastWriteTime - DateTime.Now).TotalDays > 30;
                if (groundEffectInfoNeedsUpdate)
                {
                    GroundEffectLabel.Content = "Ground effect info exists on disk";
                    GroundEffectButton.Content = "Update";
                }
                else
                {
                    GroundEffectLabel.Content = "Ground effect info exists on disk";
                    GroundEffectButton.Content = "Redownload";
                }

                GroundEffectLabel.FontWeight = FontWeights.Normal;
                GroundEffectButton.FontWeight = FontWeights.Normal;
            }
            else
            {
                GroundEffectLabel.Content = "Ground effect info is missing!";
                GroundEffectLabel.FontWeight = FontWeights.Bold;
                GroundEffectButton.Content = "Download";
                GroundEffectButton.FontWeight = FontWeights.Bold;
            }

            modelBlobNeedsDownload = !File.Exists(Path.Combine(toolFolder, "meta", "blob.json"));
            if (!modelBlobNeedsDownload)
            {
                var modelBlobLastWriteTime = File.GetLastWriteTime(Path.Combine(toolFolder, "meta", "blob.json"));
                var modelBlobNeedsUpdate = (modelBlobLastWriteTime - DateTime.Now).TotalDays > 30;
                if (modelBlobNeedsUpdate)
                {
                    ModelBlobLabel.Content = "Model blob exists on disk";
                    ModelBlobButton.Content = "Update";
                }
                else
                {
                    ModelBlobLabel.Content = "Model blob exists on disk";
                    ModelBlobButton.Content = "Redownload";
                }

                ModelBlobLabel.FontWeight = FontWeights.Normal;
                ModelBlobButton.FontWeight = FontWeights.Normal;
            }
            else
            {
                ModelBlobLabel.Content = "Model blob is missing!";
                ModelBlobLabel.FontWeight = FontWeights.Bold;
                ModelBlobButton.Content = "Download";
                ModelBlobButton.FontWeight = FontWeights.Bold;
            }

            if (!listfileNeedsDownload && !heightInfoNeedsDownload && !modelBlobNeedsDownload)
            {
                StartButton.IsEnabled = true;
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();

            if (isRunning)
            {
                MessageBox.Show("Converter is already running, please close the existing instance first.", "Already running", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var converter = Process.Start(new ProcessStartInfo() { UseShellExecute = true, FileName = Path.Combine(toolFolder, "MapUpconverter.exe") });

            converter!.EnableRaisingEvents = true;

            isRunning = true;

            converter.Exited += (s, e) =>
            {
                isRunning = false;
            };

            e.Handled = true;
        }

        private async void ListfileButton_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            ListfileButton.IsEnabled = false;
            ListfileButton.Content = "Downloading...";
            try
            {
                await Downloads.DownloadListfile(toolFolder);
            }
            catch (Exception ex)
            {
                ListfileLabel.Content = "Error downloading";
                MessageBox.Show("Error downloading listfile: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            ListfileButton.IsEnabled = true;
            ListfileButton.Content = "Redownload";

            CheckRequiredFiles();
        }

        private async void GroundEffectButton_Click(object sender, RoutedEventArgs e)
        {
            GroundEffectButton.IsEnabled = false;
            GroundEffectButton.Content = "Downloading...";
            try
            {
                await Downloads.DownloadGroundEffectInfo(toolFolder);
            }
            catch (Exception ex)
            {
                GroundEffectLabel.Content = "Error downloading";
                MessageBox.Show("Error downloading ground effect info: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            GroundEffectButton.IsEnabled = true;
            GroundEffectButton.Content = "Redownload";

            CheckRequiredFiles();
        }


        private async void HeightInfoButton_Click(object sender, RoutedEventArgs e)
        {
            HeightInfoButton.IsEnabled = false;
            HeightInfoButton.Content = "Downloading...";
            try
            {
                await Downloads.DownloadHeightTextureInfo(toolFolder);
            }
            catch (Exception ex)
            {
                HeightInfoLabel.Content = "Error downloading";
                MessageBox.Show("Error downloading height texture info: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            HeightInfoButton.IsEnabled = true;
            HeightInfoButton.Content = "Redownload";

            CheckRequiredFiles();
        }

        private async void ModelBlobButton_Click(object sender, RoutedEventArgs e)
        {
            ModelBlobButton.IsEnabled = false;
            ModelBlobButton.Content = "Downloading...";
            try
            {
                await Downloads.DownloadModelBlob(toolFolder);
            }
            catch (Exception ex)
            {
                ModelBlobLabel.Content = "Error downloading";
                MessageBox.Show("Error downloading model blob: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            ModelBlobButton.IsEnabled = true;
            ModelBlobButton.Content = "Redownload";

            CheckRequiredFiles();
        }
    }
}