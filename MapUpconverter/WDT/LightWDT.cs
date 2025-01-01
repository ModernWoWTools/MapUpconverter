using MapUpconverter.Utils;
using System.Collections.Concurrent;
using System.Numerics;
using Warcraft.NET.Files.WDT.Flags;

namespace MapUpconverter.WDT
{
    public static class LightWDT
    {
        private static object LightLock = new();

        public static Warcraft.NET.Files.WDT.Light.Legion.WorldLightTable GenerateForLegion(ConcurrentDictionary<string, Warcraft.NET.Files.ADT.TerrainObject.Zero.TerrainObjectZero> cachedOBJ0ADTs)
        {
            var lightWDT = new Warcraft.NET.Files.WDT.Light.Legion.WorldLightTable()
            {
                Version = new Warcraft.NET.Files.WDT.Chunks.MVER(20),
                PointLights2 = new Warcraft.NET.Files.WDT.Chunks.Legion.MPL2(),
            };

            var li = 0;

            Parallel.ForEach(Directory.GetFiles(Path.Combine(Settings.InputDir, "world", "maps", Settings.MapName), "*.adt"), filename =>
            {
                var adtName = Path.GetFileNameWithoutExtension(filename).ToLowerInvariant();

                if(!Program.lightEntries.TryGetValue(adtName, out var lightEntries))
                {
                    Console.WriteLine("Cache miss for " + adtName + ", parsing for lights..");
                    var wotlkADT = new Warcraft.NET.Files.ADT.Terrain.Wotlk.Terrain(File.ReadAllBytes(filename));

                    lock (LightLock)
                    {
                        Program.lightEntries.TryAdd(adtName, new List<(string, Warcraft.NET.Files.ADT.Entries.MDDFEntry)>());
                    }

                    if (wotlkADT == null || !wotlkADT.Models.Filenames.Any(x => x.Contains("noggit_light", StringComparison.CurrentCultureIgnoreCase)))
                        return;

                    foreach (var m2Entry in wotlkADT.ModelPlacementInfo.MDDFEntries)
                    {
                        var m2Filename = Path.GetFileNameWithoutExtension(wotlkADT.Models.Filenames[(int)m2Entry.NameId]).ToLower();

                        if (!m2Filename.StartsWith("noggit_light"))
                            continue;

                        lock (LightLock)
                        {
                            Program.lightEntries[adtName].Add((m2Filename, m2Entry));
                        }
                    }
                }

                var splitName = filename.Split('_');

                var x = byte.Parse(splitName[^2]);
                var y = byte.Parse(splitName[^1].Replace(".adt", ""));

                foreach (var adtLightEntry in Program.lightEntries[adtName])
                {
                    var m2Filename = adtLightEntry.Item1;
                    var m2Entry = adtLightEntry.Item2;

                    var splitModelName = m2Filename.Split("_");

                    var newPos = new Vector3((m2Entry.Position.Z - 17066.666f) * -1, (m2Entry.Position.X - 17066.666f) * -1, m2Entry.Position.Y);
                    var lightEntry = new Warcraft.NET.Files.WDT.Entries.Legion.MPL2Entry()
                    {
                        Id = (uint)li,
                        Position = newPos,
                        Color = LightInfo.GetRGBA(splitModelName[2].Replace("01", "")),
                        Intensity = Settings.LightBaseIntensity + (0.1f * (m2Entry.ScalingFactor / 1024f)),
                        AttenuationStart = 0.0f,
                        AttenuationEnd = Settings.LightBaseAttenuationEnd + (1 * (m2Entry.ScalingFactor / 1024f)),
                        Rotation = new Vector3(0, 0, 0),
                        TileX = x,
                        TileY = y,
                        MLTAIndex = -1,
                        MTEXIndex = -1
                    };

                    if (m2Filename.Contains("flicker"))
                    {
                        foreach (var nameSplit in splitModelName)
                        {
                            if (nameSplit.StartsWith("flicker"))
                            {
                                var cleanSplit = nameSplit.Replace("01", "");
                                var flickerEntry = LightInfo.GetLightAnim(cleanSplit);
                                if (flickerEntry != null)
                                {
                                    lightWDT.LightAnimations ??= new();
                                    lightWDT.LightAnimations.Entries.Add(flickerEntry);
                                    lightEntry.MLTAIndex = (short)(lightWDT.LightAnimations.Entries.Count - 1);
                                }
                            }
                        }
                    }

                    lock (LightLock)
                    {
                        li++;
                        lightWDT.PointLights2.Entries.Add(lightEntry);
                    }
                }
            });

            return lightWDT;
        }

