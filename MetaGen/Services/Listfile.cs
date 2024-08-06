namespace MetaGen.Services
{
    public static class Listfile
    {
        public static Dictionary<uint, string> NameMap = [];
        public static Dictionary<string, uint> ReverseMap = [];
        public static void Initialize(string listfileDir)
        {
            var listfileLines = File.ReadAllLines(Path.Combine(listfileDir, "listfile.csv"));
            foreach (var line in listfileLines)
            {
                var parts = line.Split(';');
                if (parts.Length < 2)
                    continue;

                var fdid = uint.Parse(parts[0]);
                var name = parts[1].ToLowerInvariant();
                NameMap[fdid] = name;
                ReverseMap[name] = fdid;
            }

            if(File.Exists(Path.Combine(listfileDir, "custom-listfile.csv")))
            {
                var customListfileLines = File.ReadAllLines(Path.Combine(listfileDir, "custom-listfile.csv"));
                foreach (var customLine in customListfileLines)
                {
                    var parts = customLine.Split(';');
                    if (parts.Length < 2)
                        continue;

                    var fdid = uint.Parse(parts[0]);
                    var name = parts[1].ToLowerInvariant();
                    NameMap[fdid] = name;
                    ReverseMap[name] = fdid;
                }
            }
        }
    }
}
