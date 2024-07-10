using MapUpconverter.Utils;
using Newtonsoft.Json;

namespace MapUpconverter.Epsilon
{
    public static class PatchManifest
    {

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
