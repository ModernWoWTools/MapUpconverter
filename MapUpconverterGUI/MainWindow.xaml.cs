using MapUpconverter;
using MapUpconverter.Arctium;
using MapUpconverter.Epsilon;
using MapUpconverter.Utils;
using Microsoft.Win32;
using Newtonsoft.Json;
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
                ApplySettings();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading settings file, try deleting it and restarting. Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown(1);
            }

            UpdateAvailablePresets();
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

        private void ApplySettings()
        {
            InputDir.Text = MapUpconverter.Settings.InputDir;
            OutputDir.Text = MapUpconverter.Settings.OutputDir;

            GenerateWDTWDLCheckbox.IsChecked = MapUpconverter.Settings.GenerateWDTWDL;
            ConvertOnSaveCheckbox.IsChecked = MapUpconverter.Settings.ConvertOnSave;

            MapName.Text = MapUpconverter.Settings.MapName;

            EpsilonDir.Text = MapUpconverter.Settings.EpsilonDir;
            PatchName.Text = MapUpconverter.Settings.EpsilonPatchName;

            LauncherDir.Text = MapUpconverter.Settings.ArctiumDir;
            ArctiumPatchName.Text = MapUpconverter.Settings.ArctiumPatchName;

            WDTFileDataID.Text = MapUpconverter.Settings.RootWDTFileDataID.ToString();
            ArctiumWDTFileDataID.Text = MapUpconverter.Settings.RootWDTFileDataID.ToString();

            ClientRefreshEnabled.IsChecked = MapUpconverter.Settings.ClientRefresh;
            CASRefreshEnabled.IsChecked = MapUpconverter.Settings.CASRefresh;

            MapID.Text = MapUpconverter.Settings.MapID.ToString();

            if (MapUpconverter.Settings.ExportTarget == "Epsilon")
            {
                EpsilonRadioButton.IsChecked = true;
            }
            else if (MapUpconverter.Settings.ExportTarget == "Arctium")
            {
                ArctiumRadioButton.IsChecked = true;
            }
            else
            {
                GenericRadioButton.IsChecked = true;
            }

            if (MapUpconverter.Settings.TargetVersion == 735)
            {
                LegionRadioButton.IsChecked = true;
            }
            else if (MapUpconverter.Settings.TargetVersion == 830)
            {
                BFARadioButton.IsChecked = true;
            }
            else if (MapUpconverter.Settings.TargetVersion == 927)
            {
                SLRadioButton.IsChecked = true;
            }
            else
            {
                SLRadioButton.IsChecked = true;
            }

            LightBaseAttenuation.Text = MapUpconverter.Settings.LightBaseAttenuationEnd.ToString();
            LightBaseIntensity.Text = MapUpconverter.Settings.LightBaseIntensity.ToString();

            AdvancedLightConfig.IsChecked = Settings.UseAdvancedLightConfig;

            BaseCustomFDID.Text = MapUpconverter.Settings.BaseCustomFDID.ToString();
        }

        private void UpdateAvailablePresets()
        {
            if (!Directory.Exists("presets"))
                return;

            var presets = Directory.GetFiles("presets", "*.json");

            AvailablePresets.Items.Clear();

            foreach (var preset in presets)
            {
                var presetName = Path.GetFileNameWithoutExtension(preset);
                AvailablePresets.Items.Add(presetName);
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
                var folderName = folderDialog.FolderName;

                if (Directory.GetFiles(folderName, "*.noggitproj").Length == 0)
                {
                    var messageDialog = MessageBox.Show("This folder is not a valid Noggit project directory (does not contain .noggitproj), are you sure you want to continue?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (messageDialog == MessageBoxResult.Yes)
                    {
                        InputDir.Text = folderDialog.FolderName;
                    }
                    else
                    {
                        InputDir.Text = "";
                    }
                }
                else
                {
                    InputDir.Text = folderDialog.FolderName;
                }

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

                if (!File.Exists(Path.Combine(folderName, "Epsilon.exe")) && !File.Exists(Path.Combine(folderName, "_retail_", "arbiterdll.dll")))
                {
                    MessageBox.Show("Epsilon.exe and/or Arbiter not found, please select the correct folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    EpsilonDir.Text = "";
                }
                else
                {
                    EpsilonDir.Text = folderName;
                    ResetSaveButton();
                }
            }
        }

        private void LauncherDirButton_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new OpenFolderDialog
            {
                Title = "Select Arctium WoW Launcher.exe folder",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer)
            };

            if (folderDialog.ShowDialog() == true)
            {
                var folderName = folderDialog.FolderName;

                if (!File.Exists(Path.Combine(folderName, "Arctium WoW Launcher.exe")))
                {
                    MessageBox.Show("Arctium WoW Launcher.exe not found in this folder, please select the correct folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    LauncherDir.Text = "";
                }
                else
                {
                    LauncherDir.Text = folderName;
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

        private void ArctiumPatchName_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            MapUpconverter.Settings.ArctiumPatchName = ArctiumPatchName.Text;
            ResetSaveButton();
        }

        private void LauncherDir_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            MapUpconverter.Settings.ArctiumDir = LauncherDir.Text;
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

        private void ArctiumWDTFileDataID_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (uint.TryParse(ArctiumWDTFileDataID.Text.Trim(), out var cleanedID))
            {
                ArctiumWDTFileDataID.Text = cleanedID.ToString();
                MapUpconverter.Settings.RootWDTFileDataID = cleanedID;
                ResetSaveButton();
            }
            else
            {
                ArctiumWDTFileDataID.Text = "";
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
                if (MapID.Text != "-")
                    MapID.Text = "";
            }
        }

        private void LightBaseIntensity_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (float.TryParse(LightBaseIntensity.Text.Trim(), out var cleanedIntensity))
            {
                LightBaseIntensity.Text = cleanedIntensity.ToString();
                MapUpconverter.Settings.LightBaseIntensity = cleanedIntensity;
                ResetSaveButton();
            }
            else
            {
                LightBaseIntensity.Text = "";
            }
        }

        private void LightBaseAttenuation_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (float.TryParse(LightBaseAttenuation.Text.Trim(), out var cleanedAttenuation))
            {
                LightBaseAttenuation.Text = cleanedAttenuation.ToString();
                MapUpconverter.Settings.LightBaseAttenuationEnd = cleanedAttenuation;
                ResetSaveButton();
            }
            else
            {
                LightBaseAttenuation.Text = "";
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

        private void CASRefreshEnabled_Checked(object sender, RoutedEventArgs e)
        {
            MapUpconverter.Settings.CASRefresh = CASRefreshEnabled.IsChecked == true;
            ResetSaveButton();
        }

        private void GenerateWDTWDL_Checked(object sender, RoutedEventArgs e)
        {
            MapUpconverter.Settings.GenerateWDTWDL = GenerateWDTWDLCheckbox.IsChecked == true;
            ResetSaveButton();
        }

        private void ExportTarget_Checked(object sender, RoutedEventArgs e)
        {
            //OutputDir.IsEnabled = false;
            //OutputDirButton.IsEnabled = false;
            if (EpsilonRadioButton.IsChecked == true)
            {
                MapUpconverter.Settings.ExportTarget = "Epsilon";
            }
            else if (ArctiumRadioButton.IsChecked == true)
            {
                MapUpconverter.Settings.ExportTarget = "Arctium";
            }
            else
            {
                //OutputDir.IsEnabled = true;
                //OutputDirButton.IsEnabled = true;
                MapUpconverter.Settings.ExportTarget = "Generic";
            }
            ResetSaveButton();
        }

        private void ExportVersion_Checked(object sender, RoutedEventArgs e)
        {
            if (LegionRadioButton.IsChecked == true)
            {
                MapUpconverter.Settings.TargetVersion = 735;
            }
            else if (BFARadioButton.IsChecked == true)
            {
                MapUpconverter.Settings.TargetVersion = 830;
            }
            else if (SLRadioButton.IsChecked == true)
            {
                MapUpconverter.Settings.TargetVersion = 927;
            }
            else
            {
                MapUpconverter.Settings.TargetVersion = 927;
            }

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

        private bool SaveSettings()
        {
            if (string.IsNullOrEmpty(MapName.Text))
            {
                MessageBox.Show("Map name is not allowed to be empty, please enter a map name.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            else if (string.IsNullOrEmpty(InputDir.Text))
            {
                MessageBox.Show("Input directory is not allowed to be empty, please select a directory.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            else
            {
                MapUpconverter.Settings.Save(toolFolder);
                SaveButton.Content = "Settings saved!";
                SaveButton.IsEnabled = false;
                return true;
            }
        }

        private void ResetSaveButton()
        {
            SaveButton.Content = "Apply changes";
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
            var settingsSaved = SaveSettings();

            if (!settingsSaved)
                return;

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

        private void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists("presets"))
                Directory.CreateDirectory("presets");

            var presetName = PresetName.Text;

            if (File.Exists(Path.Combine("presets", presetName + ".json")))
            {
                var overwritePrompt = MessageBox.Show("Preset with this name already exists, overwrite?", "Overwrite?", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (overwritePrompt == MessageBoxResult.No)
                    return;
            }

            MapUpconverter.Settings.Save("presets", presetName);

            UpdateAvailablePresets();
        }

        private void LoadSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var presetName = AvailablePresets.SelectedItem as string;

            if (string.IsNullOrEmpty(presetName))
                return;

            MapUpconverter.Settings.Load("presets", presetName);

            ApplySettings();
            SaveSettings();
        }

        private void DeleteSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var presetName = AvailablePresets.SelectedItem as string;

            if (string.IsNullOrEmpty(presetName))
                return;

            var deletePrompt = MessageBox.Show("Are you sure you want to delete this preset?", "Delete?", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (deletePrompt == MessageBoxResult.No)
                return;

            File.Delete(Path.Combine("presets", presetName + ".json"));

            UpdateAvailablePresets();
        }

        private void PresetName_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            SaveSettingsButton.IsEnabled = PresetName.Text.Length > 0;
        }

        private void AvailablePresets_Selected(object sender, RoutedEventArgs e)
        {
            LoadSettingsButton.IsEnabled = AvailablePresets.SelectedItem != null;
            DeleteSettingsButton.IsEnabled = AvailablePresets.SelectedItem != null;

            PresetPreview.Text = "";
            PresetName.Text = AvailablePresets.SelectedItem as string;

            PreviewSettings("presets", AvailablePresets.SelectedItem as string);
        }

        private void PreviewSettings(string folder, string presetName)
        {
            if (string.IsNullOrEmpty(presetName))
                return;

            var presetPath = Path.Combine(folder, presetName + ".json");

            if (!File.Exists(presetPath))
                return;

            var presetJSON = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(presetPath));

            PresetPreview.Text = "";

            foreach (var setting in presetJSON)
            {
                PresetPreview.Text += setting.Name + ": " + setting.Value + "\n";
            }
        }

        private async void ManualClientRefresh_Click(object sender, RoutedEventArgs e)
        {
            ManualClientRefresh.Content = "Refreshing...";
            ManualClientRefresh.IsEnabled = false;

            try
            {
                await Task.Run(() =>
                {
                    EpsilonConnection.Connect();

                    var allTiles = new List<(int TileID, int UpdateFlags)>();
                    for (var x = 0; x < 63; x++)
                    {
                        for (var y = 0; y < 63; y++)
                        {
                            allTiles.Add((x * 64 + y, 0x7)); // Update root, tex and obj
                        }
                    }

                    EpsilonConnection.RequestMapTileUpdate(MapUpconverter.Settings.MapID, allTiles);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error during refresh: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            ManualClientRefresh.Content = "Manual client refresh";
            ManualClientRefresh.IsEnabled = true;
        }

        private async void ManualPatchRefresh_Click(object sender, RoutedEventArgs e)
        {
            ManualPatchRefresh.Content = "Refreshing...";
            ManualPatchRefresh.IsEnabled = false;

            try
            {
                await Task.Run(() =>
                {
                    Downloads.Initialize(toolFolder);

                    if (!File.Exists(Path.Combine(toolFolder, "meta", "listfile.csv")))
                        throw new FileNotFoundException("Listfile not found, download it first");

                    Listfile.Initialize(toolFolder);

                    if (Settings.ExportTarget == "Epsilon")
                    {
                        PatchManifest.ScanUsedFileDataIDs();
                        PatchManifest.Update();
                    }
                    else if (Settings.ExportTarget == "Arctium")
                    {
                        FileMapping.Update();
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error during refresh: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            ManualPatchRefresh.Content = "Manual patch refresh";
            ManualPatchRefresh.IsEnabled = true;
        }

        private void AdvancedLightConfig_Checked(object sender, RoutedEventArgs e)
        {
            Settings.UseAdvancedLightConfig = AdvancedLightConfig.IsChecked == true;
            ResetSaveButton();
        }

        private void BaseCustomFDID_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (uint.TryParse(BaseCustomFDID.Text.Trim(), out var cleanedBaseCustomFDID))
            {
                BaseCustomFDID.Text = cleanedBaseCustomFDID.ToString();
                MapUpconverter.Settings.BaseCustomFDID = cleanedBaseCustomFDID;
                ResetSaveButton();
            }
            else
            {
                BaseCustomFDID.Text = "";
            }
        }
    }
}