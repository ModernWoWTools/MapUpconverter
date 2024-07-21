namespace MetaGen.Services
{
    public static class Listfile
    {
        public static Dictionary<uint, string> NameMap = [];

        public static void Initialize(string listfileDir)
        {
            var listfileLines = File.ReadAllLines(Path.Combine(listfileDir, "listfile.csv"));
            foreach (var line in listfileLines)
            {
                var parts = line.Split(';');
                if (parts.Length < 2)
                    continue;

                NameMap[uint.Parse(parts[0])] = parts[1].ToLowerInvariant();
            }
        }
    }
}
