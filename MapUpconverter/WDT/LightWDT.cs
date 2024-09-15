using MapUpconverter.Utils;
using System.Collections.Concurrent;
using System.Numerics;
using Warcraft.NET.Files.Structures;
using Warcraft.NET.Files.WDT.Flags;

namespace MapUpconverter.WDT
{
    public static class LightWDT
    {
        public static Warcraft.NET.Files.WDT.Light.SL.WorldLightTable GenerateForSL(ConcurrentDictionary<string, Warcraft.NET.Files.ADT.TerrainObject.Zero.TerrainObjectZero> cachedOBJ0ADTs)
        {
            var lightWDT = new Warcraft.NET.Files.WDT.Light.SL.WorldLightTable()
            {
                Version = new Warcraft.NET.Files.WDT.Chunks.MVER(20),
                PointLights3 = new Warcraft.NET.Files.WDT.Chunks.SL.MPL3(),
            };

            var li = 0;

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
                        Color = LightInfo.GetRGBA(splitModelName[2].Replace("01", "")),
                        Intensity = 2.5f + (0.1f * (m2Entry.ScalingFactor / 1024f)),
                        AttenuationStart = 0.0f,
                        AttenuationEnd = 15.0f + (1 * (m2Entry.ScalingFactor / 1024f)),
                        Flags = m2Filename.Contains("withshadows") ? MPL3Flags.Raytraced : 0,
                        Unknown1 = 14336,
                        Unused0 = new Vector3(0, 0, 0),
                        TileX = x,
                        TileY = y,
                        MLTAIndex = -1,
                        MTEXIndex = -1
                    };

                    if (m2Filename.Contains("flicker"))
                    {
                        foreach(var nameSplit in splitModelName)
                        {
                            if (nameSplit.StartsWith("flicker"))
                            {
                                var cleanSplit = nameSplit.Replace("01", "");
                                var flickerEntry = LightInfo.GetLightAnim(cleanSplit);
                                if(flickerEntry != null)
                                {
                                    lightWDT.LightAnimations ??= new();
                                    lightWDT.LightAnimations.Entries.Add(flickerEntry);
                                    lightEntry.MLTAIndex = (short)(lightWDT.LightAnimations.Entries.Count - 1);
                                }
                            }
                        }
                    }

                    li++;

                    Console.WriteLine("[DEBUG] Adding light for " + Path.GetFileNameWithoutExtension(filename) + " for model " + m2Entry.NameId + " at pos " + lightEntry.Position);
                    lightWDT.PointLights3.Entries.Add(lightEntry);
                }
            }

            return lightWDT;
        }
    }
}
