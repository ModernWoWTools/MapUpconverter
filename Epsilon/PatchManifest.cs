using MapUpconverter.Utils;
using Newtonsoft.Json;

namespace MapUpconverter.Epsilon
{
    public static class PatchManifest
    {
        public static void ScanUsedFileDataIDs()
        {
            if (string.IsNullOrEmpty(Settings.EpsilonDir))
                return;

            if(!Directory.Exists(Path.Combine(Settings.EpsilonDir, "_retail_", "Patches")))
                return;

            foreach (var manifest in Directory.GetFiles(Path.Combine(Settings.EpsilonDir, "_retail_", "Patches"), "patch.json", SearchOption.AllDirectories))
            {
                var epsilonPatchManifest = JsonConvert.DeserializeObject<EpsilonPatchManifest>(File.ReadAllText(manifest));

                // Assume there is a max of 1 map per patch (the case for out patches, maybe not for other patches)
                var mapNameForPatch = "";

                foreach(var patchFile in epsilonPatchManifest.files)
                {
                    if (Listfile.NameMap.TryGetValue(patchFile.id, out var currentFilename))
                    {
                        // FileDataID exists in listfile, check if the filename matches for informational purposes (we skip it anyways)

                        if (patchFile.file.Contains('/'))
                        {
                            // File is in a subdirectory, assume proper game directory structure was used for overrides/custom files
                            if (currentFilename != patchFile.file.ToLower())
                                Console.WriteLine("Notice: " + currentFilename + " (listfile) is not the same as " + patchFile.file + " (patch.json) for file data ID " + patchFile.id + ".");
                        }
                        else
                        {
                            // File is in root patch directory, assume only basename is provided
                            var currentBasename = Path.GetFileName(currentFilename);
                            if (currentBasename != patchFile.file.ToLower())
                                Console.WriteLine("Notice: " + currentBasename + " (listfile) is not the same as " + patchFile.file + " (patch.json) for file data ID " + patchFile.id + ".");
                        }
                     
                        continue;
                    }
                    else
                    {
                        // FileDataID does not exist in listfile
                        if(patchFile.file.Contains('/'))
                        {
                            // File is in a subdirectory, assume proper game directory structure was used for overrides/custom files
                            Listfile.AddCustomFileDataIDToListfile(patchFile.id, patchFile.file);
                        }
                        else
                        {
                            // File is in root patch directory, assume only basename is provided, attempt to make an educated guess at the filename
                            var normalizedFileName = patchFile.file.ToLower();
                            if (normalizedFileName.EndsWith(".wdt") || normalizedFileName.EndsWith(".tex") || normalizedFileName.EndsWith(".wdl"))
                            {
                                mapNameForPatch = normalizedFileName.Replace(".wdt", "").Replace(".tex", "").Replace(".wdl", "").Replace("_occ", "").Replace("_lgt", "").Replace("_fogs", "").Replace("_mpv", "");
                                Listfile.AddCustomFileDataIDToListfile(patchFile.id, "world/maps/" + mapNameForPatch + "/" + normalizedFileName);
                            }
                            else if (normalizedFileName.EndsWith(".adt"))
                            {
                                mapNameForPatch = normalizedFileName.Replace(".adt", "").Replace("_obj0", "").Replace("_obj1", "").Replace("_tex0", "").Replace("_lod", "");

                                if (mapNameForPatch.Contains('_'))
                                {
                                    var splitName = mapNameForPatch.Split('_');
                                    mapNameForPatch = mapNameForPatch.Replace("_" + splitName[^2] + "_" + splitName[^1], "");
                                }

                                Listfile.AddCustomFileDataIDToListfile(patchFile.id, "world/maps/" + mapNameForPatch + "/" + normalizedFileName);
                            }
                            else
                            {
                                Console.WriteLine("Could not find a good match for " + normalizedFileName + " (file data ID " + patchFile.id + ") in listfile, adding as-is. Please verify manually.");
                                Listfile.AddCustomFileDataIDToListfile(patchFile.id, normalizedFileName);
                            }
                        }
                    }
                }
            }

        }

        public static void Update()
        {
            var epsilonPatchManifestPath = Path.Combine(Settings.EpsilonDir, "_retail_", "Patches", Settings.EpsilonPatchName, "patch.json");

            if (File.Exists(epsilonPatchManifestPath))
            {
                var epsilonPatchManifest = JsonConvert.DeserializeObject<EpsilonPatchManifest>(File.ReadAllText(epsilonPatchManifestPath));

                // Update patch version
                if (string.IsNullOrEmpty(epsilonPatchManifest.version))
                    epsilonPatchManifest.version = "MU-1";
                else if (int.TryParse(epsilonPatchManifest.version.Replace("MU-", ""), out int version))
                    epsilonPatchManifest.version = "MU-" + (version + 1).ToString();

                // Update patch files
                epsilonPatchManifest.files = new List<EpsilonPatchManifestFile>(epsilonPatchManifest.files);

                var patchManifestFilesChanged = false;

                foreach (var outputFile in Directory.GetFiles(Settings.OutputDir, "*.*"))
                {
                    var outputFileName = Path.GetFileName(outputFile);
                    if (outputFileName == "patch.json" || outputFileName == "desktop.ini")
                        continue;

                    if (!epsilonPatchManifest.files.Any(x => x.file == outputFileName))
                    {
                        patchManifestFilesChanged = true;

                        Console.WriteLine("Adding " + outputFileName + " to Epsilon patch manifest.");

                        var fileDataID = Listfile.NameMap.FirstOrDefault(x => x.Value.EndsWith(outputFileName)).Key;
                        if (fileDataID == 0)
                        {
                            Console.WriteLine("Could not find file data ID for " + outputFileName + ", skipping! Epsilon patch might be invalid/disabled now.");
                            continue;
                        }

                        epsilonPatchManifest.files.Add(new EpsilonPatchManifestFile
                        {
                            id = fileDataID,
                            file = outputFileName
                        });
                    }
                }

                foreach (var file in epsilonPatchManifest.files.ToList())
                {
                    if (!File.Exists(Path.Combine(Settings.OutputDir, file.file)))
                    {
                        patchManifestFilesChanged = true;
                        epsilonPatchManifest.files.Remove(file);
                    }
                }

                if (patchManifestFilesChanged)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Epsilon patch manifest file list updated. A re-launch is required to load any new files in-game.");
                    Console.ResetColor();
                    File.WriteAllText(epsilonPatchManifestPath, JsonConvert.SerializeObject(epsilonPatchManifest, Formatting.Indented));
                }
            }
            else
            {
                Console.WriteLine("Epsilon patch not found. Skipping patch update. Either create a patch by the name you set in settings or leave the name in settings empty to skip this step.");
            }
        }

        private struct EpsilonPatchManifest
        {
            public string name { get; set; }
            public string version { get; set; }
            public string url { get; set; }
            public List<EpsilonPatchManifestFile> files { get; set; }
        }

        private struct EpsilonPatchManifestFile
        {
            public uint id { get; set; }
            public string file { get; set; }
        }
    }
}
