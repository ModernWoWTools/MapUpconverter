﻿
using MapUpconverter.Utils;
using Warcraft.NET.Files.WDT.Flags;

namespace MapUpconverter.WDT
{
    public static class RootWDT
    {
        public static Warcraft.NET.Files.WDT.Root.WotLK.WorldDataTable GenerateLegion()
        {
            var wdtResults = Directory.GetFiles(Settings.InputDir, Settings.MapName + ".wdt", SearchOption.AllDirectories);
            if(wdtResults.Length == 0)
                Console.WriteLine("No input WDT found for map " + Settings.MapName + ", using default flags.");

            var wotlkWDTPath = wdtResults.FirstOrDefault();
            var wotlkWDTExists = File.Exists(wotlkWDTPath);
            Warcraft.NET.Files.WDT.Root.WotLK.WorldDataTable wotlkWDT = new();
            var wotlkFlags = new MPHDFlags();

            if (wotlkWDTExists)
            {
                wotlkWDT = new Warcraft.NET.Files.WDT.Root.WotLK.WorldDataTable(File.ReadAllBytes(wotlkWDTPath));
                wotlkFlags = wotlkWDT.Header.Flags;
            }

            var currentWDTPath = Path.Combine(ExportHelper.GetExportDirectory(), "world", "maps", Settings.MapName, Settings.MapName + ".wdt");

            var rootWDT = new Warcraft.NET.Files.WDT.Root.WotLK.WorldDataTable()
            {
                Version = new Warcraft.NET.Files.WDT.Chunks.MVER(18),
                Header = new Warcraft.NET.Files.WDT.Chunks.MPHD(),
                Tiles = new Warcraft.NET.Files.WDT.Chunks.MAIN(),
                WorldModelObjects = new Warcraft.NET.Files.ADT.Chunks.MWMO(),
                WorldModelObjectPlacementInfo = new Warcraft.NET.Files.ADT.Chunks.MODF(),
            };

            rootWDT.Header.Flags = wotlkFlags | MPHDFlags.hasHeightTexturing;

            var availableADTs = Directory.GetFiles(Path.Combine(ExportHelper.GetExportDirectory(), "world", "maps", Settings.MapName), "*.adt", SearchOption.TopDirectoryOnly).Select(x => x.ToLowerInvariant()).ToHashSet();  

            for (byte x = 0; x < 64; x++)
            {
                for (byte y = 0; y < 64; y++)
                {
                    var hasADT = availableADTs.Contains(Path.Combine(ExportHelper.GetExportDirectory(), "world", "maps", Settings.MapName, Settings.MapName + "_" + x + "_" + y + ".adt").ToLowerInvariant());

                    rootWDT.Tiles.Entries[x, y] = new Warcraft.NET.Files.WDT.Entries.MAINEntry()
                    {
                        Flags = 0,
                        AsyncId = 0
                    };

                    if (hasADT)
                        rootWDT.Tiles.Entries[x, y].Flags |= MAINFlags.HasAdt;

                    if (wotlkWDTExists)
                    {
                        if (rootWDT.Tiles.Entries[x, y].Flags.HasFlag(MAINFlags.HasWater))
                            rootWDT.Tiles.Entries[x, y].Flags |= MAINFlags.HasWater;

                        rootWDT.Tiles.Entries[x, y].AsyncId = wotlkWDT.Tiles.Entries[x, y].AsyncId;
                    }
                }
            }
            return rootWDT;
        }

