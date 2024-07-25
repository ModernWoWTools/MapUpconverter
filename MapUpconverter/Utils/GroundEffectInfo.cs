using Newtonsoft.Json;

namespace MapUpconverter.Utils
{
    public static class GroundEffectInfo
    {
        public static Dictionary<uint, uint[]> TextureGroundEffectMap = [];

        public static void Initialize(string configPath)
        {
            if (!File.Exists(configPath))
                throw new FileNotFoundException("Ground effect info not found at " + configPath);

            var currentByID = JsonConvert.DeserializeObject<Dictionary<string, uint[]>>(File.ReadAllText(configPath)) ?? throw new Exception("Failed to read GroundEffectIDsByTextureFileID");
            TextureGroundEffectMap = new Dictionary<uint, uint[]>(currentByID.ToDictionary(x => uint.Parse(x.Key), x => x.Value.Order().ToArray()));
        }
    }
}
