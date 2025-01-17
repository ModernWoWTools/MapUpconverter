﻿using Newtonsoft.Json;

namespace MapUpconverter
{
    public static class Settings
    {
        public static string InputDir = "";
        public static string OutputDir = "";

        public static string MapName = "";

        public static string ExportTarget = "";

        public static string EpsilonDir = "";
        public static string EpsilonPatchName = "";

        public static string ArctiumDir = "";
        public static string ArctiumPatchName = "";

        public static bool GenerateWDTWDL = true;
        public static uint RootWDTFileDataID = 0;

        public static bool ConvertOnSave = false;

        public static bool ClientRefresh = false;
        public static bool CASRefresh = false;

        public static int MapID = -1;

        public static int TargetVersion = 927;

        public static float LightBaseIntensity = 1.25f;
        public static float LightBaseAttenuationEnd = 7.5f;

        public static bool UseAdvancedLightConfig = false;

        public static uint BaseCustomFDID = 927_000_000;

        public static void Load(string toolFolder, string settingsName = "settings")
        {
            var jsonPath = Path.Combine(toolFolder, settingsName + ".json");

            if (!File.Exists(jsonPath))
                throw new FileNotFoundException("Settings file not found at " + jsonPath + ", cannot continue.");

            var settingsJSON = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(jsonPath)) ?? throw new Exception("Failed to load settings.json");

            InputDir = settingsJSON.inputDir;
            OutputDir = settingsJSON.outputDir;

            MapName = settingsJSON.mapName;

            EpsilonDir = settingsJSON.epsilonDir;
            EpsilonPatchName = settingsJSON.epsilonPatchName;

            if (settingsJSON.generateWDTWDL == null)
                GenerateWDTWDL = true;
            else
                GenerateWDTWDL = settingsJSON.generateWDTWDL;

            RootWDTFileDataID = settingsJSON.rootWDTFileDataID;

            ConvertOnSave = settingsJSON.convertOnSave;

            if (settingsJSON.clientRefresh == null)
                ClientRefresh = false;
            else
                ClientRefresh = settingsJSON.clientRefresh;

            if (settingsJSON.mapID == null)
                MapID = -1;
            else
                MapID = settingsJSON.mapID;

            if(settingsJSON.arctiumDir == null)
                ArctiumDir = "";
            else
                ArctiumDir = settingsJSON.arctiumDir;

            if (settingsJSON.arctiumPatchName == null)
                ArctiumPatchName = "";
            else
                ArctiumPatchName = settingsJSON.arctiumPatchName;

            if(settingsJSON.exportTarget == null)
                ExportTarget = "";
            else
                ExportTarget = settingsJSON.exportTarget;

            if(settingsJSON.casRefresh == null)
                CASRefresh = false;
            else
                CASRefresh = settingsJSON.casRefresh;

            if (settingsJSON.targetVersion == null)
                TargetVersion = 927;
            else
                TargetVersion = settingsJSON.targetVersion;

            if (settingsJSON.lightBaseIntensity == null)
                LightBaseIntensity = 1.25f;
            else
                LightBaseIntensity = settingsJSON.lightBaseIntensity;

            if (settingsJSON.lightBaseAttenuationEnd == null)
                LightBaseAttenuationEnd = 7.5f;
            else
                LightBaseAttenuationEnd = settingsJSON.lightBaseAttenuationEnd;

            if (settingsJSON.useAdvancedLightConfig == null)
                UseAdvancedLightConfig = false;
            else
                UseAdvancedLightConfig = settingsJSON.useAdvancedLightConfig;

            if (settingsJSON.baseCustomFDID == null)
                BaseCustomFDID = 927_000_000;
            else
                BaseCustomFDID = settingsJSON.baseCustomFDID;
        }

        public static void Save(string toolFolder, string settingsName = "settings")
        {
            var jsonPath = Path.Combine(toolFolder, settingsName + ".json");

            var settingsJSON = new
            {
                inputDir = InputDir,
                outputDir = OutputDir,

                mapName = MapName,

                epsilonDir = EpsilonDir,
                epsilonPatchName = EpsilonPatchName,

                arctiumDir = ArctiumDir,
                arctiumPatchName = ArctiumPatchName,

                generateWDTWDL = GenerateWDTWDL,
                rootWDTFileDataID = RootWDTFileDataID,

                exportTarget = ExportTarget,

                convertOnSave = ConvertOnSave,

                clientRefresh = ClientRefresh,
                casRefresh = CASRefresh,
                mapID = MapID,

                targetVersion = TargetVersion,

                lightBaseIntensity = LightBaseIntensity,
                lightBaseAttenuationEnd = LightBaseAttenuationEnd,

                useAdvancedLightConfig = UseAdvancedLightConfig
            };

            File.WriteAllText(jsonPath, JsonConvert.SerializeObject(settingsJSON, Formatting.Indented));
        }
    }
}
