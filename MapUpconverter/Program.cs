using MapUpconverter.Epsilon;
using MapUpconverter.Utils;
using System.Collections.Concurrent;

namespace MapUpconverter
{
    internal class Program
    {
        private static readonly ConcurrentDictionary<string, Warcraft.NET.Files.ADT.Terrain.BfA.Terrain> cachedRootADTs = [];
        private static readonly ConcurrentDictionary<string, Warcraft.NET.Files.ADT.TerrainObject.Zero.TerrainObjectZero> cachedOBJ0ADTs = [];
        private static readonly ConcurrentDictionary<string, Warcraft.NET.Files.ADT.TerrainObject.One.TerrainObjectOne> cachedOBJ1ADTs = [];
        private static readonly ConcurrentDictionary<int, int> UpdatedTiles = [];

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

            Downloads.Initialize(toolFolder);

            // Load listfile from listfile.csv
            if (!File.Exists(Path.Combine(toolFolder, "meta", "listfile.csv")))
            {
                Console.WriteLine("Downloading listfile..");
                Downloads.DownloadListfile(toolFolder).Wait();
            }

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

            if (Settings.ExportTarget == "Epsilon" && !string.IsNullOrEmpty(Settings.EpsilonDir))
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

            if (!Directory.Exists(Path.Combine(toolFolder, "meta")))
                Directory.CreateDirectory(Path.Combine(toolFolder, "meta"));

            // Load texture height/scale information
            if (!File.Exists(Path.Combine(toolFolder, "meta", "TextureInfoByFilePath.json")))
            {
                Console.WriteLine("Downloading height info..");
                Downloads.DownloadHeightTextureInfo(toolFolder).Wait();
            }

