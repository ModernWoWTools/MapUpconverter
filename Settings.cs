using Newtonsoft.Json;

namespace MapUpconverter
{
    public static class Settings
    {
        public static string InputDir = "";
        public static string OutputDir = "";

        public static string MapName = "";
        public static string ReplaceMapNameWith = "";

        public static string EpsilonDir = "";
        public static string EpsilonPatchName = "";

        public static uint RootWDTFileDataID = 0;

        public static bool ConvertOnSave = false;

        public static void Load(string toolFolder)
        {
            var jsonPath = Path.Combine(toolFolder, "settings.json");

            if (!File.Exists(jsonPath))
                throw new FileNotFoundException("Settings file not found at " + jsonPath + ", cannot continue.");

            var settingsJSON = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(jsonPath)) ?? throw new Exception("Failed to load settings.json");

            InputDir = settingsJSON.inputDir;
            OutputDir = settingsJSON.outputDir;

            MapName = settingsJSON.mapName;
            ReplaceMapNameWith = settingsJSON.replaceMapNameWith;

            EpsilonDir = settingsJSON.epsilonDir;
            EpsilonPatchName = settingsJSON.epsilonPatchName;

            RootWDTFileDataID = settingsJSON.rootWDTFileDataID;

            ConvertOnSave = settingsJSON.convertOnSave;
        }
    }
}
