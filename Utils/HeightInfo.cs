using Newtonsoft.Json;

namespace MapUpconverter.Utils
{
    public class TextureInfo
    {
        public TextureInfo(byte scale, float heightScale, float heightOffset, uint groundEffect)
        {
            Scale = scale;
            HeightScale = heightScale;
            HeightOffset = heightOffset;
            GroundEffect = groundEffect;
        }

        private byte _scale;
        public byte Scale
        {
            get => _scale;
            set
            {
                if (value > 15)
                    _scale = 15;
                else
                    _scale = value;
            }
        }
        public float HeightScale { get; set; }
        public float HeightOffset { get; set; }
        public uint GroundEffect { get; set; }

        public uint GetFlags()
        {
            return (uint)(_scale << 4);
        }
    }

    public static class HeightInfo
    {
        public static Dictionary<string, TextureInfo> textureInfoMap = [];

        public static void Initialize(string configPath)
        {
            if (!File.Exists(configPath))
                throw new FileNotFoundException("Height texturing config not found at " + configPath);

            textureInfoMap = JsonConvert.DeserializeObject<Dictionary<string, TextureInfo>>(File.ReadAllText(configPath)) ?? throw new Exception("Failed to read height texturing info config");
        }
    }
}
