using MapUpconverter.Utils;
using Warcraft.NET.Files.ADT.Chunks;
using Warcraft.NET.Files.ADT.Entries.MoP;
using Warcraft.NET.Files.WDT.Flags;

namespace MapUpconverter.ADT
{
    public static class Tex0
    {
        public static Warcraft.NET.Files.ADT.TerrainTexture.BfA.TerrainTexture Convert(Warcraft.NET.Files.ADT.Terrain.Wotlk.Terrain wotlkRootADT)
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
            
            // check if GroundEffectMap is all 0 in all MCNKs
            var regenGroundEffectMap = wotlkRootADT.Chunks.Any(x => x.Header.GroundEffectMap.All(y => y == 0));

            for (int i = 0; i < 256; i++)
            {
                var wotlkChunk = wotlkRootADT.Chunks[i];

                // Apparently required for Noggit output -- verify if still correct after Noggit ground effect editor is released
                if(regenGroundEffectMap)
                    wotlkChunk.FixGroundEffectMap(wotlkFlags.HasFlag(MPHDFlags.BigAlpha));

                bfaTex0.Chunks[i] = new Warcraft.NET.Files.ADT.TerrainTexture.MCNK
                {
                    AlphaMaps = wotlkChunk.AlphaMaps,
                    TextureLayers = wotlkChunk.TextureLayers,
                    BakedShadows = wotlkChunk.BakedShadows
                };

                // TODO: This might be neat to set properly!
                // bfaTex0.Chunks[i].TerrainMaterials = new Warcraft.NET.Files.ADT.TerrainTexture.MapChunk.SubChunks.MCMT();
            }

            return bfaTex0;
        }
    }
}
