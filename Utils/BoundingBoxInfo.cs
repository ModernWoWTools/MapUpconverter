using Newtonsoft.Json;
using System.Numerics;

namespace MapUpconverter.Utils
{
    public static class BoundingBoxInfo
    {
        public static Dictionary<string, Warcraft.NET.Files.Structures.BoundingBox> boundingBoxBlobDict = new();

        public static void Initialize(string configPath)
        {
            var frenchBlob = JsonConvert.DeserializeObject<Dictionary<string, CAaBox>>(File.ReadAllText(configPath));
            foreach (var frenchBoundingBox in frenchBlob)
            {
                boundingBoxBlobDict.Add(frenchBoundingBox.Key, new()
                {
                    Minimum = frenchBoundingBox.Value.BottomCorner,
                    Maximum = frenchBoundingBox.Value.TopCorner
                });
            }
        }
    }

    public struct CAaBox
    {
        public CAaBox(Vector3 inBottomCorner, Vector3 inTopCorner)
        {
            BottomCorner = inBottomCorner;
            TopCorner = inTopCorner;
        }

        public Vector3 BottomCorner;
        public Vector3 TopCorner;
    }
}
