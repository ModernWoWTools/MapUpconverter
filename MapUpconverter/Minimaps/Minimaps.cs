using MapUpconverter.Utils;
using Warcraft.NET.Files.BLP;

namespace MapUpconverter.Minimaps
{
    public static class Minimaps
    {
        public static void Convert()
        {
            if (Directory.Exists(Path.Combine(Settings.InputDir, "textures", "minimap")))
            {
                foreach (var file in Directory.GetFiles(Path.Combine(Settings.InputDir, "textures", "minimap"), "*.blp"))
                {
                    if (!Path.GetFileNameWithoutExtension(file).StartsWith(Settings.MapName + "_", StringComparison.CurrentCultureIgnoreCase))
                        continue;

                    var cleanedName = Path.GetFileNameWithoutExtension(file).ToLower().Replace(Settings.MapName.ToLower() + "_", "");
                    var splitName = cleanedName.Split('_');

                    var targetName = Path.Combine(ExportHelper.GetExportDirectory(), "world", "minimaps", Settings.MapName, "map" + splitName[0].PadLeft(2, '0') + "_" + splitName[1].PadLeft(2, '0') + ".blp");
                    var targetDir = Path.GetDirectoryName(targetName);

                    if (!Directory.Exists(targetDir))
                        Directory.CreateDirectory(targetDir!);

                    var currentBLP = new BLP(File.ReadAllBytes(file));

                    if (currentBLP.GetPixelFormat() != BLPPixelFormat.DXT1)
                    {
                        var newBLP = new BLP(currentBLP.GetMipMap(0), BLPPixelFormat.DXT1, false);
                        File.WriteAllBytes(targetName!, newBLP.Serialize());
                    }
                    else
                    {
                        File.Copy(file, targetName!, true);
                    }
                }
            }

            if (Directory.Exists(Path.Combine(Settings.InputDir, "textures", "maptextures")))
            {
                foreach (var file in Directory.GetFiles(Path.Combine(Settings.InputDir, "textures", "maptextures"), "*.blp"))
                {
                    if (!Path.GetFileNameWithoutExtension(file).StartsWith(Settings.MapName + "_", StringComparison.CurrentCultureIgnoreCase))
                        continue;

                    var cleanedName = Path.GetFileNameWithoutExtension(file).ToLower().Replace(Settings.MapName.ToLower() + "_", "");
                    var splitName = cleanedName.Split('_');

                    var targetName = Path.Combine(ExportHelper.GetExportDirectory(), "world", "maptextures", Settings.MapName, Settings.MapName + "_" + splitName[0].PadLeft(2, '0') + "_" + splitName[1].PadLeft(2, '0') + ".blp");

                    if (splitName.Length == 3 && splitName[2] == "n")
                    {
                        targetName = Path.Combine(ExportHelper.GetExportDirectory(), "world", "maptextures", Settings.MapName, Settings.MapName + "_" + splitName[0].PadLeft(2, '0') + "_" + splitName[1].PadLeft(2, '0') + "_n.blp");
                    }

                    var targetDir = Path.GetDirectoryName(targetName);

                    if (!Directory.Exists(targetDir))
                        Directory.CreateDirectory(targetDir!);

                    var currentBLP = new BLP(File.ReadAllBytes(file));

                    if (currentBLP.GetPixelFormat() != BLPPixelFormat.DXT5)
                    {
                        var newBLP = new BLP(currentBLP.GetMipMap(0), BLPPixelFormat.DXT5, true);
                        File.WriteAllBytes(targetName!, newBLP.Serialize());
                    }
                    else
                    {
                        File.Copy(file, targetName!, true);
                    }
                }
            }
        }
    }
}