        public static Warcraft.NET.Files.WDT.Light.SL.WorldLightTable GenerateForSL(ConcurrentDictionary<string, Warcraft.NET.Files.ADT.TerrainObject.Zero.TerrainObjectZero> cachedOBJ0ADTs)
        {
            var lightWDT = new Warcraft.NET.Files.WDT.Light.SL.WorldLightTable()
            {
                Version = new Warcraft.NET.Files.WDT.Chunks.MVER(20),
                PointLights3 = new Warcraft.NET.Files.WDT.Chunks.SL.MPL3(),
            };

            var li = 0;

            Parallel.ForEach(Directory.GetFiles(Path.Combine(Settings.InputDir, "world", "maps", Settings.MapName), "*.adt"), filename =>
            {
                var adtName = Path.GetFileNameWithoutExtension(filename).ToLowerInvariant();

                if (!Program.lightEntries.TryGetValue(adtName, out var lightEntries))
                {
                    Console.WriteLine("Cache miss for " + adtName + ", parsing for lights..");
                    var wotlkADT = new Warcraft.NET.Files.ADT.Terrain.Wotlk.Terrain(File.ReadAllBytes(filename));

                    lock (LightLock)
                    {
                        Program.lightEntries.TryAdd(adtName, []);
                    }

                    if (wotlkADT == null || !wotlkADT.Models.Filenames.Any(x => x.Contains("noggit_light", StringComparison.CurrentCultureIgnoreCase)))
                        return;

                    foreach (var m2Entry in wotlkADT.ModelPlacementInfo.MDDFEntries)
                    {
                        var m2Filename = Path.GetFileNameWithoutExtension(wotlkADT.Models.Filenames[(int)m2Entry.NameId]).ToLower();

                        if (!m2Filename.StartsWith("noggit_light"))
                            continue;

                        lock (LightLock)
                        {
                            Program.lightEntries[adtName].Add((m2Filename, m2Entry));
                        }
                    }
                }

                var splitName = filename.Split('_');

                var x = byte.Parse(splitName[^2]);
                var y = byte.Parse(splitName[^1].Replace(".adt", ""));

                foreach (var adtLightEntry in Program.lightEntries[adtName])
                {
                    var m2Filename = adtLightEntry.Item1;
                    var m2Entry = adtLightEntry.Item2;

                    var splitModelName = m2Filename.Split("_");

                    var newPos = new Vector3((m2Entry.Position.Z - 17066.666f) * -1, (m2Entry.Position.X - 17066.666f) * -1, m2Entry.Position.Y);
                    var lightEntry = new Warcraft.NET.Files.WDT.Entries.SL.MPL3Entry()
                    {
                        Id = (uint)li,
                        Position = newPos,
                        Color = LightInfo.GetRGBA(splitModelName[2].Replace("01", "")),
                        Intensity = Settings.LightBaseIntensity + (0.1f * (m2Entry.ScalingFactor / 1024f)),
                        AttenuationStart = 0.0f,
                        AttenuationEnd = Settings.LightBaseAttenuationEnd + (1 * (m2Entry.ScalingFactor / 1024f)),
                        Flags = m2Filename.Contains("withshadows") ? MPL3Flags.Raytraced : 0,
                        Scale = 0.5f,
                        Rotation = new Vector3(0, 0, 0),
                        TileX = x,
                        TileY = y,
                        MLTAIndex = -1,
                        MTEXIndex = -1
                    };

                    if (m2Filename.Contains("flicker"))
                    {
                        foreach (var nameSplit in splitModelName)
                        {
                            if (nameSplit.StartsWith("flicker"))
                            {
                                var cleanSplit = nameSplit.Replace("01", "");
                                var flickerEntry = LightInfo.GetLightAnim(cleanSplit);
                                if (flickerEntry != null)
                                {
                                    lightWDT.LightAnimations ??= new();
                                    lightWDT.LightAnimations.Entries.Add(flickerEntry);
                                    lightEntry.MLTAIndex = (short)(lightWDT.LightAnimations.Entries.Count - 1);
                                }
                            }
                        }
                    }

                    lock (LightLock)
                    {
                        li++;
                        lightWDT.PointLights3.Entries.Add(lightEntry);
                    }
                }
            });

            return lightWDT;
        }
    }
}
