using Newtonsoft.Json;
using Warcraft.NET.Files.Structures;
using Warcraft.NET.Files.WDT.Entries.Legion;

namespace MapUpconverter.Utils
{
    public static class LightInfo
    {
        private static Dictionary<string, RGBA> ColorMap = [];
        private static Dictionary<string, MLTAEntry> LightAnims = [];

        public static void Initialize(string configPath)
        {
            var lightInfoPath = Path.Combine(configPath, "LightInfo.json");

            if (!File.Exists(lightInfoPath))
                throw new FileNotFoundException("Light info not found at " + lightInfoPath);

            var lightInfo = JsonConvert.DeserializeObject<LightManifest>(File.ReadAllText(lightInfoPath));

            ColorMap = lightInfo.ColorMap;
            LightAnims = lightInfo.LightAnims;
        }

        public static RGBA GetRGBA(string color)
        {
            if (ColorMap.TryGetValue(color, out RGBA rgba))
                return rgba;

            Console.WriteLine("Unknown color: " + color + ", returning 0.");
            return new RGBA(0, 0, 0, 0);
        }

        public static MLTAEntry? GetLightAnim(string name)
        {
            if (LightAnims.TryGetValue(name, out MLTAEntry? lightAnim))
                return lightAnim;

            return null;
        }
    }

    public struct LightManifest
    {
        public Dictionary<string, RGBA> ColorMap;
        public Dictionary<string, MLTAEntry> LightAnims;
    }
}
