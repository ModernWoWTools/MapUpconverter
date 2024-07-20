namespace MapUpconverter.Utils
{
    public static class Listfile
    {
        public static Dictionary<uint, string> NameMap = [];
        public static Dictionary<string, uint> ReverseMap = [];
        public static List<uint> customFDIDs = [];
        private static uint baseCustomFileDataID = 927_000_000;
        private static string ListfileDir = "";

        public static void Initialize(string listfileDir)
        {
            ListfileDir = listfileDir;

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
                    NameMap[fdid] = parts[1];
                    ReverseMap[parts[1]] = fdid;

                    customFDIDs.Add(fdid);

                    if(fdid > 927_000_000 && fdid < 937_000_000 && fdid > baseCustomFileDataID)
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
