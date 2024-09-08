using MetaGen.Services;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using static MapUpconverter.Utils.BoundingBoxInfo;

namespace MetaGen.Scanners
{
    public static class Models
    {
        private static ConcurrentDictionary<uint, JSONCAaBox> boundingBoxBlobDict = [];

        public static void LoadCurrent(string metaFolder)
        {
            if (!File.Exists(Path.Combine(metaFolder, "blob.json")))
                return;

            var blobPath = Path.Combine(metaFolder, "blob.json");
            boundingBoxBlobDict = JsonConvert.DeserializeObject<ConcurrentDictionary<uint, JSONCAaBox>>(File.ReadAllText(blobPath)) ?? throw new Exception("Failed to read blob.json");
        }

        public static bool ProcessM2(uint model)
        {
            using (var ms = new MemoryStream())
            {
                if (!CASC.FileExists(model))
                    return false;

                try
                {
                    var modelStream = CASC.OpenFile(model);
                    if (modelStream == null)
                        return false;

                    modelStream.CopyTo(ms);

                    ms.Position = 0;

                    var boundingBox = MapUpconverter.Utils.BoundingBoxInfo.ProcessM2(ms.ToArray());
                    boundingBoxBlobDict[model] = boundingBox;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error parsing M2 " + model + ": " + e.Message);
                    return false;
                }
            }

            return true;
        }

        public static bool ProcessWMO(uint model)
        {
            using (var ms = new MemoryStream())
            {
                if (!CASC.FileExists(model))
                    return false;

                try
                {
                    var modelStream = CASC.OpenFile(model);
                    if (modelStream == null)
                        return false;

                    modelStream.CopyTo(ms);

                    ms.Position = 0;

                    var boundingBox = MapUpconverter.Utils.BoundingBoxInfo.ProcessWMO(ms.ToArray());
                    boundingBoxBlobDict[model] = boundingBox;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error parsing WMO " + model + ": " + e.Message);
                    return false;
                }

                return true;
            }
        }

        public static void Save(string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(boundingBoxBlobDict.OrderBy(x => x.Key).ToDictionary(), Formatting.Indented));
        }
    }
}
