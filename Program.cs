using MapUpconverter.Utils;

namespace MapUpconverter
{
    internal class Program
    {
        private static readonly Dictionary<string, Warcraft.NET.Files.ADT.Terrain.BfA.Terrain> cachedRootADTs = [];
        private static readonly Dictionary<string, Warcraft.NET.Files.ADT.TerrainObject.One.TerrainObjectOne> cachedOBJ1ADTs = [];

        static void Main(string[] args)
        {
            string toolFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()?.Location) ?? "";

            // Load settings from settings.json
            try
            {
                Settings.Load(toolFolder);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to load settings: " + e.Message);
                Console.WriteLine("Press enter to exit");
                Console.ReadLine();
                return;
            }

            if (!Directory.Exists(Settings.InputDir))
            {
                Console.WriteLine("Input directory " + Settings.InputDir + " does not exist");
                Console.WriteLine("Press enter to exit");
                Console.ReadLine();
                return;
            }

            long totalTimeMS = 0;
            var timer = new System.Diagnostics.Stopwatch();
            timer.Start();

            // Load listfile from listfile.csv
            // TODO: Auto-download? Expiration warnings?
            try
            {
                Console.Write("Loading listfile..");
                Listfile.Initialize(toolFolder);
                Console.WriteLine("..done in " + timer.ElapsedMilliseconds + "ms");
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to load listfile: " + e.Message);
                Console.WriteLine("Press enter to exit");
                Console.ReadLine();
                return;
            }

            totalTimeMS += timer.ElapsedMilliseconds;
            timer.Restart();

            // Load texture height/scale information
            // TODO: Updating?
            try
            {
                Console.Write("Loading height info..");
                HeightInfo.Initialize(Path.Combine(toolFolder, "TextureInfoByFilePath.json"));
                Console.WriteLine("..done in " + timer.ElapsedMilliseconds + "ms");
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to load height info: " + e.Message);
                Console.WriteLine("Press enter to exit");
                Console.ReadLine();
                return;
            }

            totalTimeMS += timer.ElapsedMilliseconds;
            timer.Restart();

            // Load texture height/scale information
            // TODO: Updating?
            try
            {
                Console.Write("Loading bounding box info..");
                BoundingBoxInfo.Initialize(Path.Combine(toolFolder, "blob.json"));
                Console.WriteLine("..done in " + timer.ElapsedMilliseconds + "ms");
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to load bounding box info: " + e.Message);
                Console.WriteLine("Press enter to exit");
                Console.ReadLine();
                return;
            }

            totalTimeMS += timer.ElapsedMilliseconds;
            timer.Stop();

#if DEBUG
            Warcraft.NET.Settings.throwOnMissingChunk = false;
#endif
            if (!Directory.Exists(Settings.OutputDir))
            {
                Console.WriteLine("Output directory " + Settings.OutputDir + " does not exist, creating..");
                Directory.CreateDirectory(Settings.OutputDir);
            }

            Console.WriteLine("Startup took " + totalTimeMS + "ms");

            if (!Settings.ConvertOnSave)
            {
                Console.WriteLine("On-save mode is disabled, starting one-time map conversion..");
                ConvertMap();
                return;
            }
            else
            {
                Console.WriteLine("On-save mode is enabled, watching for changes in " + Settings.InputDir + "..");
                // Spawn watcher
                // TODO: Handle batch saves, hand to changedADTs in one go
            }
        }

        private static void ConvertMap()
        {
            var timer = new System.Diagnostics.Stopwatch();
            var totalTimeMS = 0L;
            timer.Start();

            var adts = Directory.GetFiles(Settings.InputDir, "*.adt");

            Console.Write("Converting " + adts.Length + " adts..");
            Parallel.ForEach(adts, ConvertWotLKADT);
            Console.WriteLine("..done in " + timer.ElapsedMilliseconds + "ms");
            totalTimeMS += timer.ElapsedMilliseconds;

            timer.Restart();

            Console.Write("Generating WDL from converted ADTs..");
            ConvertWDL();
            Console.WriteLine("..done in " + timer.ElapsedMilliseconds + "ms");
            totalTimeMS += timer.ElapsedMilliseconds;

            Console.WriteLine("Conversion took " + totalTimeMS + "ms");
        }

        private static void ConvertWotLKADT(string inputADT)
        {
            var wotlkADT = new Warcraft.NET.Files.ADT.Terrain.Wotlk.Terrain(File.ReadAllBytes(inputADT));

            var root = ADT.Root.Convert(wotlkADT);
            var tex0 = ADT.Tex0.Convert(wotlkADT);
            var obj0 = ADT.Obj0.Convert(wotlkADT);
            var obj1 = ADT.Obj1.Convert(wotlkADT, obj0);

            File.WriteAllBytes(Path.Combine(Settings.OutputDir, Path.GetFileName(inputADT)), root.Serialize());
            File.WriteAllBytes(Path.Combine(Settings.OutputDir, Path.GetFileNameWithoutExtension(inputADT) + "_tex0.adt"), tex0.Serialize());
            File.WriteAllBytes(Path.Combine(Settings.OutputDir, Path.GetFileNameWithoutExtension(inputADT) + "_obj0.adt"), obj0.Serialize());
            File.WriteAllBytes(Path.Combine(Settings.OutputDir, Path.GetFileNameWithoutExtension(inputADT) + "_obj1.adt"), obj1.Serialize());

            cachedRootADTs[Path.GetFileNameWithoutExtension(inputADT)] = root;
            cachedOBJ1ADTs[Path.GetFileNameWithoutExtension(inputADT) + "_obj1"] = obj1;
        }

        private static void ConvertWDL()
        {
            var wdl = WDL.WDL.Generate(Settings.OutputDir, cachedRootADTs, cachedOBJ1ADTs);
            File.WriteAllBytes(Path.Combine(Settings.OutputDir, Settings.MapName + ".wdl"), wdl.Serialize());
        }
    }
}