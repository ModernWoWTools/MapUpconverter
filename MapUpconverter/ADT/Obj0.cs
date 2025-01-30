using MapUpconverter.Utils;
using Warcraft.NET.Files.ADT.Chunks;
using Warcraft.NET.Files.ADT.Entries;

namespace MapUpconverter.ADT
{
    public static class Obj0
    {
        public static Warcraft.NET.Files.ADT.TerrainObject.Zero.TerrainObjectZero ConvertLegion(string tileName, Warcraft.NET.Files.ADT.Terrain.Wotlk.Terrain wotlkRootADT)
        {
            var legionObj0 = new Warcraft.NET.Files.ADT.TerrainObject.Zero.TerrainObjectZero
            {
                Version = new MVER(18),
                Chunks = new Warcraft.NET.Files.ADT.TerrainObject.Zero.MCNK[256],
                WorldModelObjectIndices = wotlkRootADT.WorldModelObjectIndices,
                WorldModelObjects = wotlkRootADT.WorldModelObjects,
                ModelIndices = wotlkRootADT.ModelIndices,
                Models = wotlkRootADT.Models,
                ModelPlacementInfo = wotlkRootADT.ModelPlacementInfo,
                WorldModelObjectPlacementInfo = wotlkRootADT.WorldModelObjectPlacementInfo
            };

            // M2
            for (int i = 0; i < wotlkRootADT.ModelPlacementInfo.MDDFEntries.Count; i++)
            {
                var wotlkModelName = wotlkRootADT.Models.Filenames[(int)wotlkRootADT.ModelPlacementInfo.MDDFEntries[i].NameId].ToLowerInvariant().Replace("\\", "/");

                if (Path.GetFileNameWithoutExtension(wotlkModelName.ToLower()).StartsWith("noggit"))
                {
                    // We need to make a copy as we edit it later
                    Program.lightEntries[tileName].Add((Path.GetFileNameWithoutExtension(wotlkModelName.ToLower()), new MDDFEntry()
                    {
                        Flags = wotlkRootADT.ModelPlacementInfo.MDDFEntries[i].Flags,
                        NameId = wotlkRootADT.ModelPlacementInfo.MDDFEntries[i].NameId,
                        Position = wotlkRootADT.ModelPlacementInfo.MDDFEntries[i].Position,
                        Rotation = wotlkRootADT.ModelPlacementInfo.MDDFEntries[i].Rotation,
                        ScalingFactor = wotlkRootADT.ModelPlacementInfo.MDDFEntries[i].ScalingFactor
                    }));

                    // Scaled to 0 and hopefully it works on Legion
                    Console.WriteLine("Scaling " + wotlkModelName + " to 0");
                    legionObj0.ModelPlacementInfo.MDDFEntries[i].ScalingFactor = 0;
                    continue;
                }
            }

            // WMO
            for (int i = 0; i < wotlkRootADT.WorldModelObjectPlacementInfo.MODFEntries.Count; i++)
            {
                var wotlkModelName = wotlkRootADT.WorldModelObjects.Filenames[(int)wotlkRootADT.WorldModelObjectPlacementInfo.MODFEntries[i].NameId].ToLowerInvariant().Replace("\\", "/");

                legionObj0.WorldModelObjectPlacementInfo.MODFEntries[i].Flags |= Warcraft.NET.Files.ADT.Flags.MODFFlags.HasScale;

                if (legionObj0.WorldModelObjectPlacementInfo.MODFEntries[i].Flags.HasFlag(Warcraft.NET.Files.ADT.Flags.MODFFlags.UseDoodadSetsFromMWDS))
                    legionObj0.WorldModelObjectPlacementInfo.MODFEntries[i].Flags &= ~Warcraft.NET.Files.ADT.Flags.MODFFlags.UseDoodadSetsFromMWDS;
            }

            for (int i = 0; i < 256; i++)
            {
                legionObj0.Chunks[i] = new();
                if (wotlkRootADT.Chunks[i].Header.ModelReferenceCount > 0 && wotlkRootADT.Chunks[i].ModelReferences != null)
                {
                    legionObj0.Chunks[i].ModelReferences = new();
                    legionObj0.Chunks[i].ModelReferences.ModelReferences = new uint[wotlkRootADT.Chunks[i].Header.ModelReferenceCount];
                    for (int j = 0; j < wotlkRootADT.Chunks[i].Header.ModelReferenceCount; j++)
                    {
                        legionObj0.Chunks[i].ModelReferences.ModelReferences[j] = wotlkRootADT.Chunks[i].ModelReferences.ModelReferences[j];
                    }
                }

                if (wotlkRootADT.Chunks[i].Header.WorldModelObjectReferenceCount > 0 && wotlkRootADT.Chunks[i].ModelReferences != null)
                {
                    legionObj0.Chunks[i].WorldObjectReferences = new();
                    legionObj0.Chunks[i].WorldObjectReferences.WorldObjectReferences = new uint[wotlkRootADT.Chunks[i].Header.WorldModelObjectReferenceCount];
                    for (int j = 0; j < wotlkRootADT.Chunks[i].Header.WorldModelObjectReferenceCount; j++)
                    {
                        legionObj0.Chunks[i].WorldObjectReferences.WorldObjectReferences[j] = wotlkRootADT.Chunks[i].ModelReferences.WorldObjectReferences[j];
                    }
                }
            }

            return legionObj0;
        }

