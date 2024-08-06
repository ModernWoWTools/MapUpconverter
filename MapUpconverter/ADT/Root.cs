using Warcraft.NET.Files.ADT.Chunks;
using Warcraft.NET.Files.ADT.Flags;
using Warcraft.NET.Files.WDT.Flags;

namespace MapUpconverter.ADT
{
    public static class Root
    {
        public static Warcraft.NET.Files.ADT.Terrain.BfA.Terrain Convert(Warcraft.NET.Files.ADT.Terrain.Wotlk.Terrain wotlkRootADT)
        {
            var wotlkWDTPath = Path.Combine(Settings.InputDir, Settings.MapName + ".wdt");
            var wotlkFlags = new MPHDFlags();
            var wotlkWDTExists = File.Exists(wotlkWDTPath);
            Warcraft.NET.Files.WDT.Root.BfA.WorldDataTable wotlkWDT = new();

            if (wotlkWDTExists)
            {
                wotlkWDT = new Warcraft.NET.Files.WDT.Root.BfA.WorldDataTable(File.ReadAllBytes(wotlkWDTPath));
                wotlkFlags = wotlkWDT.Header.Flags;
            }

            var bfaRoot = new Warcraft.NET.Files.ADT.Terrain.BfA.Terrain
            {
                Version = new MVER(18),
                Header = wotlkRootADT.Header,
                Water = wotlkRootADT.Water,
                BoundingBox = new MFBO(-30000, 30000),
                Chunks = new Warcraft.NET.Files.ADT.Terrain.BfA.MCNK[256]
            };

            bfaRoot.Header.Flags = new MHDRFlags();
            bfaRoot.Header.Flags |= MHDRFlags.MFBO;

            // check if GroundEffectMap is all 0 in all MCNKs
            var regenGroundEffectMap = wotlkRootADT.Chunks.All(x => x.Header.GroundEffectMap.All(y => y == 0));

            for (int i = 0; i < 256; i++)
            {
                var wotlkChunk = wotlkRootADT.Chunks[i];

                // Apparently required for Noggit output -- verify if still correct after Noggit ground effect editor is released
                if (regenGroundEffectMap)
                    wotlkChunk.FixGroundEffectMap(wotlkFlags.HasFlag(MPHDFlags.BigAlpha));

                bfaRoot.Chunks[i] = new Warcraft.NET.Files.ADT.Terrain.BfA.MCNK();
                bfaRoot.Chunks[i].Header = wotlkChunk.Header;
                // Note: bfaRoot.Chunks[i].Header.Flags might need updating for e.g. high_res_holes and such when this becomes a thing

                bfaRoot.Chunks[i].Heightmap = wotlkChunk.Heightmap;
                bfaRoot.Chunks[i].VertexNormals = wotlkChunk.VertexNormals;
                bfaRoot.Chunks[i].VertexShading = wotlkChunk.VertexShading;
                bfaRoot.Chunks[i].SoundEmitters = wotlkChunk.SoundEmitters;

                // TODO: Does Noggit need adding of this maybe?
                // bfaRoot.Chunks[i].VertexLighting = new Warcraft.NET.Files.ADT.Terrain.MCNK.SubChunks.MCLV();
            }

            return bfaRoot;
        }
    }
}
