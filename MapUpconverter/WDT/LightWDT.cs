using System.Collections.Concurrent;
using System.Numerics;

namespace MapUpconverter.WDT
{
    public static class LightWDT
    {
        private static uint ColorNameToBGRA(string color)
        {
            switch (color)
            {
                case "orange": // 0 65 D6 0
                    return 14050560;

                case "red": // 0 0 255 0
                    return 16711680;

                case "blue": // 255 0 0 0
                    return 255;

                case "green": // 0 0 128 0
                    return 32768;

                case "purple": // 128 0 128 0
                    return 8388736;

                case "yellow": // 0 255 255 0
                    return 16776960;

                case "dimwhite": // 205 250 255 0
                    return 16775885;

                case "felgreen": // 0 0 255 0
                    return 65280;

                case "deepskyblue":
                    return 49151;

                default:
                    Console.WriteLine("Unknown light color: " + color);
                    return 0;
            }
        }

        public static Warcraft.NET.Files.WDT.Light.SL.WorldLightTable GenerateForSL(ConcurrentDictionary<string, Warcraft.NET.Files.ADT.TerrainObject.Zero.TerrainObjectZero> cachedOBJ0ADTs)
        {
            var lightWDT = new Warcraft.NET.Files.WDT.Light.SL.WorldLightTable()
            {
                Version = new Warcraft.NET.Files.WDT.Chunks.MVER(20),
                PointLights3 = new Warcraft.NET.Files.WDT.Chunks.SL.MPL3(),
            };

            var li = 0;

            lightWDT.LightAnimations = new();
            lightWDT.LightAnimations.Entries.Add(new()
            {
                FlickerIntensity = 25.0f,
                FlickerSpeed = 15.0f,
                FlickerMode = 2
            });

            foreach (var filename in Directory.GetFiles(Path.Combine(Settings.InputDir, "world", "maps", Settings.MapName), "*.adt"))
            {
                var splitName = filename.Split('_');

                var x = byte.Parse(splitName[^2]);
                var y = byte.Parse(splitName[^1].Replace(".adt", ""));

                var wotlkADT = new Warcraft.NET.Files.ADT.Terrain.Wotlk.Terrain(File.ReadAllBytes(filename));

                foreach (var m2Entry in wotlkADT.ModelPlacementInfo.MDDFEntries)
                {
                    var m2Filename = Path.GetFileNameWithoutExtension(wotlkADT.Models.Filenames[(int)m2Entry.NameId]).ToLower();

                    if (!m2Filename.StartsWith("noggit_light"))
                        continue;

                    var splitModelName = m2Filename.Split("_");

                    var newPos = new Vector3((m2Entry.Position.Z - 17066.666f) * -1, (m2Entry.Position.X - 17066.666f) * -1, m2Entry.Position.Y);
                    //newPos = newPos + lightInfoEntry.PositionOffset;
                    var lightEntry = new Warcraft.NET.Files.WDT.Entries.SL.MPL3Entry()
                    {
                        Id = (uint)li,
                        Position = newPos,
                        Color = ColorNameToBGRA(splitModelName[2].Replace("01", "")),
                        Intensity = 2.5f + (0.1f * (m2Entry.ScalingFactor / 1024f)),
                        AttenuationStart = 0.0f,
                        AttenuationEnd = 15.0f + (1 * (m2Entry.ScalingFactor / 1024f)),
                        Flags = m2Filename.Contains("withshadows") ? (ushort)1 : (ushort)0,
                        Unknown1 = 14336,
                        Unused0 = new Vector3(0, 0, 0),
                        TileX = x,
                        TileY = y,
                        MLTAIndex = 0,
                        MTEXIndex = -1
                    };

                    li++;

                    Console.WriteLine("[DEBUG] Adding light for " + Path.GetFileNameWithoutExtension(filename) + " for model " + m2Entry.NameId + " at pos " + lightEntry.Position);
                    lightWDT.PointLights3.Entries.Add(lightEntry);
                }
            }

            return lightWDT;
        }
    }
}
