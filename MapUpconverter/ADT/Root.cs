using Warcraft.NET.Files.ADT.Chunks;
using Warcraft.NET.Files.ADT.Flags;

namespace MapUpconverter.ADT
{
    public static class Root
    {
        public static Warcraft.NET.Files.ADT.Terrain.BfA.Terrain Convert(Warcraft.NET.Files.ADT.Terrain.Wotlk.Terrain wotlkRootADT)
        {
            var bfaRoot = new Warcraft.NET.Files.ADT.Terrain.BfA.Terrain
            {
                Version = new MVER(18),
                Header = wotlkRootADT.Header,
                Water = wotlkRootADT.Water,
                BoundingBox = wotlkRootADT.BoundingBox ?? new MFBO(-1000, 2000),
                Chunks = new Warcraft.NET.Files.ADT.Terrain.BfA.MCNK[256]
            };

            bfaRoot.Header.Flags = new MHDRFlags();
            bfaRoot.Header.Flags |= MHDRFlags.MFBO;

            for (int i = 0; i < 256; i++)
            {
                var wotlkChunk = wotlkRootADT.Chunks[i];

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
