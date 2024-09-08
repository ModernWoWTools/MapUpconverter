using Newtonsoft.Json;
using System.Numerics;

namespace MapUpconverter.Utils
{
    public static class BoundingBoxInfo
    {
        public static Dictionary<string, Warcraft.NET.Files.Structures.BoundingBox> boundingBoxBlobDict = [];

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

            if(File.Exists(Path.Combine(configFolder, "custom-blob.json")))
            {
                var customBlob = JsonConvert.DeserializeObject<Dictionary<string, JSONCAaBox>>(File.ReadAllText(Path.Combine(configFolder, "custom-blob.json"))) ?? throw new Exception("Failed to parse custom bounding box info JSON");

                foreach (var bb in customBlob)
                {
                    boundingBoxBlobDict[bb.Key] = new()
                    {
                        Minimum = bb.Value.BottomCorner,
                        Maximum = bb.Value.TopCorner
                    };
                }
            }
        }

        public static JSONCAaBox ProcessM2(byte[] bytes)
        {
            var m2 = new Warcraft.NET.Files.M2.Model(bytes);

            var boundingBox = new JSONCAaBox
            {
                BottomCorner = new Vector3
                {
                    X = m2.ModelInformation.BoundingBox.Minimum.X,
                    Y = m2.ModelInformation.BoundingBox.Minimum.Y,
                    Z = m2.ModelInformation.BoundingBox.Minimum.Z
                },

                TopCorner = new Vector3
                {
                    X = m2.ModelInformation.BoundingBox.Maximum.X,
                    Y = m2.ModelInformation.BoundingBox.Maximum.Y,
                    Z = m2.ModelInformation.BoundingBox.Maximum.Z
                }
            };

            return boundingBox;
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
