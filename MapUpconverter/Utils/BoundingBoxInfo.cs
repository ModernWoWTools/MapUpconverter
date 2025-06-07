using Newtonsoft.Json;
using System.Numerics;

namespace MapUpconverter.Utils
{
    public static class BoundingBoxInfo
    {
        public static Dictionary<string, Warcraft.NET.Files.Structures.BoundingBox> boundingBoxBlobDict = [];
        public static Dictionary<string, Warcraft.NET.Files.Structures.BoundingBox> customBoundingBoxBlobDict = [];

        public static void Initialize(string configFolder)
        {
            var bbBlob = JsonConvert.DeserializeObject<Dictionary<string, JSONCAaBox>>(File.ReadAllText(Path.Combine(configFolder, "blob.json"))) ?? throw new Exception("Failed to parse bounding box info JSON");

            // Because the french converter stored the bounding box in a different format, we need to convert it to the format we use.
            foreach (var bb in bbBlob)
            {
                boundingBoxBlobDict.Add(bb.Key, new()
                {
                    Minimum = bb.Value.BottomCorner,
                    Maximum = bb.Value.TopCorner
                });
            }

            if (File.Exists(Path.Combine(configFolder, "custom-blob.json")))
            {
                var customBlob = JsonConvert.DeserializeObject<Dictionary<string, JSONCAaBox>>(File.ReadAllText(Path.Combine(configFolder, "custom-blob.json"))) ?? throw new Exception("Failed to parse custom bounding box info JSON");

                foreach (var bb in customBlob)
                {
                    boundingBoxBlobDict[bb.Key] = new()
                    {
                        Minimum = bb.Value.BottomCorner,
                        Maximum = bb.Value.TopCorner
                    };

                    customBoundingBoxBlobDict[bb.Key] = new()
                    {
                        Minimum = bb.Value.BottomCorner,
                        Maximum = bb.Value.TopCorner
                    };
                }
            }
        }

        public async static Task UpdateBoundingBoxesForPatch(string toolFolder)
        {
            if (boundingBoxBlobDict.Count == 0)
                Initialize(Path.Combine(toolFolder, "meta"));

            if (Listfile.NameMap.Count == 0)
                Listfile.Initialize(toolFolder);

            var baseDir = ExportHelper.GetExportDirectory();

            var badWMOExtensions = new List<string>();
            for (int i = 0; i < 999; i++)
                badWMOExtensions.Add($"_{i:D3}.wmo");

            foreach (var wmo in Directory.GetFiles(baseDir, "*.wmo", SearchOption.AllDirectories))
            {
                // Skip group WMOs
                if (badWMOExtensions.Any(x => wmo.EndsWith(x, StringComparison.OrdinalIgnoreCase)))
                    continue;

                var bytes = await File.ReadAllBytesAsync(wmo);
                var boundingBox = ProcessWMO(bytes);
                var listfileName = wmo.Replace(baseDir, "").Replace('\\', '/').TrimStart('/').ToLowerInvariant();

                if (!Listfile.ReverseMap.TryGetValue(listfileName, out var fdid))
                    throw new Exception("Failed to find WMO " + listfileName + " in listfile, cannot continue. Make sure you've run patch generation at least once.");

                customBoundingBoxBlobDict[fdid.ToString()] = new()
                {
                    Minimum = boundingBox.BottomCorner,
                    Maximum = boundingBox.TopCorner
                };
            }

            foreach (var m2 in Directory.GetFiles(baseDir, "*.m2", SearchOption.AllDirectories))
            {
                var bytes = await File.ReadAllBytesAsync(m2);
                var boundingBox = ProcessM2(bytes);
                var listfileName = m2.Replace(baseDir, "").Replace('\\', '/').TrimStart('/').ToLowerInvariant();

                if (!Listfile.ReverseMap.TryGetValue(listfileName, out var fdid))
                    throw new Exception("Failed to find M2 " + listfileName + " in listfile, cannot continue. Make sure you've run patch generation at least once.");

                customBoundingBoxBlobDict[fdid.ToString()] = new()
                {
                    Minimum = boundingBox.BottomCorner,
                    Maximum = boundingBox.TopCorner
                };
            }

            WriteCustomBoundingBoxBlob(toolFolder);
        }

        private static void WriteCustomBoundingBoxBlob(string toolFolder)
        {
            var tempDict = new Dictionary<string, JSONCAaBox>(customBoundingBoxBlobDict.Count);
            foreach (var kvp in customBoundingBoxBlobDict)
            {
                tempDict[kvp.Key] = new JSONCAaBox(kvp.Value.Minimum, kvp.Value.Maximum);
            }

            var json = JsonConvert.SerializeObject(tempDict, Formatting.Indented);
            File.WriteAllText(Path.Combine(toolFolder, "meta", "custom-blob.json"), json);
        }

        public static JSONCAaBox ProcessM2(byte[] bytes)
        {
            var rawBoundingBox = new JSONCAaBox();

            using (var ms = new MemoryStream(bytes))
            using (var bin = new BinaryReader(ms))
            {
                var md21Magic = new string(bin.ReadChars(4));
                if (md21Magic != "MD21")
                    throw new Exception("Invalid M2 file, expected MD21 magic, got " + md21Magic);
                var md21Size = bin.ReadUInt32();

                var md20Magfc = new string(bin.ReadChars(4));
                if (md20Magfc != "MD20")
                    throw new Exception("Invalid M2 file, expected MD20 magic, got " + md20Magfc);

                bin.BaseStream.Seek(168, SeekOrigin.Begin); // Skip to bounding box

                rawBoundingBox.BottomCorner = new Vector3
                {
                    X = bin.ReadSingle(),
                    Y = bin.ReadSingle(),
                    Z = bin.ReadSingle()
                };

                rawBoundingBox.TopCorner = new Vector3
                {
                    X = bin.ReadSingle(),
                    Y = bin.ReadSingle(),
                    Z = bin.ReadSingle()
                };
            }

            return rawBoundingBox;
        }

        public static JSONCAaBox ProcessWMO(byte[] bytes)
        {
            var wmo = new Warcraft.NET.Files.WMO.WorldMapObject.BfA.WorldMapObjectRoot(bytes);

            var boundingBox = new JSONCAaBox
            {
                BottomCorner = new Vector3
                {
                    X = wmo.Header.BoundingBox.Minimum.X,
                    Y = wmo.Header.BoundingBox.Minimum.Y,
                    Z = wmo.Header.BoundingBox.Minimum.Z
                },

                TopCorner = new Vector3
                {
                    X = wmo.Header.BoundingBox.Maximum.X,
                    Y = wmo.Header.BoundingBox.Maximum.Y,
                    Z = wmo.Header.BoundingBox.Maximum.Z
                }
            };

            return boundingBox;
        }

        public struct JSONCAaBox(Vector3 inBottomCorner, Vector3 inTopCorner)
        {
            public Vector3 BottomCorner = inBottomCorner;
            public Vector3 TopCorner = inTopCorner;
        }
    }
}
