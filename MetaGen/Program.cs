using MetaGen.Scanners;
using MetaGen.Services;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using static MapUpconverter.Utils.BoundingBoxInfo;

namespace MetaGen
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 1 && (args[0].ToLower().EndsWith(".m2") || args[0].ToLower().EndsWith(".wmo") || Directory.Exists(args[0])))
            {
                if (args[0].ToLower().EndsWith(".m2"))
                    Console.WriteLine(JsonConvert.SerializeObject(ProcessM2(File.ReadAllBytes(args[0])), Formatting.Indented));
                else if (args[0].ToLower().EndsWith(".wmo"))
                    Console.WriteLine(JsonConvert.SerializeObject(ProcessWMO(File.ReadAllBytes(args[0])), Formatting.Indented));
                else if (Directory.Exists(args[0]))
                {
                    var boundingBoxBlobDict = new Dictionary<string, JSONCAaBox>();

                    foreach (var file in Directory.GetFiles(args[0], "*.m2"))
                        boundingBoxBlobDict.Add(Path.GetFileName(file), ProcessM2(File.ReadAllBytes(file)));

                    foreach (var file in Directory.GetFiles(args[0], "*.wmo"))
                        boundingBoxBlobDict.Add(Path.GetFileName(file), ProcessWMO(File.ReadAllBytes(file)));

                    Console.WriteLine(JsonConvert.SerializeObject(boundingBoxBlobDict, Formatting.Indented));
                }

                Console.ReadLine();
                return;
            }

            Console.WriteLine("MetaGen - A tool meant to extract meta information from WoW files.");

            if (args.Length < 2)
            {
                Console.WriteLine("Usage (single mode): MetaGen <.m2 or .wmo file>");
                Console.WriteLine("Usage (full run mode, requires additional libraries compiled separately): MetaGen <meta folder> <CASC product> (WoW folder or CDN host)");
                Console.WriteLine("Press enter to exit.");
                Console.ReadLine();
                return;
            }

            var metaFolder = Path.Combine(args[0]);

            if (!Directory.Exists(metaFolder))
                throw new Exception("Given meta folder does not exist");

            // Listfile parsing
            if (!File.Exists(Path.Combine(metaFolder, "listfile.csv")))
                throw new Exception("Listfile not found at " + Path.Combine(metaFolder, "listfile.csv"));

            Console.Write("Parsing listfile...");
            Listfile.Initialize(metaFolder);
            Console.WriteLine("done.");

            // CASCLib setup
            var cascProduct = args[1];
            var wowFolder = args.Length > 2 ? args[2] : null;

            if (File.Exists("fakebuildconfighash"))
                File.Delete("fakebuildconfighash");

            if (File.Exists("fakecdnconfighash"))
                File.Delete("fakecdnconfighash");

            var buildConfig = args.Length > 3 ? args[3] : null;
            if (buildConfig != null)
            {
                Console.WriteLine("Using custom build config hash " + buildConfig);
                File.WriteAllText("fakebuildconfighash", buildConfig);
            }

            var cdnConfig = args.Length > 4 ? args[4] : null;
            if (cdnConfig != null)
            {
                Console.WriteLine("Using custom cdn config hash " + cdnConfig);
                File.WriteAllText("fakecdnconfighash", cdnConfig);
            }

            CASC.Initialize(cascProduct, wowFolder);

            // ADT dumping
            Console.Write("Generating list of tex0 ADTs to process...");
            var tex0ADTList = Listfile.NameMap.Where(kv => kv.Value.EndsWith("_tex0.adt")).Select(x => x.Key).ToList();
            Console.WriteLine("done, found " + tex0ADTList.Count + " ADTs.");

#if DEBUG
            Warcraft.NET.Settings.throwOnMissingChunk = false;
#endif
            Console.WriteLine("Loading existing height info...");
            ADT.LoadCurrent(metaFolder);

            var timer = new System.Diagnostics.Stopwatch();
            timer.Start();
            Console.Write("Starting ADT scanning...");
            Parallel.ForEach(tex0ADTList, adt =>
            {
                ADT.ProcessADT(adt);
            });
            timer.Stop();
            Console.WriteLine("done, took " + (timer.ElapsedMilliseconds / 1000) + " seconds.");

            var textureInfoByFDIDPath = Path.Combine(metaFolder, "TextureInfoByFileId.json");
            Console.WriteLine("Saving height info by file ID to " + textureInfoByFDIDPath);
            ADT.SaveTextureInfoByFileID(textureInfoByFDIDPath);

            var textureInfoByFilePath = Path.Combine(metaFolder, "TextureInfoByFilePath.json");
            Console.WriteLine("Saving height info by file path to " + textureInfoByFilePath);
            ADT.SaveTextureInfoByFilePath(textureInfoByFilePath);

            ADT.RemoveDupes();

            var groundEffectIDsByTextureFileIDPath = Path.Combine(metaFolder, "GroundEffectIDsByTextureFileID.json");
            Console.WriteLine("Saving ground effect IDs by texture file ID to " + groundEffectIDsByTextureFileIDPath);
            ADT.SaveGroundEffectIDsByTextureFileID(groundEffectIDsByTextureFileIDPath);

            var groundEffectIDsByTextureFilePath = Path.Combine(metaFolder, "GroundEffectIDsByTextureFilePath.json");
            Console.WriteLine("Saving ground effect IDs by texture file path to " + groundEffectIDsByTextureFilePath);
            ADT.SaveGroundEffectIDsByTextureFilePath(groundEffectIDsByTextureFilePath);

            // WMO bounding boxes
            Console.Write("Generating list of WMOs to process...");
            var wmoList = Listfile.NameMap.Where(kv => kv.Value.EndsWith(".wmo")).ToDictionary();
            var filteredList = new List<uint>();
            foreach (var wmo in wmoList)
            {
                if (Regex.IsMatch(wmo.Value, @"_\d{3}.wmo") || Regex.IsMatch(wmo.Value, @"_lod\d{1}.wmo"))
                    continue;

                filteredList.Add(wmo.Key);
            }
            Console.WriteLine("done, found " + filteredList.Count + " WMOs.");

            timer.Restart();
            Console.Write("Starting WMO scanning...");
            Parallel.ForEach(filteredList, wmo =>
            {
                Models.ProcessWMO(wmo);
            });

            timer.Stop();
            Console.WriteLine("done, took " + (timer.ElapsedMilliseconds / 1000) + " seconds.");


            // M2 bounding boxes
            Console.Write("Generating list of M2s to process...");
            var m2List = Listfile.NameMap.Where(kv => kv.Value.EndsWith(".m2")).ToDictionary();
            Console.WriteLine("done, found " + m2List.Count + " M2s.");

            timer.Restart();
            Console.Write("Starting M2 scanning...");
            Models.LoadCurrent(metaFolder);
            Parallel.ForEach(m2List, m2 =>
            {
                Models.ProcessM2(m2.Key);
            });
            timer.Stop();
            Console.WriteLine("done, took " + (timer.ElapsedMilliseconds / 1000) + " seconds.");

            // save to blob.json
            var blobPath = Path.Combine(metaFolder, "blob.json");
            Console.WriteLine("Saving model blob info to " + blobPath);
            Models.Save(blobPath);
        }
    }
}
