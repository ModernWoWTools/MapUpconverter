using MapUpconverter.Utils;
using Warcraft.NET;

namespace MapUpconverter
{
    internal class Program
    {
        static void Main(string[] args)
        {
            long totalTimesMS = 0;
            var timer = new System.Diagnostics.Stopwatch();
            timer.Start();

            Console.Write("Loading listfile..");
            Listfile.Initialize(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "listfile.csv"));
            Console.WriteLine("..done in " + timer.ElapsedMilliseconds + "ms");
            totalTimesMS += timer.ElapsedMilliseconds;
            timer.Restart();

            Console.Write("Loading height info..");
            HeightInfo.Initialize(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "TextureInfoByFilePath.json"));
            Console.WriteLine("..done in " + timer.ElapsedMilliseconds + "ms");
            totalTimesMS += timer.ElapsedMilliseconds;
            timer.Restart();

            Console.Write("Loading bounding box info..");
            BoundingBoxInfo.Initialize(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "blob.json"));
            Console.WriteLine("..done in " + timer.ElapsedMilliseconds + "ms");
            totalTimesMS += timer.ElapsedMilliseconds;
            timer.Restart();

            Warcraft.NET.Settings.logLevel = LogLevel.None;
            Warcraft.NET.Settings.throwOnMissingChunk = false;

            var inputDir = "G:\\WinterWonderland\\world\\maps\\winterwonderland"; 
            var outputDir = "output";

            if(!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            var adts = Directory.GetFiles(inputDir, "*.adt");
            Console.Write("Converting " + adts.Length + " adts..");
            Parallel.ForEach(adts, adt =>
            {
                var wotlkADT = new Warcraft.NET.Files.ADT.Terrain.Wotlk.Terrain(File.ReadAllBytes(adt));

                var root = ADT.Root.Convert(wotlkADT);
                var tex0 = ADT.Tex0.Convert(wotlkADT);
                var obj0 = ADT.Obj0.Convert(wotlkADT);

                var obj1 = ADT.Obj1.Convert(wotlkADT, obj0);

                File.WriteAllBytes(Path.Combine(outputDir, Path.GetFileName(adt)), root.Serialize());
                File.WriteAllBytes(Path.Combine(outputDir, Path.GetFileNameWithoutExtension(adt) + "_tex0.adt"), tex0.Serialize());
                File.WriteAllBytes(Path.Combine(outputDir, Path.GetFileNameWithoutExtension(adt) + "_obj0.adt"), obj0.Serialize());
                File.WriteAllBytes(Path.Combine(outputDir, Path.GetFileNameWithoutExtension(adt) + "_obj1.adt"), obj1.Serialize());

            });

            Console.WriteLine("..done in " + timer.ElapsedMilliseconds + "ms");
            totalTimesMS += timer.ElapsedMilliseconds;
            Console.WriteLine("Took " + totalTimesMS + "ms in total.");
        }
    }
}