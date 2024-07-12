using Newtonsoft.Json;
using System.Numerics;

namespace MapUpconverter.Utils
{
    public static class BoundingBoxInfo
    {
        public static Dictionary<string, Warcraft.NET.Files.Structures.BoundingBox> boundingBoxBlobDict = [];

        public static void Initialize(string configPath)
        {
            var frenchBlob = JsonConvert.DeserializeObject<Dictionary<string, JSONCAaBox>>(File.ReadAllText(configPath)) ?? throw new Exception("Failed to parse bounding box info JSON");

            // Because the french converter stored the bounding box in a different format, we need to convert it to the format we use.
            foreach (var frenchBoundingBox in frenchBlob)
            {
                boundingBoxBlobDict.Add(frenchBoundingBox.Key, new()
                {
                    Minimum = frenchBoundingBox.Value.BottomCorner,
                    Maximum = frenchBoundingBox.Value.TopCorner
                });
            }
        }

        // Past me to future me: don't leak this outside of this class, it makes things confusing.
        private struct JSONCAaBox(Vector3 inBottomCorner, Vector3 inTopCorner)
        {
            public Vector3 BottomCorner = inBottomCorner;
            public Vector3 TopCorner = inTopCorner;
        }
    }
}
