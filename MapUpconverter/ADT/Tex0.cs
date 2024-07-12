using MapUpconverter.Utils;
using Warcraft.NET.Files.ADT.Chunks;
using Warcraft.NET.Files.ADT.Entries.MoP;

namespace MapUpconverter.ADT
{
    public static class Tex0
    {
        public static Warcraft.NET.Files.ADT.TerrainTexture.BfA.TerrainTexture Convert(Warcraft.NET.Files.ADT.Terrain.Wotlk.Terrain wotlkRootADT)
        {
            var bfaTex0 = new Warcraft.NET.Files.ADT.TerrainTexture.BfA.TerrainTexture
            {
                Version = new MVER(18),
                Chunks = new Warcraft.NET.Files.ADT.TerrainTexture.MCNK[256]
            };

            bfaTex0.TextureDiffuseIds = new();
            bfaTex0.TextureHeightIds = new();
            bfaTex0.TextureFlags = new();

            foreach (var texture in wotlkRootADT.Textures.Filenames)
            {
                var diffuseTexture = texture.ToLowerInvariant();

                // We prefer _s.blps for MDID
                if (!diffuseTexture.EndsWith("_s.blp") && Listfile.ReverseMap.TryGetValue(diffuseTexture.Replace(".blp", "_s.blp"), out var diffuseFDID))
                {
                    bfaTex0.TextureDiffuseIds.Textures.Add(diffuseFDID);
                }
                else
                {
                    if (Listfile.ReverseMap.TryGetValue(texture.ToLowerInvariant(), out var diffuseNonSpecFDID))
                    {
                        bfaTex0.TextureDiffuseIds.Textures.Add(diffuseNonSpecFDID);
                    }
                    else
                    {
                        Console.WriteLine("Could not find diffuse texture FDID for " + texture + " (" + diffuseTexture + ")");
                        bfaTex0.TextureDiffuseIds.Textures.Add(0);
                    }
                }

                if (diffuseTexture.EndsWith("_s.blp"))
                    diffuseTexture = diffuseTexture.Replace("_s.blp", ".blp");

                var heightTexture = diffuseTexture.Replace(".blp", "_h.blp");

                if (Listfile.ReverseMap.TryGetValue(heightTexture.ToLowerInvariant(), out var heightFDID))
                {
                    bfaTex0.TextureHeightIds.Textures.Add(heightFDID);
                }
                else
                {
                    Console.WriteLine("Could not find height texture FDID for " + texture + " (" + heightTexture + ")");
                    bfaTex0.TextureHeightIds.Textures.Add(0);
                }

                var mtxpEntry = new MTXPEntry();
                if (HeightInfo.textureInfoMap.TryGetValue(diffuseTexture, out var heightInfo))
                {
                    mtxpEntry.TextureScale = heightInfo.Scale;
                    mtxpEntry.HeightScale = heightInfo.HeightScale;
                    mtxpEntry.HeightOffset = heightInfo.HeightOffset;
                }
                else
                {
                    Console.WriteLine("Could not find height info for " + diffuseTexture);
                }

                bfaTex0.TextureFlags.TextureFlagEntries.Add(mtxpEntry);
            }

            for (int i = 0; i < 256; i++)
            {
                var wotlkChunk = wotlkRootADT.Chunks[i];

                // Apparently required for Noggit output -- verify if still correct after Noggit ground effect editor is released
                // TODO: Check WDT if it has the same flag set
                wotlkChunk.FixGroundEffectMap(true);

                bfaTex0.Chunks[i] = new Warcraft.NET.Files.ADT.TerrainTexture.MCNK();

                bfaTex0.Chunks[i].AlphaMaps = wotlkChunk.AlphaMaps;
                bfaTex0.Chunks[i].TextureLayers = wotlkChunk.TextureLayers;
                bfaTex0.Chunks[i].BakedShadows = wotlkChunk.BakedShadows;

                // TODO: This might be neat to set properly!
                // bfaTex0.Chunks[i].TerrainMaterials = new Warcraft.NET.Files.ADT.TerrainTexture.MapChunk.SubChunks.MCMT();
            }

            return bfaTex0;
        }
    }
}
