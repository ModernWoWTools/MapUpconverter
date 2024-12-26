namespace MapUpconverter.Utils
{
    public static class Listfile
    {
        public static Dictionary<uint, string> NameMap = [];
        public static Dictionary<string, uint> ReverseMap = [];
        public static List<uint> customFDIDs = [];
        private static uint baseCustomFileDataID;
        private static string ListfileDir = "";

        public static void Initialize(string listfileDir)
        {
            ListfileDir = listfileDir;
            baseCustomFileDataID = Settings.BaseCustomFDID;

            var listfilePath = Path.Combine(listfileDir, "meta", "listfile.csv");
            if (!File.Exists(listfilePath))
                throw new FileNotFoundException("Listfile not found at " + listfilePath);

            var listfileLines = File.ReadAllLines(listfilePath);

            NameMap = new Dictionary<uint, string>(listfileLines.Length);
            ReverseMap = new Dictionary<string, uint>(listfileLines.Length);

            foreach (var line in listfileLines)
            {
                var parts = line.Split(';');
                if (parts.Length < 2)
                    continue;

                var fdid = uint.Parse(parts[0]);
                NameMap[fdid] = parts[1];
                ReverseMap[parts[1]] = fdid;
            }

            var customListfilePath = Path.Combine(listfileDir, "meta", "custom-listfile.csv");
            if (File.Exists(customListfilePath))
            {
                foreach (var line in File.ReadAllLines(customListfilePath))
                {
                    var parts = line.Split(';');
                    if (parts.Length < 2)
                        continue;

                    var fdid = uint.Parse(parts[0]);
                    var filename = parts[1].ToLowerInvariant();

                    NameMap[fdid] = filename;
                    ReverseMap[filename] = fdid;

                    customFDIDs.Add(fdid);

                    if(fdid > Settings.BaseCustomFDID && fdid < (Settings.BaseCustomFDID + 1_000_000) && fdid > baseCustomFileDataID)
                        baseCustomFileDataID = fdid;
                }
            }
        }

        public static uint GetNextFreeFileDataID()
        {
            return ++baseCustomFileDataID;
        }

        public static void AddCustomFileDataIDToListfile(uint fileDataID, string filename)
        {
            if (filename == "world/maps/" + Settings.MapName + "/" + Settings.MapName + ".wdt" && Settings.RootWDTFileDataID != 0)
            {
                // Special case -- for overriding maps users can use existing Blizzard WDT FDIDs referenced from Map.db2, this means we need to add an official FDID to the patch.
                fileDataID = Settings.RootWDTFileDataID;
            }

            filename = filename.ToLower();

            if (NameMap.ContainsKey(fileDataID))
                Console.WriteLine("File data ID " + fileDataID + " is already assigned to " + NameMap[fileDataID] + " , skipping addition of " + filename + ".");

            NameMap[fileDataID] = filename;

            if (!ReverseMap.TryGetValue(filename, out var currentFilename))
            {
                ReverseMap[filename] = fileDataID;
            }
            else
            {
                Console.WriteLine("Warning: File data ID " + fileDataID + " (" + filename + ") is already assigned to ID " + currentFilename + " ,skipping.");
            }
            
            customFDIDs.Add(fileDataID);

            // Update custom-listfile.csv
            File.WriteAllLines(Path.Combine(ListfileDir, "meta", "custom-listfile.csv"), customFDIDs.Select(x => x + ";" + NameMap[x]));
        }
    }
}