        public static Warcraft.NET.Files.ADT.TerrainObject.Zero.TerrainObjectZero Convert(string tileName, Warcraft.NET.Files.ADT.Terrain.Wotlk.Terrain wotlkRootADT)
        {
            var bfaObj0 = new Warcraft.NET.Files.ADT.TerrainObject.Zero.TerrainObjectZero
            {
                Version = new MVER(18),
                Chunks = new Warcraft.NET.Files.ADT.TerrainObject.Zero.MCNK[256]
            };

            // M2
            bfaObj0.ModelPlacementInfo = wotlkRootADT.ModelPlacementInfo;
            for (int i = 0; i < wotlkRootADT.ModelPlacementInfo.MDDFEntries.Count; i++)
            {
                var wotlkModelName = wotlkRootADT.Models.Filenames[(int)wotlkRootADT.ModelPlacementInfo.MDDFEntries[i].NameId].ToLowerInvariant().Replace("\\", "/");

                if (Path.GetFileNameWithoutExtension(wotlkModelName.ToLower()).StartsWith("noggit"))
                {
                    // We need to make a copy as we edit it later
                    Program.lightEntries[tileName].Add((Path.GetFileNameWithoutExtension(wotlkModelName.ToLower()), new MDDFEntry()
                    {
                        Flags = wotlkRootADT.ModelPlacementInfo.MDDFEntries[i].Flags,
                        NameId = wotlkRootADT.ModelPlacementInfo.MDDFEntries[i].NameId,
                        Position = wotlkRootADT.ModelPlacementInfo.MDDFEntries[i].Position,
                        Rotation = wotlkRootADT.ModelPlacementInfo.MDDFEntries[i].Rotation,
                        ScalingFactor = wotlkRootADT.ModelPlacementInfo.MDDFEntries[i].ScalingFactor
                    }));

                    // Use errorcube scaled to 0 as replacement model for lights.
                    bfaObj0.ModelPlacementInfo.MDDFEntries[i].NameId = 166046;
                    bfaObj0.ModelPlacementInfo.MDDFEntries[i].Flags |= Warcraft.NET.Files.ADT.Flags.MDDFFlags.NameIdIsFiledataId;
                    bfaObj0.ModelPlacementInfo.MDDFEntries[i].ScalingFactor = 0;
                    continue;
                }

                if (!Listfile.ReverseMap.TryGetValue(wotlkModelName, out uint fdid))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Warning: No FDID found for M2 " + wotlkModelName + ", this might indicate an outdated listfile or outdated downported asset filenames.");
                    Console.ResetColor();

                    if (uint.TryParse(Path.GetFileNameWithoutExtension(wotlkModelName), out fdid))
                        Console.WriteLine("Using M2 placeholder filename as FDID: " + fdid);

                    if(fdid == 0)
                    {
                        var splitModel = Path.GetFileNameWithoutExtension(wotlkModelName).Split("_");
                        var lastPart = splitModel[splitModel.Length - 1];
                        if (uint.TryParse(lastPart, out fdid))
                            Console.WriteLine("Using last part of M2 placeholder filename as FDID: " + fdid);
                    }

                    if (Listfile.NameMap.TryGetValue(fdid, out var name) && name.ToLowerInvariant().EndsWith(".m2"))
                    {
                        Console.WriteLine("Found name for new FDID: " + name);
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Detected FDID " + fdid + " is not in listfile or is not an M2, discarding..");
                        Console.ResetColor();
                        fdid = 0;
                    }

                    if (fdid == 0)
                    {
                        var baseName = Path.GetFileName(wotlkModelName);

                        Console.WriteLine("Scanning listfile for files named " + baseName + "...");
                        var matchingFiles = Listfile.ReverseMap.Where(kv => kv.Key.EndsWith(baseName)).ToList();
                        if (matchingFiles.Count > 0)
                        {
                            Console.WriteLine("Found " + matchingFiles.Count + " matching files, using first match: " + matchingFiles[0].Key + " -> " + matchingFiles[0].Value);
                            fdid = matchingFiles[0].Value;
                        }
                    }

                    if (fdid == 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("No FDID found for M2 " + wotlkModelName + ", using errorcube.m2 instead!");
                        fdid = 166046;
                        Console.ResetColor();
                    }
                }

                bfaObj0.ModelPlacementInfo.MDDFEntries[i].NameId = fdid;
                bfaObj0.ModelPlacementInfo.MDDFEntries[i].Flags |= Warcraft.NET.Files.ADT.Flags.MDDFFlags.NameIdIsFiledataId;
            }

