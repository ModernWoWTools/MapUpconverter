using Warcraft.NET.Files.BLP;

namespace MapUpconverter.Minimaps
{
    public static class Minimaps
    {
        public static void Convert()
        {
            if (!Directory.Exists(Path.Combine(Settings.InputDir, "textures", "minimap")))
                return;

            foreach (var file in Directory.GetFiles(Path.Combine(Settings.InputDir, "textures", "minimap"), "*.blp"))
            {
                var cleanedName = Path.GetFileNameWithoutExtension(file).ToLower().Replace(Settings.MapName + "_", "");
                var splitName = cleanedName.Split('_');

                var targetName = Path.Combine(Settings.OutputDir, "world", "minimaps", Settings.MapName, "map" + splitName[0].PadLeft(2, '0') + "_" + splitName[1].PadLeft(2, '0') + ".blp");
                var targetDir = Path.GetDirectoryName(targetName);

                if (File.Exists(targetName))
                    continue;

                if(!Directory.Exists(targetDir))
                    Directory.CreateDirectory(targetDir!);

                var currentBLP = new BLP(File.ReadAllBytes(file));

                if(currentBLP.GetPixelFormat() != BLPPixelFormat.DXT1)
                {
                    var newBLP = new BLP(currentBLP.GetMipMap(0), BLPPixelFormat.DXT1, false);
                    File.WriteAllBytes(targetName!, newBLP.Serialize());
                }
                else
                {
                    File.Copy(file, targetName!);
                }
            }
        }
    }
}