            try
            {
                Console.Write("Loading height info..");
                HeightInfo.Initialize(Path.Combine(toolFolder, "meta", "TextureInfoByFilePath.json"));
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

            // Load ground effect texture information
            if (!File.Exists(Path.Combine(toolFolder, "meta", "GroundEffectIDsByTextureFileID.json")))
            {
                Console.WriteLine("Downloading ground effect info..");
                Downloads.DownloadGroundEffectInfo(toolFolder).Wait();
            }

            try
            {
                Console.Write("Loading ground effect info..");
                GroundEffectInfo.Initialize(Path.Combine(toolFolder, "meta", "GroundEffectIDsByTextureFileID.json"));
                Console.WriteLine("..done in " + timer.ElapsedMilliseconds + "ms");
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to load ground effect info: " + e.Message);
                Console.WriteLine("Press enter to exit");
                Console.ReadLine();
                return;
            }

            totalTimeMS += timer.ElapsedMilliseconds;
            timer.Restart();

            // Load texture height/scale information
            if (!File.Exists(Path.Combine(toolFolder, "meta", "blob.json")))
            {
                Console.WriteLine("Downloading model bounding box blob..");
                Downloads.DownloadModelBlob(toolFolder).Wait();
            }

            try
            {
                Console.Write("Loading bounding box info..");
                BoundingBoxInfo.Initialize(Path.Combine(toolFolder, "meta"));
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

            if (!Directory.Exists(ExportHelper.GetExportDirectory()))
            {
                Console.WriteLine("Output directory " + ExportHelper.GetExportDirectory() + " does not exist, creating..");
                Directory.CreateDirectory(ExportHelper.GetExportDirectory());
            }

            var mapDir = Path.Combine(ExportHelper.GetExportDirectory(), "world", "maps", Settings.MapName);
            if (!Directory.Exists(mapDir))
            {
                Console.WriteLine("Map directory " + mapDir + " does not exist, creating..");
                Directory.CreateDirectory(mapDir);
            }

            if (Settings.ClientRefresh)
                EpsilonConnection.Connect();

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
                    Console.WriteLine("Failed to convert map: " + e.Message + "\n" + e.StackTrace);
                    Console.ReadLine();
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
                            Console.WriteLine("Starting on-save conversion of ADT " + Path.GetFileNameWithoutExtension(adtFilename) + "..");
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

                                    adtQueue.Add(adtFilename);
                                    continue;
                                }
                                else
                                {
                                    Console.WriteLine("Failed to convert ADT " + Path.GetFileNameWithoutExtension(adtFilename) + ": " + ioE.Message + "\n" + ioE.StackTrace);
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Failed to convert ADT " + Path.GetFileNameWithoutExtension(adtFilename) + ": " + e.Message + "\n" + e.StackTrace);
                            }

                            if (adtQueue.Count == 0)
                            {
                                Console.WriteLine("No ADTs in queue, updating WDL/Epsilon (if needed)..");
                                if (Settings.GenerateWDTWDL)
                                {
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
                                    // TODO: We don't want to generate WDTs on the fly yet, this will need support in the hot reloading stuff first
                                    try
                                    {
                                        timer.Restart();
                                        ConvertWDT();
                                        timer.Stop();
                                        Console.WriteLine("Generating WDT took " + timer.ElapsedMilliseconds + "ms");
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine("Failed to generate WDT: " + e.Message);
                                    }
                                }

                                if (Settings.ExportTarget == "Epsilon" && !string.IsNullOrEmpty(Settings.EpsilonDir))
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
                                else if (Settings.ExportTarget == "Arctium" && !string.IsNullOrEmpty(Settings.ArctiumDir))
                                {
                                    try
                                    {
                                        timer.Restart();
                                        Arctium.FileMapping.Update();
                                        timer.Stop();
                                        Console.WriteLine("Updating Arctium file mapping took " + timer.ElapsedMilliseconds + "ms");
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine("Failed to update Arctium file mapping: " + e.Message);
                                    }
                                }

                                if (Settings.ClientRefresh && !UpdatedTiles.IsEmpty)
                                {
                                    Console.WriteLine("Requesting map refresh..");
                                    RequestMapUpdate();
                                }
                            }
                        }
                    }
                });

                Console.WriteLine("Press enter to stop watching and exit");
                Console.ReadLine();
            }
        }

        private static void RequestMapUpdate()
        {
            if (!Settings.ClientRefresh)
                return;

            var tiles = new List<(int TileID, int UpdateFlags)>();
            foreach (var tile in UpdatedTiles)
            {
                tiles.Add((tile.Key, tile.Value));
            }

            EpsilonConnection.RequestMapTileUpdate(Settings.MapID, tiles);

            UpdatedTiles.Clear();
        }

        private static void OnADTChanged(object sender, FileSystemEventArgs e)
        {
            var path = Path.Combine(Settings.InputDir, "world", "maps", Settings.MapName);
            if (!e.FullPath.ToLowerInvariant().StartsWith(path.ToLowerInvariant()))
            {
                Console.WriteLine("Ignoring ADT " + e.FullPath + " because it's not in the map directory " + Path.Combine(Settings.InputDir, "world", "maps", Settings.MapName));
                return;
            }

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
//            Parallel.ForEach(adts, ConvertWotLKADT);
//#elif DEBUG
            foreach (var adt in adts)
            {
                ConvertWotLKADT(adt);
            }
//#endif
            Console.WriteLine("..done in " + timer.ElapsedMilliseconds + "ms");

            totalTimeMS += timer.ElapsedMilliseconds;
            timer.Restart();

            if (Settings.GenerateWDTWDL)
            {
                Console.Write("Generating WDL from converted ADTs..");
                ConvertWDL();
                Console.WriteLine("..done in " + timer.ElapsedMilliseconds + "ms");

                totalTimeMS += timer.ElapsedMilliseconds;
                timer.Restart();
            }

            Console.Write("Converting minimaps..");
            ConvertMinimaps();
            Console.WriteLine("..done in " + timer.ElapsedMilliseconds + "ms");

            totalTimeMS += timer.ElapsedMilliseconds;
            timer.Restart();

            if (Settings.GenerateWDTWDL)
            {
                Console.Write("Converting WDT..");
                ConvertWDT();
                Console.WriteLine("..done in " + timer.ElapsedMilliseconds + "ms");

                totalTimeMS += timer.ElapsedMilliseconds;
                timer.Restart();
            }

            if (Settings.ExportTarget == "Epsilon" && !string.IsNullOrEmpty(Settings.EpsilonDir))
                Epsilon.PatchManifest.Update();
            else if (Settings.ExportTarget == "Arctium" && !string.IsNullOrEmpty(Settings.ArctiumDir))
                Arctium.FileMapping.Update();

            Console.WriteLine("Conversion took " + totalTimeMS + "ms");
            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }

        private static void ConvertWotLKADT(string inputADT)
        {
            var path = Path.Combine(Settings.InputDir, "world", "maps", Settings.MapName);
            if (!Path.GetFileNameWithoutExtension(inputADT).StartsWith(Settings.MapName, StringComparison.InvariantCultureIgnoreCase))
            {
                Console.WriteLine("Ignoring ADT " + inputADT + " because it's not named as map " + Settings.MapName);
                return;
            }

            using (var ms = new MemoryStream(File.ReadAllBytes(inputADT)))
            {
                var wotlkADT = new Warcraft.NET.Files.ADT.Terrain.Wotlk.Terrain(ms.ToArray());

                var root = ADT.Root.Convert(wotlkADT);

                var adtName = Path.GetFileNameWithoutExtension(inputADT);

                var rootSerialized = root.Serialize();
                cachedRootADTs[adtName] = root;
                WriteADTIfChanged(adtName, "root", rootSerialized);

                var tex0 = ADT.Tex0.Convert(wotlkADT);
                var tex0Serialized = tex0.Serialize();
                WriteADTIfChanged(adtName, "tex0", tex0Serialized);

                var obj0 = ADT.Obj0.Convert(wotlkADT);
                var obj0Serialized = obj0.Serialize();
                cachedOBJ0ADTs[adtName.ToLowerInvariant() + "_obj0"] = obj0;
                WriteADTIfChanged(adtName, "obj0", obj0Serialized);

                var obj1 = ADT.Obj1.Convert(wotlkADT, obj0);
                var obj1Serialized = obj1.Serialize();
                cachedOBJ1ADTs[adtName.ToLowerInvariant() + "_obj1"] = obj1;
                WriteADTIfChanged(adtName, "obj1", obj1Serialized);

                //var lod = ADT.LOD.Convert(wotlkADT);
                //var lodSerialized = lod.Serialize();
                //var lodPath = Path.Combine(Settings.OutputDir, "world", "maps", Settings.MapName, Path.GetFileNameWithoutExtension(inputADT) + "_lod.adt");
                //WriteFileIfChanged(lodPath, lodSerialized);
            }
        }

        private static void WriteADTIfChanged(string adtName, string type, byte[] data)
        {
            var path = Path.Combine(ExportHelper.GetExportDirectory(), "world", "maps", Settings.MapName, adtName + (type == "root" ? ".adt" : "_" + type + ".adt"));

            if (!File.Exists(path))
            {
                var tries = 0;
                var doneWriting = false;

                while (!doneWriting)
                {
                    try
                    {
                        File.WriteAllBytes(path, data);
                        doneWriting = true;
                    }
                    catch (IOException ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Failed to write ADT " + Path.GetFileName(path) + ": " + ex.Message + ", retrying..");
                        Console.ResetColor();
                        tries++;

                        if (tries == 5)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Failed to write ADT " + Path.GetFileName(path) + " after 5 tries, throwing.");
                            throw;
                        }
                    }
                }
                
                return;
            }

            var existingData = File.ReadAllBytes(path);
            if (!existingData.SequenceEqual(data))
            {
                var tries = 0;
                var doneWriting = false;

                while (!doneWriting)
                {
                    try
                    {
                        File.WriteAllBytes(path, data);
                        doneWriting = true;
                    }
                    catch (IOException ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Failed to write ADT " + Path.GetFileName(path) + ": " + ex.Message + ", retrying..");
                        Console.ResetColor();
                        tries++;

                        if (tries == 5)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Failed to write ADT " + Path.GetFileName(path) + " after 5 tries, throwing.");
                            throw;
                        }
                    }
                }

                if (Settings.ClientRefresh)
                {
                    var splitType = adtName.Split('_');
                    var tileX = int.Parse(splitType[1]);
                    var tileY = int.Parse(splitType[2]);

                    int tileUpdateFlag = 0;

                    switch (type)
                    {
                        case "root":
                            tileUpdateFlag = 0x1;
                            break;
                        case "tex0":
                            tileUpdateFlag = 0x2;
                            break;
                        case "obj0":
                        case "obj1":
                            tileUpdateFlag = 0x4;
                            break;
                        case "lod":
                            tileUpdateFlag = 0x8;
                            break;
                    }

                    Console.WriteLine("Updated tile " + adtName + " " + type);

                    if (UpdatedTiles.TryGetValue(tileY * 64 + tileX, out int flags))
                    {
                        UpdatedTiles[tileY * 64 + tileX] = flags | tileUpdateFlag;
                    }
                    else
                    {
                        UpdatedTiles[tileY * 64 + tileX] = tileUpdateFlag;
                    }
                }

                return;
            }

            //Console.WriteLine("ADT " + Path.GetFileName(path) + " is unchanged, skipping write..");
        }

        private static void ConvertWDL()
        {
            var wdl = WDL.WDL.Generate(cachedRootADTs, cachedOBJ1ADTs);
            File.WriteAllBytes(Path.Combine(ExportHelper.GetExportDirectory(), "world", "maps", Settings.MapName, Settings.MapName + ".wdl"), wdl.Serialize());
        }

        private static void ConvertWDT()
        {
            if(Settings.TargetVersion >= 927)
            {
                var lightWDT = WDT.LightWDT.GenerateForSL(cachedOBJ0ADTs);
                File.WriteAllBytes(Path.Combine(ExportHelper.GetExportDirectory(), "world", "maps", Settings.MapName, Settings.MapName + "_lgt.wdt"), lightWDT.Serialize());
            }
            else
            {
                Console.WriteLine("Skipping light WDT generation for target version " + Settings.TargetVersion + ", not supported yet.");
            }

            var wdt = WDT.RootWDT.Generate();
            File.WriteAllBytes(Path.Combine(ExportHelper.GetExportDirectory(), "world", "maps", Settings.MapName, Settings.MapName + ".wdt"), wdt.Serialize());
        }

        private static void ConvertMinimaps()
        {
            Minimaps.Minimaps.Convert();
        }
    }
}