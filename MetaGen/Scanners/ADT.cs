using MetaGen.Services;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace MetaGen.Scanners
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
        private static ConcurrentDictionary<uint, List<uint>> TextureGroundEffectMap { get; set; } = new ConcurrentDictionary<uint, List<uint>>();

        public static void LoadCurrent(string metaFolder)
        {
            if (File.Exists(Path.Combine(metaFolder, "TextureInfoByFileId.json")))
            {
                var textureInfoPath = Path.Combine(metaFolder, "TextureInfoByFileId.json");
                var currentByID = JsonConvert.DeserializeObject<Dictionary<string, TextureInfo>>(File.ReadAllText(textureInfoPath)) ?? throw new Exception("Failed to read TextureInfoByFileId.json");
                TextureInfoMap = new ConcurrentDictionary<uint, TextureInfo>(currentByID.ToDictionary(x => uint.Parse(x.Key), x => x.Value));
            }

            if (File.Exists(Path.Combine(metaFolder, "GroundEffectIDsByTextureFileID.json")))
            {
                var groundEffectPath = Path.Combine(metaFolder, "GroundEffectIDsByTextureFileID.json");
                var currentByID = JsonConvert.DeserializeObject<Dictionary<string, List<uint>>>(File.ReadAllText(groundEffectPath)) ?? throw new Exception("Failed to read GroundEffectIDsByTextureFileID");
                TextureGroundEffectMap = new ConcurrentDictionary<uint, List<uint>>(currentByID.ToDictionary(x => uint.Parse(x.Key), x => x.Value));
            }
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

                if (bfaADT.TextureDiffuseIds != null && bfaADT.Chunks != null)
                {
                    for (var i = 0; i < bfaADT.Chunks.Length; i++)
                    {
                        var chunk = bfaADT.Chunks[i];
                        if (chunk.TextureLayers == null)
                            continue;

                        for (var j = 0; j < chunk.TextureLayers.Layers.Count; j++)
                        {
                            var layer = chunk.TextureLayers.Layers[j];
                            if (layer.EffectID != 0)
                            {
                                var didForTexture = bfaADT.TextureDiffuseIds.Textures[(int)layer.TextureID];

                                if (didForTexture == 0)
                                    continue;

                                if (!TextureGroundEffectMap.ContainsKey(didForTexture))
                                    TextureGroundEffectMap[didForTexture] = new List<uint>();

                                if (!TextureGroundEffectMap[didForTexture].Contains(layer.EffectID))
                                    TextureGroundEffectMap[didForTexture].Add(layer.EffectID);
                            }
                        }
                    }
                }


                if (bfaADT.TextureParameters == null)
                    return false;

                for (var i = 0; i < bfaADT.TextureParameters.TextureFlagEntries.Count; i++)
                {
                    var mtxp = bfaADT.TextureParameters.TextureFlagEntries[i];
                    if (mtxp.HeightScale == 0 && mtxp.HeightOffset == 1)
                        continue;

                    if (bfaADT.TextureHeightIds == null || bfaADT.TextureHeightIds.Textures[i] == 0)
                        continue;

                    if (TextureInfoMap.TryGetValue(bfaADT.TextureHeightIds.Textures[i], out var existingInfo))
                    {

                        if (existingInfo.Scale != mtxp.TextureScale || existingInfo.HeightScale != mtxp.HeightScale || existingInfo.HeightOffset != mtxp.HeightOffset)
                        {
                            // Check if the old values were defaults, if so don't bother 
                            if (existingInfo.Scale == 1 && existingInfo.HeightScale == 6 && existingInfo.HeightOffset == 1)
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

        public static void RemoveDupes()
        {
            var dbcd = new DBCD.DBCD(new CASCDBCProvider(), new DBCD.Providers.GithubDBDProvider());

            var groundEffectDB = dbcd.Load("GroundEffectTexture");
            var groundEffectMap = new Dictionary<int, int>();
            foreach (var geRow in groundEffectDB.Values)
            {
                var rowString = "";
                foreach (var field in groundEffectDB.AvailableColumns)
                {
                    var value = geRow[field];
                    switch (value.GetType().ToString())
                    {
                        case "System.SByte[]":
                            var sbyteArray = (sbyte[])value;
                            for (var i = 0; i < sbyteArray.Length; i++)
                            {
                                rowString += sbyteArray[i].ToString() + "_";
                            }
                            break;
                        case "System.UInt16[]":
                            var uintArray = (ushort[])value;
                            for (var i = 0; i < uintArray.Length; i++)
                            {
                                rowString += uintArray[i].ToString() + "_";
                            }
                            break;
                        case "System.Byte":
                            rowString += ((byte)value).ToString() + "_";
                            break;
                        case "System.Int32":
                            rowString += ((int)value).ToString() + "_";
                            break;
                        case "System.UInt32":
                            rowString += ((uint)value).ToString() + "_";
                            break;
                        default:
                            Console.WriteLine("Unknown type: " + value.GetType());
                            break;
                    }
                }

                groundEffectMap.Add((int)geRow["ID"], rowString.GetHashCode());
            }

            foreach (var texture in TextureGroundEffectMap)
            {
                var groundEffects = texture.Value;
                var newGEs = new List<uint>();
                var usedHashes = new List<int>();
                foreach (var ge in groundEffects)
                {
                    if (groundEffectMap.TryGetValue((int)ge, out var hash))
                    {
                        if (!usedHashes.Contains(hash))
                        {
                            newGEs.Add(ge);
                            usedHashes.Add(hash);
                        }
                    }
                }

                TextureGroundEffectMap[texture.Key] = newGEs;
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
                if (Listfile.NameMap.TryGetValue(kv.Key, out var filename))
                    textureInfoByFilePath[filename.Replace("_h.blp", ".blp")] = kv.Value;
            }

            File.WriteAllText(path, JsonConvert.SerializeObject(textureInfoByFilePath.OrderBy(x => x.Key).ToDictionary(), Formatting.Indented));
        }

        public static void SaveGroundEffectIDsByTextureFileID(string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(TextureGroundEffectMap.OrderBy(x => x.Key).ToDictionary(x => x.Key.ToString(), x => x.Value), Formatting.Indented));
        }

        public static void SaveGroundEffectIDsByTextureFilePath(string path)
        {
            var groundEffectIDsByFilePath = new Dictionary<string, List<uint>>();
            foreach (var kv in TextureGroundEffectMap)
            {
                if (Listfile.NameMap.TryGetValue(kv.Key, out var filename))
                    groundEffectIDsByFilePath[filename.Replace("_h.blp", ".blp").Replace("_s.blp", ".blp")] = kv.Value;
            }

            File.WriteAllText(path, JsonConvert.SerializeObject(groundEffectIDsByFilePath.OrderBy(x => x.Key).ToDictionary(), Formatting.Indented));
        }
    }
}