        public static Warcraft.NET.Files.WDT.Root.BfA.WorldDataTable Generate()
        {
            var wdtResults = Directory.GetFiles(Settings.InputDir, Settings.MapName + ".wdt", SearchOption.AllDirectories);
            if (wdtResults.Length == 0)
                Console.WriteLine("No input WDT found for map " + Settings.MapName + ", using default flags.");

            var wotlkWDTPath = wdtResults.FirstOrDefault();
            var wotlkWDTExists = File.Exists(wotlkWDTPath);
            Warcraft.NET.Files.WDT.Root.BfA.WorldDataTable wotlkWDT = new();
            var wotlkFlags = new MPHDFlags();

            if (wotlkWDTExists)
            {
                wotlkWDT = new Warcraft.NET.Files.WDT.Root.BfA.WorldDataTable(File.ReadAllBytes(wotlkWDTPath));
                wotlkFlags = wotlkWDT.Header.Flags;
            }

            var currentWDTPath = Path.Combine(ExportHelper.GetExportDirectory(), "world", "maps", Settings.MapName, Settings.MapName + ".wdt");

            var rootWDT = new Warcraft.NET.Files.WDT.Root.BfA.WorldDataTable()
            {
                Version = new Warcraft.NET.Files.WDT.Chunks.MVER(18),
                Header = new Warcraft.NET.Files.WDT.Chunks.MPHD(),
                Tiles = new Warcraft.NET.Files.WDT.Chunks.MAIN(),
                WorldModelObjects = new Warcraft.NET.Files.ADT.Chunks.MWMO(),
                WorldModelObjectPlacementInfo = new Warcraft.NET.Files.ADT.Chunks.MODF(),
            };

            rootWDT.Ids = new Warcraft.NET.Files.WDT.Chunks.BfA.MAID();

            rootWDT.Header.Flags = wotlkFlags | MPHDFlags.hasHeightTexturing | MPHDFlags.HasMAID;

            if (File.Exists(Path.Combine(ExportHelper.GetExportDirectory(), "world", "maps", Settings.MapName, Settings.MapName + "_lgt.wdt")))
                rootWDT.Header.LgtFileID = GetOrAssignFileDataID("world/maps/" + Settings.MapName + "/" + Settings.MapName + "_lgt.wdt");
            else
                rootWDT.Header.LgtFileID = 1249658;

            if (File.Exists(Path.Combine(ExportHelper.GetExportDirectory(), "world", "maps", Settings.MapName, Settings.MapName + "_occ.wdt")))
                rootWDT.Header.OccFileID = GetOrAssignFileDataID("world/maps/" + Settings.MapName + "/" + Settings.MapName + "_occ.wdt");
            else
                rootWDT.Header.OccFileID = 1100613;

            if (File.Exists(Path.Combine(ExportHelper.GetExportDirectory(), "world", "maps", Settings.MapName, Settings.MapName + "_fogs.wdt")))
                rootWDT.Header.FogsFileID = GetOrAssignFileDataID("world/maps/" + Settings.MapName + "/" + Settings.MapName + "_fogs.wdt");
            else
                rootWDT.Header.FogsFileID = 1668535;

            if (File.Exists(Path.Combine(ExportHelper.GetExportDirectory(), "world", "maps", Settings.MapName, Settings.MapName + "_mpv.wdt")))
                rootWDT.Header.MpvFileID = GetOrAssignFileDataID("world/maps/" + Settings.MapName + "/" + Settings.MapName + "_mpv.wdt");
            else
                rootWDT.Header.MpvFileID = 2495665;

            if (File.Exists(Path.Combine(ExportHelper.GetExportDirectory(), "world", "maps", Settings.MapName, Settings.MapName + ".tex")))
                rootWDT.Header.TexFileID = GetOrAssignFileDataID("world/maps/" + Settings.MapName + "/" + Settings.MapName + ".tex");
            else
                rootWDT.Header.TexFileID = 1249780;

            if (File.Exists(Path.Combine(ExportHelper.GetExportDirectory(), "world", "maps", Settings.MapName, Settings.MapName + ".wdl")))
                rootWDT.Header.WdlFileID = GetOrAssignFileDataID("world/maps/" + Settings.MapName + "/" + Settings.MapName + ".wdl");
            else
                throw new Exception("WDL not found in output directory for map " + Settings.MapName + ". This is required for the root WDT.");

            rootWDT.Header.Pd4FileID = 0;

            for (byte x = 0; x < 64; x++)
            {
                for (byte y = 0; y < 64; y++)
                {
                    var hasADT = File.Exists(Path.Combine(ExportHelper.GetExportDirectory(), "world", "maps", Settings.MapName, Settings.MapName + "_" + x + "_" + y + ".adt"));
                    var hasLodADT = File.Exists(Path.Combine(ExportHelper.GetExportDirectory(), "world", "maps", Settings.MapName, Settings.MapName + "_" + x + "_" + y + "_lod.adt"));
                    var hasMapTexture = File.Exists(Path.Combine(ExportHelper.GetExportDirectory(), "world", "maptextures", Settings.MapName, Settings.MapName + "_" + x.ToString().PadLeft(2, '0') + "_" + y.ToString().PadLeft(2, '0') + ".blp"));
                    var hasMapTextureN = File.Exists(Path.Combine(ExportHelper.GetExportDirectory(), "world", "maptextures", Settings.MapName, Settings.MapName + "_" + x.ToString().PadLeft(2, '0') + "_" + y.ToString().PadLeft(2, '0') + "_n.blp"));
                    var hasMinimapTexture = File.Exists(Path.Combine(ExportHelper.GetExportDirectory(), "world", "minimaps", Settings.MapName, "map" + x.ToString().PadLeft(2, '0') + "_" + y.ToString().PadLeft(2, '0') + ".blp"));

                    rootWDT.Tiles.Entries[x, y] = new Warcraft.NET.Files.WDT.Entries.MAINEntry()
                    {
                        Flags = 0,
                        AsyncId = 0
                    };

                    if (hasADT)
                        rootWDT.Tiles.Entries[x, y].Flags |= MAINFlags.HasAdt;

                    if (wotlkWDTExists)
                    {
                        if (rootWDT.Tiles.Entries[x, y].Flags.HasFlag(MAINFlags.HasWater))
                            rootWDT.Tiles.Entries[x, y].Flags |= MAINFlags.HasWater;

                        rootWDT.Tiles.Entries[x, y].AsyncId = wotlkWDT.Tiles.Entries[x, y].AsyncId;
                    }

                    rootWDT.Ids.Entries[x, y] = new Warcraft.NET.Files.WDT.Entries.BfA.MAIDEntry()
                    {
                        RootAdtFileId = hasADT ? GetOrAssignFileDataID("world/maps/" + Settings.MapName + "/" + Settings.MapName + "_" + x + "_" + y + ".adt") : 0,
                        Obj0AdtFileId = hasADT ? GetOrAssignFileDataID("world/maps/" + Settings.MapName + "/" + Settings.MapName + "_" + x + "_" + y + "_obj0.adt") : 0,
                        Obj1AdtFileId = hasADT ? GetOrAssignFileDataID("world/maps/" + Settings.MapName + "/" + Settings.MapName + "_" + x + "_" + y + "_obj1.adt") : 0,
                        Tex0AdtFileId = hasADT ? GetOrAssignFileDataID("world/maps/" + Settings.MapName + "/" + Settings.MapName + "_" + x + "_" + y + "_tex0.adt") : 0,
                        LodAdtFileId = hasLodADT ? GetOrAssignFileDataID("world/maps/" + Settings.MapName + "/" + Settings.MapName + "_" + x + "_" + y + "_lod.adt") : 0,
                        MapTextureFileId = hasMapTexture ? GetOrAssignFileDataID("world/maptextures/" + Settings.MapName + "/" + Settings.MapName + "_" + x.ToString().PadLeft(2, '0') + "_" + y.ToString().PadLeft(2, '0') + ".blp") : 0,
                        MapTextureNFileId = hasMapTextureN ? GetOrAssignFileDataID("world/maptextures/" + Settings.MapName + "/" + Settings.MapName + "_" + x.ToString().PadLeft(2, '0') + "_" + y.ToString().PadLeft(2, '0') + "_n.blp") : 0,
                        MinimapTextureFileId = hasMinimapTexture ? GetOrAssignFileDataID("world/minimaps/" + Settings.MapName + "/map" + x.ToString().PadLeft(2, '0') + "_" + y.ToString().PadLeft(2, '0') + ".blp") : 0
                    };
                }
            }

            return rootWDT;
        }

        private static uint GetOrAssignFileDataID(string filename)
        {
            if (Listfile.ReverseMap.TryGetValue(filename.ToLower(), out var fileDataID))
            {
                return fileDataID;
            }
            else
            {
                var newFileDataID = Listfile.GetNextFreeFileDataID();
                Console.WriteLine("Assigning new file data ID " + newFileDataID + " for " + filename + ".");
                Listfile.AddCustomFileDataIDToListfile(newFileDataID, filename);
                return newFileDataID;
            }
        }
    }
}
