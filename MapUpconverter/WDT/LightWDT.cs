using System.Collections.Concurrent;
using System.Numerics;
using Warcraft.NET.Files.Structures;
using Warcraft.NET.Files.WDT.Flags;

namespace MapUpconverter.WDT
{
    public static class LightWDT
    {
        private static RGBA ColorNameToRGBA(string color)
        {
            switch (color)
            {
                case "orange":
                    return new RGBA(255, 148, 112, 0);

                case "red":
                    return new RGBA(255, 0, 0, 0);

                case "blue":
                    return new RGBA(0, 0, 255, 0);

                case "green":
                    return new RGBA(0, 128, 0, 0);

                case "purple":
                    return new RGBA(128, 0, 128, 0);

                case "yellow":
                    return new RGBA(255, 255, 0, 0);

                case "dimwhite":
                    return new RGBA(255, 250, 205, 0);

                case "felgreen":
                    return new RGBA(0, 255, 0, 0);

                case "deepskyblue":
                    return new RGBA(0, 191, 255, 0);

                default:
                    Console.WriteLine("Unknown light color: " + color);
                    return new RGBA(0, 0, 0, 0);
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
                        Color = ColorNameToRGBA(splitModelName[2].Replace("01", "")),
                        Intensity = 2.5f + (0.1f * (m2Entry.ScalingFactor / 1024f)),
                        AttenuationStart = 0.0f,
                        AttenuationEnd = 15.0f + (1 * (m2Entry.ScalingFactor / 1024f)),
                        Flags = m2Filename.Contains("withshadows") ? MPL3Flags.Raytraced : 0,
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
