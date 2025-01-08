using MapUpconverter.Utils;
using System.Collections.Concurrent;
using Warcraft.NET;
using Warcraft.NET.Files.ADT.Chunks.Legion;
using Warcraft.NET.Files.ADT.Entries.Legion;
using Warcraft.NET.Files.WDL.Chunks;

namespace MapUpconverter.WDL
{
    public static class WDL
    {
        public static Warcraft.NET.Files.WDL.Legion.WorldDataLod Generate(ConcurrentDictionary<string, Warcraft.NET.Files.ADT.Terrain.BfA.Terrain> cachedRootADTs, ConcurrentDictionary<string, Warcraft.NET.Files.ADT.TerrainObject.One.TerrainObjectOne> cachedOBJ1ADTs)
        {
            var adtDict = new Dictionary<(byte, byte), string>();

            foreach (var file in Directory.GetFiles(ExportHelper.GetExportDirectory(), "*.adt", SearchOption.AllDirectories))
            {
                if (file.EndsWith("_lod.adt") || file.EndsWith("obj0.adt") || file.EndsWith("obj1.adt") || file.EndsWith("tex0.adt"))
                    continue;

                var filename = Path.GetFileNameWithoutExtension(file).ToLowerInvariant();

                if (!filename.StartsWith(Settings.MapName.ToLowerInvariant()))
                {
                    Console.WriteLine(filename + " does not belong to " + Settings.MapName + ", skipping it in WDL generation...");
                    continue;
                }

                var splitName = filename.Split('_');

                var x = byte.Parse(splitName[^2]);
                var y = byte.Parse(splitName[^1].Replace(".adt", ""));
                adtDict[(x, y)] = filename.ToLowerInvariant();
            }

            var wdl = new Warcraft.NET.Files.WDL.Legion.WorldDataLod
            {
                Version = new MVER(18),
                LevelDoodadDetail = new(),
                LevelDoodadExtent = new(),
                LevelWorldObjectDetail = new(),
                LevelWorldObjectExtent = new(),
                MapAreaOffsets = new()
            };

            wdl.MapAreaOffsets = MAOF.CreateEmpty();

            const float stepSize = Metrics.TileSize / 16.0f;

            var mlmxEntries = new Dictionary<uint, MLMXEntry>();

            for (byte ai = 0; ai < 64; ++ai)
            {
                for (byte aj = 0; aj < 64; ++aj)
                {
                    if (adtDict.TryGetValue((ai, aj), out var adtName))
                    {
                        if (!cachedRootADTs.TryGetValue(adtName, out var rootADT))
                        {
                            rootADT = new Warcraft.NET.Files.ADT.Terrain.BfA.Terrain(File.ReadAllBytes(Path.Combine(ExportHelper.GetExportDirectory(), "world", "maps", Settings.MapName, adtName + ".adt")));
                            cachedRootADTs.TryAdd(adtName, rootADT);
                        }

                        if (!cachedOBJ1ADTs.TryGetValue(adtName + "_obj1", out var OBJ1ADT))
                        {
                            OBJ1ADT = new Warcraft.NET.Files.ADT.TerrainObject.One.TerrainObjectOne(File.ReadAllBytes(Path.Combine(ExportHelper.GetExportDirectory(), "world", "maps", Settings.MapName, adtName + "_obj1.adt")));
                            cachedOBJ1ADTs.TryAdd(adtName + "_obj1", OBJ1ADT);
                        }

                        var mare = new MARE();

                        var heights = new float[256][];
                        for (var i = 0; i < 16; ++i)
                        {
                            for (var j = 0; j < 16; ++j)
                            {
                                heights[i * 16 + j] = (float[])rootADT.Chunks[i * 16 + j].Heightmap.Vertices.Clone();

                                for (var k = 0; k < heights[i * 16 + j].Length; ++k)
                                    heights[i * 16 + j][k] += rootADT.Chunks[i * 16 + j].Header.MapTilePosition.Y;
                            }
                        }

                        // Outer
                        // By Luzifix: https://github.com/Luzifix/ADTConvert
                        for (var i = 0; i < 17; ++i)
                        {
                            for (var j = 0; j < 17; ++j)
                            {
                                var posx = j * stepSize;
                                var posy = i * stepSize;

                                var landHeight = GetLandHeight(heights, posx, posy);
                                mare.HighResVertices[i * 17 + j] = (short)
                                    Math.Min(
                                        Math.Max(
                                            Math.Round(landHeight),
                                            short.MinValue),
                                        short.MaxValue);
                            }
                        }

                        // Inner
                        // By Luzifix: https://github.com/Luzifix/ADTConvert
                        for (var i = 0; i < 16; ++i)
                        {
                            for (var j = 0; j < 16; ++j)
                            {
                                var posx = j * stepSize;
                                var posy = i * stepSize;
                                posx += stepSize / 2.0f;
                                posy += stepSize / 2.0f;

                                var landHeight = GetLandHeight(heights, posx, posy);
                                mare.LowResVertices[i * 16 + j] = (short)
                                    Math.Min(
                                        Math.Max(
                                            Math.Round(landHeight),
                                            short.MinValue),
                                        short.MaxValue);
                            }
                        }

                        wdl.MapAreas[aj * 64 + ai] = mare;

                        var bigWMOIndexes = OBJ1ADT.LevelWorldObjectExtent.Entries.Select((i, s) => new { Entry = i, Index = s }).Where(x => x.Entry.Radius > 1000).Select(x => x.Index).ToList();

                        foreach (var bigWMOIndex in bigWMOIndexes)
                        {
                            var bigWMO = OBJ1ADT.LevelWorldObjectDetail.MLMDEntries[bigWMOIndex];

                            if (mlmxEntries.ContainsKey(bigWMO.UniqueID))
                                continue;

                            var newEntry = OBJ1ADT.LevelWorldObjectExtent.Entries[bigWMOIndex];
                            newEntry.Radius = 17066 * 2;
                            mlmxEntries.Add(bigWMO.UniqueID, newEntry);
                            wdl.LevelWorldObjectDetail.MLMDEntries.Add(bigWMO);
                        }

                        wdl.MapAreaHoles[aj * 64 + ai] = MAHO.CreateEmpty();

                        if (rootADT.Water != null)
                        {
                            // Note: While this 'works' for all water, Blizzard only does this for ocean.
                            // Water placed at non-ocean levels will probably look funky...
                            // ...but since this is distance stuff, this is good enough for now.
                            var maoe = new MAOE
                            {
                                Data = new byte[32]
                            };

                            var bitArray = new System.Collections.BitArray(maoe.Data);

                            using (var ms = new MemoryStream(rootADT.Water.data))
                            using (var br = new BinaryReader(ms))
                            {
                                for (var x2 = 0; x2 < 16; x2++)
                                {
                                    for (var y2 = 0; y2 < 16; y2++)
                                    {
                                        br.BaseStream.Position += 4;
                                        var layerCount = br.ReadUInt32();
                                        br.BaseStream.Position += 4;

                                        bitArray[x2 * 16 + y2] = layerCount >= 1;
                                    }
                                }
                            }

                            bitArray.CopyTo(maoe.Data, 0);

                            wdl.MapAreaOcean[aj * 64 + ai] = maoe;
                        }
                    }
                }
            }

            if (mlmxEntries.Count > 0)
            {
                mlmxEntries = mlmxEntries.OrderByDescending(e => e.Value.Radius).ToDictionary(e => e.Key, e => e.Value);

                wdl.LevelWorldObjectDetail.MLMDEntries = wdl.LevelWorldObjectDetail.MLMDEntries.OrderBy(entry => mlmxEntries.Keys.ToList().IndexOf(entry.UniqueID)).ToList();
                wdl.LevelWorldObjectExtent = new MLMX() { Entries = mlmxEntries.Values.ToList() };
            }

            return wdl;
        }

        // By Luzifix: https://github.com/Luzifix/ADTConvert
        private static float GetLandHeight(float[][] heights, float x, float y)
        {
            var cx = (int)Math.Floor(x / Metrics.ChunkSize);
            var cy = (int)Math.Floor(y / Metrics.ChunkSize);
            cx = Math.Min(Math.Max(cx, 0), 15);
            cy = Math.Min(Math.Max(cy, 0), 15);

            if (heights[cy * 16 + cx] == null)
                return 0;

            x -= cx * Metrics.ChunkSize;
            y -= cy * Metrics.ChunkSize;

            var row = (int)(y / (Metrics.UnitSize * 0.5f) + 0.5f);
            var col = (int)((x - Metrics.UnitSize * 0.5f * (row % 2)) / Metrics.UnitSize + 0.5f);

            if (row < 0 || col < 0 || row > 16 || col > (((row % 2) != 0) ? 8 : 9))
                return 0;

            return heights[cy * 16 + cx][17 * (row / 2) + (((row % 2) != 0) ? 9 : 0) + col];
        }
    }
}