            // WMO
            bfaObj0.WorldModelObjectPlacementInfo = wotlkRootADT.WorldModelObjectPlacementInfo;
            for (int i = 0; i < wotlkRootADT.WorldModelObjectPlacementInfo.MODFEntries.Count; i++)
            {
                var wotlkModelName = wotlkRootADT.WorldModelObjects.Filenames[(int)wotlkRootADT.WorldModelObjectPlacementInfo.MODFEntries[i].NameId].ToLowerInvariant().Replace("\\", "/");

                if (!Listfile.ReverseMap.TryGetValue(wotlkModelName, out uint fdid))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Warning: No FDID found for WMO " + wotlkModelName + ", this might indicate an outdated listfile or outdated downported asset filenames.");

                    if (uint.TryParse(Path.GetFileNameWithoutExtension(wotlkModelName), out fdid))
                        Console.WriteLine("Using WMO placeholder filename as FDID: " + fdid);

                    if (fdid == 0)
                    {
                        var baseName = Path.GetFileName(wotlkModelName);

                        Console.WriteLine("Scanning listfile for files named " + baseName + "...");
                        var matchingFiles = Listfile.ReverseMap.Where(kv => kv.Key.EndsWith(baseName)).ToList();
                        if (matchingFiles.Count > 0)
                        {
                            Console.WriteLine("Found " + matchingFiles.Count + " matching files, using first match: " + matchingFiles[0].Key + " -> " + matchingFiles[0].Value);
                            fdid = matchingFiles[0].Value;
                        }
                    }

                    if (fdid == 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("No FDID found for WMO " + wotlkModelName + ", using missingwmo.wmo instead!");
                        fdid = 112521;
                    }

                    Console.ResetColor();
                }

                bfaObj0.WorldModelObjectPlacementInfo.MODFEntries[i].NameId = fdid;
                bfaObj0.WorldModelObjectPlacementInfo.MODFEntries[i].Flags |= Warcraft.NET.Files.ADT.Flags.MODFFlags.NameIdIsFiledataId;
                bfaObj0.WorldModelObjectPlacementInfo.MODFEntries[i].Flags |= Warcraft.NET.Files.ADT.Flags.MODFFlags.HasScale;

                if (bfaObj0.WorldModelObjectPlacementInfo.MODFEntries[i].Flags.HasFlag(Warcraft.NET.Files.ADT.Flags.MODFFlags.UseDoodadSetsFromMWDS))
                    bfaObj0.WorldModelObjectPlacementInfo.MODFEntries[i].Flags &= ~Warcraft.NET.Files.ADT.Flags.MODFFlags.UseDoodadSetsFromMWDS;

            }

            for (int i = 0; i < 256; i++)
            {

                bfaObj0.Chunks[i] = new();
                if (wotlkRootADT.Chunks[i].Header.ModelReferenceCount > 0 && wotlkRootADT.Chunks[i].ModelReferences != null)
                {
                    bfaObj0.Chunks[i].ModelReferences = new();
                    bfaObj0.Chunks[i].ModelReferences.ModelReferences = new uint[wotlkRootADT.Chunks[i].Header.ModelReferenceCount];
                    for (int j = 0; j < wotlkRootADT.Chunks[i].Header.ModelReferenceCount; j++)
                    {
                        bfaObj0.Chunks[i].ModelReferences.ModelReferences[j] = wotlkRootADT.Chunks[i].ModelReferences.ModelReferences[j];
                    }
                }

                if (wotlkRootADT.Chunks[i].Header.WorldModelObjectReferenceCount > 0 && wotlkRootADT.Chunks[i].ModelReferences != null)
                {
                    bfaObj0.Chunks[i].WorldObjectReferences = new();
                    bfaObj0.Chunks[i].WorldObjectReferences.WorldObjectReferences = new uint[wotlkRootADT.Chunks[i].Header.WorldModelObjectReferenceCount];
                    for (int j = 0; j < wotlkRootADT.Chunks[i].Header.WorldModelObjectReferenceCount; j++)
                    {
                        bfaObj0.Chunks[i].WorldObjectReferences.WorldObjectReferences[j] = wotlkRootADT.Chunks[i].ModelReferences.WorldObjectReferences[j];
                    }
                }
            }

            return bfaObj0;
        }
    }
}
