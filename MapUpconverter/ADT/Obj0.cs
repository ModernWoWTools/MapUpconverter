using MapUpconverter.Utils;
using Warcraft.NET.Files.ADT.Chunks;

namespace MapUpconverter.ADT
{
    public static class Obj0
    {
        public static Warcraft.NET.Files.ADT.TerrainObject.Zero.TerrainObjectZero Convert(Warcraft.NET.Files.ADT.Terrain.Wotlk.Terrain wotlkRootADT)
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

                    if (uint.TryParse(Path.GetFileNameWithoutExtension(wotlkModelName), out fdid))
                        Console.WriteLine("Using M2 placeholder filename as FDID: " + fdid);

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
                    }

                    Console.ResetColor();
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
