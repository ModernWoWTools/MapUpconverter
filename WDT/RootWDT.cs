
using MapUpconverter.Utils;
using Warcraft.NET.Files.WDT.Flags;

namespace MapUpconverter.WDT
{
    public static class RootWDT
    {
        public static Warcraft.NET.Files.WDT.Root.BfA.WorldDataTable Generate()
        {
            var wotlkWDTPath = Path.Combine(Settings.InputDir, Settings.MapName + ".wdt");
            var wotlkFlags = new MPHDFlags();
            if (File.Exists(wotlkWDTPath))
            {
                var wotlkWDT = new Warcraft.NET.Files.WDT.Root.BfA.WorldDataTable(File.ReadAllBytes(wotlkWDTPath));
                wotlkFlags = wotlkWDT.Header.Flags;
            }

            var currentWDTPath = Path.Combine(Settings.OutputDir, Settings.MapName + ".wdt");

            Warcraft.NET.Files.WDT.Root.BfA.WorldDataTable rootWDT;

            if (File.Exists(currentWDTPath))
            {
                rootWDT = new Warcraft.NET.Files.WDT.Root.BfA.WorldDataTable(File.ReadAllBytes(currentWDTPath));
            }
            else
            {
                // We need to generate an entirely new WDT, oh dear.
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Generating an entirely new WDT. This is not fully implemented yet, and will not work as expected.");
                Console.ResetColor();

                rootWDT = new Warcraft.NET.Files.WDT.Root.BfA.WorldDataTable()
                {
                    Version = new Warcraft.NET.Files.WDT.Chunks.MVER(18),
                    Header = new Warcraft.NET.Files.WDT.Chunks.MPHD(),
                    Ids = new Warcraft.NET.Files.WDT.Chunks.BfA.MAID(),
                    Tiles = new Warcraft.NET.Files.WDT.Chunks.MAIN(),
                    WorldModelObjects = new Warcraft.NET.Files.ADT.Chunks.MWMO(),
                    WorldModelObjectPlacementInfo = new Warcraft.NET.Files.ADT.Chunks.MODF(),
                };

                rootWDT.Header.Flags = wotlkFlags | MPHDFlags.hasHeightTexturing | MPHDFlags.HasMAID;

                // If filenames for these are not found in the listfile, use empty files from other maps
                rootWDT.Header.LgtFileID = GetOrAssignFileDataID("world/maps/" + Settings.MapName + "/" + Settings.MapName + "_lgt.wdt", 1249658);
                rootWDT.Header.OccFileID = GetOrAssignFileDataID("world/maps/" + Settings.MapName + "/" + Settings.MapName + "_occ.wdt", 1100613);
                rootWDT.Header.FogsFileID = GetOrAssignFileDataID("world/maps/" + Settings.MapName + "/" + Settings.MapName + "_fogs.wdt", 1668535);
                rootWDT.Header.MpvFileID = GetOrAssignFileDataID("world/maps/" + Settings.MapName + "/" + Settings.MapName + "_mpv.wdt", 2495665);
                rootWDT.Header.TexFileID = GetOrAssignFileDataID("world/maps/" + Settings.MapName + "/" + Settings.MapName + ".tex", 1249780);
                rootWDT.Header.WdlFileID = GetOrAssignFileDataID("world/maps/" + Settings.MapName + "/" + Settings.MapName + ".wdl");
                rootWDT.Header.Pd4FileID = 0;

                for (byte x = 0; x < 64; x++)
                {
                    for (byte y = 0; y < 64; y++)
                    {
                        var hasADT = File.Exists(Path.Combine(Settings.OutputDir, Settings.MapName + "_" + x + "_" + y + ".adt"));
                        var hasLodADT = File.Exists(Path.Combine(Settings.OutputDir, Settings.MapName + "_" + x + "_" + y + "_lod.adt"));
                        var hasMapTexture = File.Exists(Path.Combine(Settings.OutputDir, Settings.MapName + "_" + x.ToString().PadLeft(2, '0') + "_" + y.ToString().PadLeft(2, '0') + ".blp"));
                        var hasMapTextureN = File.Exists(Path.Combine(Settings.OutputDir, Settings.MapName + "_" + x.ToString().PadLeft(2, '0') + "_" + y.ToString().PadLeft(2, '0') + "_n.blp"));
                        var hasMinimapTexture = File.Exists(Path.Combine(Settings.OutputDir, "map" + x.ToString().PadLeft(2, '0') + "_" + y.ToString().PadLeft(2, '0') + ".blp"));

                        rootWDT.Tiles.Entries[x, y] = new Warcraft.NET.Files.WDT.Entries.MAINEntry()
                        {
                            Flags = 0,
                            AsyncId = 0
                        };

                        if(hasADT)
                            rootWDT.Tiles.Entries[x, y].Flags |= MAINFlags.HasAdt;

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
            }

            return rootWDT;
        }

        private static uint GetOrAssignFileDataID(string filename, uint defaultFileDataID = 0)
        {
            if (Listfile.ReverseMap.TryGetValue(filename.ToLower(), out var fileDataID))
            {
                return fileDataID;
            }
            else
            {
                if (defaultFileDataID != 0)
                    return defaultFileDataID;
                else
                    throw new NotImplementedException("File ID assignment is not yet implemented. Requested FDID for " + filename);
            }
        }
    }
}
