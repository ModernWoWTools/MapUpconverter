using System.Numerics;
using Warcraft.NET.Files.Structures;
using Warcraft.NET.Files.WDT.Entries.Legion;

namespace MapUpconverter.Utils
{
    public static class LightInfo
    {
        public static Dictionary<string, LightInfoEntry> lightInfoDict = [];

        static LightInfo()
        {

            // TODO: We'll need to move to a meta file
            lightInfoDict.Add("960215", new LightInfoEntry()
            {
                Id = 1,
                Color = 14050560,
                PositionOffset = new Vector3(0.0f, 0.0f, 2.0f),
                AttenuationStart = 0.0f,
                AttenuationEnd = 15.0f,
                Intensity = 2.5f,
                Unused0 = new Vector3(0.0f, 0.0f, 0.0f),
                TileX = 0,
                TileY = 0,
                MLTAIndex = -1,
                MTEXIndex = -1,
                Flags = 1,
                Unknown1 = 14336
            });
        }
    }

    public struct LightManifest
    {
        public Dictionary<string, RGBA> ColorMap;
        public Dictionary<string, MLTAEntry> LightAnims;
    }
    public struct LightInfoEntry
    {
        public uint Id { get; set; }
        public uint Color { get; set; }
        public Vector3 PositionOffset { get; set; }
        public float AttenuationStart { get; set; }
        public float AttenuationEnd { get; set; }
        public float Intensity { get; set; }
        public Vector3 Unused0 { get; set; }
        public ushort TileX { get; set; }
        public ushort TileY { get; set; }
        public short MLTAIndex { get; set; }
        public short MTEXIndex { get; set; }
        public ushort Flags { get; set; }
        public ushort Unknown1 { get; set; }
    }
}
