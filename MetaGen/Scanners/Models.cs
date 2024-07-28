using MetaGen.Services;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace MetaGen.Scanners
{
    public static class Models
    {
        private static ConcurrentDictionary<uint, CAaBox> boundingBoxBlobDict = [];

        public static void LoadCurrent(string metaFolder)
        {
            if (!File.Exists(Path.Combine(metaFolder, "blob.json")))
                return;

            var blobPath = Path.Combine(metaFolder, "blob.json");
            boundingBoxBlobDict = JsonConvert.DeserializeObject<ConcurrentDictionary<uint, CAaBox>>(File.ReadAllText(blobPath)) ?? throw new Exception("Failed to read blob.json");
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
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error parsing M2 " + model + ": " + e.Message);
                    return false;
                }

                ms.Position = 0;

                try
                {
                    var m2 = new Warcraft.NET.Files.M2.Model(ms.ToArray());

                    var boundingBox = new CAaBox
                    {
                        BottomCorner = new C3Vector<float>
                        {
                            x = m2.ModelInformation.BoundingBox.Minimum.X,
                            y = m2.ModelInformation.BoundingBox.Minimum.Y,
                            z = m2.ModelInformation.BoundingBox.Minimum.Z
                        },

                        TopCorner = new C3Vector<float>
                        {
                            x = m2.ModelInformation.BoundingBox.Maximum.X,
                            y = m2.ModelInformation.BoundingBox.Maximum.Y,
                            z = m2.ModelInformation.BoundingBox.Maximum.Z
                        }
                    };

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
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error parsing WMO " + model + ": " + e.Message);
                    return false;
                }

                ms.Position = 0;

                try
                {
                    var wmo = new Warcraft.NET.Files.WMO.WorldMapObject.BfA.WorldMapObjectRoot(ms.ToArray());

                    var boundingBox = new CAaBox
                    {
                        BottomCorner = new C3Vector<float>
                        {
                            x = wmo.Header.BoundingBox.Minimum.X,
                            y = wmo.Header.BoundingBox.Minimum.Y,
                            z = wmo.Header.BoundingBox.Minimum.Z
                        },

                        TopCorner = new C3Vector<float>
                        {
                            x = wmo.Header.BoundingBox.Maximum.X,
                            y = wmo.Header.BoundingBox.Maximum.Y,
                            z = wmo.Header.BoundingBox.Maximum.Z
                        }
                    };

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

        public struct CAaBox
        {
            public CAaBox(C3Vector<float> inBottomCorner, C3Vector<float> inTopCorner)
            {
                BottomCorner = inBottomCorner;
                TopCorner = inTopCorner;
            }

            public C3Vector<float> BottomCorner;

            public C3Vector<float> TopCorner;
        }

        public struct C3Vector<T>
        {
            public T x { readonly get; set; }
            public T y { readonly get; set; }
            public T z { readonly get; set; }
        }
    }
}
