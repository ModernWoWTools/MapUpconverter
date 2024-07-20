using MetaGen.Properties.Services;
using Newtonsoft.Json;
using SharpCompress.Common;
using System.Collections.Concurrent;

namespace MetaGen.Properties.Scanners
{
    public class TextureInfo
    {
        public int Scale { get; set; }
        public float HeightScale { get; set; }
        public float HeightOffset { get; set; }
    }

    public static class ADT
    {
        private static ConcurrentDictionary<uint, TextureInfo> TextureInfoMap { get; set; } = new ConcurrentDictionary<uint, TextureInfo>();

        public static void LoadCurrent(string metaFolder)
        {
            if(!File.Exists(Path.Combine(metaFolder, "TextureInfoByFileId.json")))
                return;

            var textureInfoPath = Path.Combine(metaFolder, "TextureInfoByFileId.json");
            var currentByID = JsonConvert.DeserializeObject<Dictionary<string, TextureInfo>>(File.ReadAllText(textureInfoPath)) ?? throw new Exception("Failed to read TextureInfoByFileId.json");
            TextureInfoMap = new ConcurrentDictionary<uint, TextureInfo>(currentByID.ToDictionary(x => uint.Parse(x.Key), x => x.Value));
        }

        public static bool ProcessADT(uint adt)
        {
            using (var ms = new MemoryStream())
            {
                if (!CASC.FileExists(adt))
                    return false;

                var adtStream = CASC.OpenFile(adt);
                if (adtStream == null)
                    return false;

                adtStream.CopyTo(ms);
                ms.Position = 0;

                var bfaADT = new Warcraft.NET.Files.ADT.TerrainTexture.BfA.TerrainTexture(ms.ToArray());

                if (bfaADT.TextureParameters == null)
                    return false;

                for (var i = 0; i < bfaADT.TextureParameters.TextureFlagEntries.Count; i++)
                {
                    var mtxp = bfaADT.TextureParameters.TextureFlagEntries[i];
                    if (mtxp.HeightScale == 0 && mtxp.HeightOffset == 1)
                        continue;

                    if (bfaADT.TextureHeightIds == null || bfaADT.TextureHeightIds.Textures[i] == 0)
                        continue;

                    if(TextureInfoMap.TryGetValue(bfaADT.TextureHeightIds.Textures[i], out var existingInfo)){

                        if(existingInfo.Scale != mtxp.TextureScale || existingInfo.HeightScale != mtxp.HeightScale || existingInfo.HeightOffset != mtxp.HeightOffset)
                        {
                            // Check if the old values were defaults, if so don't bother 
                            if(existingInfo.Scale == 1 && existingInfo.HeightScale == 6 && existingInfo.HeightOffset == 1)
                                continue;

                            Console.WriteLine("Texture " + bfaADT.TextureHeightIds.Textures[i] + " has conflicting info");
                            Console.WriteLine("\t Existing: " + existingInfo.Scale + " " + existingInfo.HeightScale + " " + existingInfo.HeightOffset);
                            Console.WriteLine("\t New: " + mtxp.TextureScale + " " + mtxp.HeightScale + " " + mtxp.HeightOffset);
                        }
                    }

                    TextureInfoMap[bfaADT.TextureHeightIds.Textures[i]] = new TextureInfo
                    {
                        Scale = mtxp.TextureScale,
                        HeightScale = mtxp.HeightScale,
                        HeightOffset = mtxp.HeightOffset
                    };
                }

                return false;
            }
        }

        public static void AddDefaultsFromListfile()
        {
            Console.WriteLine("Adding tilesets from listfile starting with tileset and ending in _h.blp with default values (can be wrong)");
            foreach (var file in Listfile.NameMap.Where(x => x.Value.EndsWith("_h.blp") && x.Value.StartsWith("tileset")))
            {
                TextureInfo textureInfo = new TextureInfo
                {
                    Scale = 1,
                    HeightScale = 6,
                    HeightOffset = 1
                };

                if (!TextureInfoMap.ContainsKey(file.Key))
                {
                    Console.WriteLine("Adding " + file.Value + " from listfile");
                    TextureInfoMap[file.Key] = textureInfo;
                }
            }
        }

        public static void SaveTextureInfoByFileID(string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(TextureInfoMap.OrderBy(x => x.Key).ToDictionary(x => x.Key.ToString(), x => x.Value), Formatting.Indented));
        }

        public static void SaveTextureInfoByFilePath(string path)
        {
            var textureInfoByFilePath = new Dictionary<string, TextureInfo>();
            foreach (var kv in TextureInfoMap)
            {
                if(Listfile.NameMap.TryGetValue(kv.Key, out var filename))
                    textureInfoByFilePath[filename.Replace("_h.blp", ".blp")] = kv.Value;
            }

            File.WriteAllText(path, JsonConvert.SerializeObject(textureInfoByFilePath.OrderBy(x => x.Key).ToDictionary(), Formatting.Indented));
        }
    }
}
