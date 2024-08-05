using MapUpconverter.Utils;

namespace MapUpconverter.Arctium
{
    public class FileMapping
    {
        public static void Update()
        {
            if (string.IsNullOrEmpty(Settings.ArctiumDir))
                throw new Exception("Arctium folder not set.");

            if (!Directory.Exists(Path.Combine(Settings.ArctiumDir, "mappings")))
                Directory.CreateDirectory(Path.Combine(Settings.ArctiumDir, "mappings"));


            var mapping = new Dictionary<uint, string>();
            foreach(var outputFile in Directory.GetFiles(ExportHelper.GetExportDirectory(), "*", SearchOption.AllDirectories))
            {
                var gamePath = outputFile.Replace(ExportHelper.GetExportDirectory() + "\\", "").Replace(ExportHelper.GetExportDirectory() + "/", "").Replace("\\", "/");
                var outputFileName = Path.GetFileName(outputFile);
                if (outputFileName == "desktop.ini")
                    continue;

                if (!Listfile.ReverseMap.TryGetValue(gamePath.ToLower(), out var fileDataID))
                {
                    if (gamePath.EndsWith(Settings.MapName + ".wdt") && Settings.RootWDTFileDataID != 0)
                    {
                        // Special case -- for overriding maps users can use existing Blizzard WDT FDIDs referenced from Map.db2, this means we need to add an official FDID to the patch.
                        fileDataID = Settings.RootWDTFileDataID;
                    }
                    else
                    {
                        var newFileDataID = Listfile.GetNextFreeFileDataID();
                        Console.WriteLine("Assigning new file data ID " + newFileDataID + " for " + gamePath + ".");
                        Listfile.AddCustomFileDataIDToListfile(newFileDataID, gamePath);
                        fileDataID = newFileDataID;
                    }
                }

                mapping[fileDataID] = Settings.ArctiumPatchName + "/" + gamePath;
            }

            File.WriteAllLines(Path.Combine(Settings.ArctiumDir, "mappings", Settings.ArctiumPatchName + ".txt"), mapping.Select(x => x.Key + ";" + x.Value).ToArray());
        }
    }
}
