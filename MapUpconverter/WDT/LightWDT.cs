using MapUpconverter.Utils;
using System.Collections.Concurrent;
using System.Numerics;

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

            var adtDict = new Dictionary<(byte, byte), string>();

            foreach (var file in Directory.GetFiles(ExportHelper.GetExportDirectory(), "*.adt", SearchOption.AllDirectories))
            {
                if (!file.EndsWith("obj0.adt"))
                    continue;

                var filename = Path.GetFileNameWithoutExtension(file).ToLowerInvariant();

                if (!filename.StartsWith(Settings.MapName.ToLowerInvariant()))
                {
                    Console.WriteLine(filename + " does not belong to " + Settings.MapName + ", skipping it in WDT generation...");
                    continue;
                }

                var splitName = filename.Split('_');

                var x = byte.Parse(splitName[^3]);
                var y = byte.Parse(splitName[^2].Replace("obj0.adt", ""));
                adtDict[(x, y)] = filename.ToLowerInvariant();
            }

            var li = 1;

            for (byte ai = 0; ai < 64; ++ai)
            {
                for (byte aj = 0; aj < 64; ++aj)
                {
                    if (adtDict.TryGetValue((ai, aj), out var adtName))
                    {
                        if (!cachedOBJ0ADTs.TryGetValue(adtName, out var OBJ0ADT))
                        {
                            OBJ0ADT = new Warcraft.NET.Files.ADT.TerrainObject.Zero.TerrainObjectZero(File.ReadAllBytes(Path.Combine(ExportHelper.GetExportDirectory(), "world", "maps", Settings.MapName, adtName + ".adt")));
                            cachedOBJ0ADTs.TryAdd(adtName, OBJ0ADT);
                        }

                        foreach(var m2Entry in OBJ0ADT.ModelPlacementInfo.MDDFEntries)
                        {
                            if (LightInfo.lightInfoDict.TryGetValue(m2Entry.NameId.ToString(), out var lightInfoEntry))
                            {
                                var newPos = new Vector3((m2Entry.Position.Z - 17066.666f) * -1, (m2Entry.Position.X - 17066.666f) * -1, m2Entry.Position.Y);
                                newPos = newPos + lightInfoEntry.PositionOffset;

                                var lightEntry = new Warcraft.NET.Files.WDT.Entries.SL.MPL3Entry()
                                {
                                    Id = (uint)li,
                                    Position = newPos,
                                    Color = lightInfoEntry.Color,
                                    Intensity = lightInfoEntry.Intensity,
                                    AttenuationStart = lightInfoEntry.AttenuationStart,
                                    AttenuationEnd = lightInfoEntry.AttenuationEnd,
                                    Flags = lightInfoEntry.Flags,
                                    Unknown1 = lightInfoEntry.Unknown1,
                                    Unused0 = lightInfoEntry.Unused0,
                                    TileX = ai,
                                    TileY = aj,
                                    MLTAIndex = lightInfoEntry.MLTAIndex,
                                    MTEXIndex = lightInfoEntry.MTEXIndex
                                };

                                li++;

                                Console.WriteLine("[DEBUG] Adding light for " + adtName + " for model " + m2Entry.NameId + " at pos " + lightEntry.Position);
                                lightWDT.PointLights3.Entries.Add(lightEntry);
                            }
                        }
                    }
                }
            }

            return lightWDT;
        }
    }
}
