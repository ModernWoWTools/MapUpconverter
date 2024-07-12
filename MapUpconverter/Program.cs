using MapUpconverter.Utils;
using System.Collections.Concurrent;

namespace MapUpconverter
{
    internal class Program
    {
        private static readonly ConcurrentDictionary<string, Warcraft.NET.Files.ADT.Terrain.BfA.Terrain> cachedRootADTs = [];
        private static readonly ConcurrentDictionary<string, Warcraft.NET.Files.ADT.TerrainObject.One.TerrainObjectOne> cachedOBJ1ADTs = [];

        private static BlockingCollection<string> adtQueue = [];

        static void Main(string[] args)
        {
            string toolFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()?.Location) ?? "";

            Console.WriteLine("Starting MapUpconverter v" + Version.SharedVersion + "..");

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

            if (!string.IsNullOrEmpty(Settings.EpsilonDir))
            {
                try
                {
                    Console.Write("Generating temp listfile and copying to Epsilon..");
                    var epsilonExtBlacklist = new string[] { ".unk", ".pd4", ".pm4", ".meta", ".dat", ".col" };
                    var epsilonListfilePath = Path.Combine(Settings.EpsilonDir, "_retail_", "Tools", "listfile.csv");
                    File.Delete(epsilonListfilePath);
                    File.WriteAllLines(epsilonListfilePath, Listfile.NameMap.Where(x => !epsilonExtBlacklist.Contains(Path.GetExtension(x.Value))).Select(x => x.Key + ";" + x.Value));
                    Console.WriteLine("..done in " + timer.ElapsedMilliseconds + "ms");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to copy listfile to Epsilon, launcher might not accept our patches: " + e.Message);
                }

                totalTimeMS += timer.ElapsedMilliseconds;
                timer.Restart();

                try
                {
                    Console.Write("Checking existing Epsilon patches for used FileDataIDs..");
                    Epsilon.PatchManifest.ScanUsedFileDataIDs();
                    Console.WriteLine("..done in " + timer.ElapsedMilliseconds + "ms");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to scan Epsilon patches for used FileDataIDs: " + e.Message);
                }
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

            var mapDir = Path.Combine(Settings.OutputDir, "world", "maps", Settings.MapName);
            if (!Directory.Exists(mapDir))
            {
                Console.WriteLine("Map directory " + mapDir + " does not exist, creating..");
                Directory.CreateDirectory(mapDir);
            }

            Console.WriteLine("Startup took " + totalTimeMS + "ms");

            if (!Settings.ConvertOnSave)
            {
                Console.WriteLine("On-save mode is disabled, starting one-time map conversion..");
                try
                {
                    ConvertMap();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to convert map: " + e.Message);
                }
                return;
            }
            else
            {
                Console.WriteLine("On-save mode is enabled, watching for changes in " + Settings.InputDir + "..");
                var monitorDirWatcher = new FileSystemWatcher();
                monitorDirWatcher.Path = Settings.InputDir;
                monitorDirWatcher.NotifyFilter = NotifyFilters.LastWrite;
                monitorDirWatcher.Filter = "*.adt";
                monitorDirWatcher.Changed += new FileSystemEventHandler(OnADTChanged);
                monitorDirWatcher.EnableRaisingEvents = true;
                monitorDirWatcher.IncludeSubdirectories = true;

                Task.Run(() =>
                {
                    while (!adtQueue.IsCompleted)
                    {
                        string adtFilename = "";

                        try
                        {
                            adtFilename = adtQueue.Take();
                        }
                        catch (InvalidOperationException) { }

                        if (!string.IsNullOrEmpty(adtFilename))
                        {
                            var timer = new System.Diagnostics.Stopwatch();
                            try
                            {
                                timer.Start();
                                ConvertWotLKADT(adtFilename);
                                timer.Stop();
                                Console.WriteLine("Converting ADT " + Path.GetFileNameWithoutExtension(adtFilename) + " took " + timer.ElapsedMilliseconds + "ms");
                            }
                            catch (IOException ioE)
                            {
                                if (ioE.Message.Contains("because it is being used by another process"))
                                {
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    Console.WriteLine("Requeuing ADT " + Path.GetFileNameWithoutExtension(adtFilename) + " because it was still being used..");
                                    Console.ResetColor();
                                }
                                else
                                {
                                    Console.WriteLine("Failed to convert ADT " + Path.GetFileNameWithoutExtension(adtFilename) + ": " + ioE.Message);
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Failed to convert ADT " + Path.GetFileNameWithoutExtension(adtFilename) + ": " + e.Message);
                            }

                            try
                            {
                                timer.Restart();
                                ConvertWDL();
                                timer.Stop();
                                Console.WriteLine("Generating WDL took " + timer.ElapsedMilliseconds + "ms");
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Failed to generate WDL: " + e.Message);
                            }

                            if (!string.IsNullOrEmpty(Settings.EpsilonDir))
                            {
                                try
                                {
                                    timer.Restart();
                                    Epsilon.PatchManifest.Update();
                                    timer.Stop();
                                    Console.WriteLine("Updating Epsilon patch manifest took " + timer.ElapsedMilliseconds + "ms");
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("Failed to update Epsilon patch manifest: " + e.Message);
                                }
                            }
                        }
                    }
                });

                Console.WriteLine("Press enter to stop watching and exit");
                Console.ReadLine();
            }
        }

        private static void OnADTChanged(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine(e.ChangeType);
            if(!e.FullPath.StartsWith(Path.Combine(Settings.InputDir, "world", "maps", Settings.MapName)))
                Console.WriteLine("Ignoring ADT " + e.FullPath + " because it's not in the map directory " + Path.Combine(Settings.InputDir, "world", "maps", Settings.MapName));
            
            if (!adtQueue.Contains(e.FullPath))
                adtQueue.Add(e.FullPath);
        }

        private static void ConvertMap()
        {
            var timer = new System.Diagnostics.Stopwatch();
            var totalTimeMS = 0L;
            timer.Start();

            var adts = Directory.GetFiles(Settings.InputDir, "*.adt", SearchOption.AllDirectories);

            Console.Write("Converting " + adts.Length + " adts..");
            //#if !DEBUG
            Parallel.ForEach(adts, ConvertWotLKADT);
            //#elif DEBUG
            //            foreach (var adt in adts)
            //            {
            //                ConvertWotLKADT(adt);
            //            }
            //#endif
            Console.WriteLine("..done in " + timer.ElapsedMilliseconds + "ms");

            totalTimeMS += timer.ElapsedMilliseconds;
            timer.Restart();

            Console.Write("Generating WDL from converted ADTs..");
            ConvertWDL();
            Console.WriteLine("..done in " + timer.ElapsedMilliseconds + "ms");

            totalTimeMS += timer.ElapsedMilliseconds;
            timer.Restart();

            Console.Write("Converting WDT..");
            ConvertWDT();
            Console.WriteLine("..done in " + timer.ElapsedMilliseconds + "ms");

            totalTimeMS += timer.ElapsedMilliseconds;
            timer.Restart();

            Console.Write("Converting minimaps..");
            ConvertMinimaps();
            Console.WriteLine("..done in " + timer.ElapsedMilliseconds + "ms");

            totalTimeMS += timer.ElapsedMilliseconds;
            timer.Restart();

            if (!string.IsNullOrEmpty(Settings.EpsilonDir))
                Epsilon.PatchManifest.Update();

            Console.WriteLine("Conversion took " + totalTimeMS + "ms");
            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }

        private static void ConvertWotLKADT(string inputADT)
        {
            try
            {
                var wotlkADT = new Warcraft.NET.Files.ADT.Terrain.Wotlk.Terrain(File.ReadAllBytes(inputADT));

                var root = ADT.Root.Convert(wotlkADT);
                var tex0 = ADT.Tex0.Convert(wotlkADT);
                var obj0 = ADT.Obj0.Convert(wotlkADT);
                var obj1 = ADT.Obj1.Convert(wotlkADT, obj0);

                File.WriteAllBytes(Path.Combine(Settings.OutputDir, "world", "maps", Settings.MapName, Path.GetFileName(inputADT)), root.Serialize());
                File.WriteAllBytes(Path.Combine(Settings.OutputDir, "world", "maps", Settings.MapName, Path.GetFileNameWithoutExtension(inputADT) + "_tex0.adt"), tex0.Serialize());
                File.WriteAllBytes(Path.Combine(Settings.OutputDir, "world", "maps", Settings.MapName, Path.GetFileNameWithoutExtension(inputADT) + "_obj0.adt"), obj0.Serialize());
                File.WriteAllBytes(Path.Combine(Settings.OutputDir, "world", "maps", Settings.MapName, Path.GetFileNameWithoutExtension(inputADT) + "_obj1.adt"), obj1.Serialize());

                cachedRootADTs[Path.GetFileNameWithoutExtension(inputADT)] = root;
                cachedOBJ1ADTs[Path.GetFileNameWithoutExtension(inputADT) + "_obj1"] = obj1;
            }catch(Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed to convert " + inputADT + ": " + e.Message + ", press enter to ignore and continue or ctrl-c to exit.");
                Console.ResetColor();
            }
        }

        private static void ConvertWDL()
        {
            var wdl = WDL.WDL.Generate(cachedRootADTs, cachedOBJ1ADTs);
            File.WriteAllBytes(Path.Combine(Settings.OutputDir, "world", "maps", Settings.MapName, Settings.MapName + ".wdl"), wdl.Serialize());
        }

        private static void ConvertWDT()
        {
            var wdt = WDT.RootWDT.Generate();
            File.WriteAllBytes(Path.Combine(Settings.OutputDir, "world", "maps", Settings.MapName, Settings.MapName + ".wdt"), wdt.Serialize());
        }

        private static void ConvertMinimaps()
        {
            Minimaps.Minimaps.Convert();
        }
    }
}