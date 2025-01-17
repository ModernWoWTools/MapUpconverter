﻿using MapUpconverter.Utils;
using System.Numerics;
using Warcraft.NET.Files.ADT.Chunks;
using Warcraft.NET.Files.ADT.Chunks.Legion;
using Warcraft.NET.Files.ADT.Entries;
using Warcraft.NET.Files.ADT.Entries.Legion;
using Warcraft.NET.Files.ADT.Flags;

namespace MapUpconverter.ADT
{
    public static class Obj1
    {
        public static Warcraft.NET.Files.ADT.TerrainObject.One.TerrainObjectOne Convert(Warcraft.NET.Files.ADT.Terrain.Wotlk.Terrain wotlkRootADT, Warcraft.NET.Files.ADT.TerrainObject.Zero.TerrainObjectZero bfaObj0)
        {
            var bfaObj1 = new Warcraft.NET.Files.ADT.TerrainObject.One.TerrainObjectOne
            {
                Version = new MVER(18),
                LevelForDetail = new(),
                LevelDoodadDetail = new(),
                LevelDoodadExtent = new(),
                LevelWorldObjectDetail = new(),
                LevelWorldObjectExtent = new(),
            };

            if(Settings.TargetVersion == 735)
            {
                bfaObj1.ModelIndices = bfaObj0.ModelIndices;
                bfaObj1.Models = bfaObj0.Models;

                bfaObj1.WorldModelObjectIndices = bfaObj0.WorldModelObjectIndices;
                bfaObj1.WorldModelObjects = bfaObj0.WorldModelObjects;
            }

            // Right now we just stick everything in the highest LOD level so it renders ASAP -- for client performance reasons this should probably be divied up.
            // When dividing in the future, note that mldxEntries and mlmxEntries will need to be sorted by the dividing radiuses.
            // e.g. if we set lod level 0 to radius < 10 and lod level 3 to >= 10, two dictionaries need to be generated that are sorted from high to low radius separately.
            // Then they need to be concatted together in the order of lod levels so you get something like [9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10].
            bfaObj1.LevelForDetail.ModelLodOffset = [0, 0, 0];
            bfaObj1.LevelForDetail.ModelLodLength = [0, 0, (uint)bfaObj0.ModelPlacementInfo.MDDFEntries.Count];
            bfaObj1.LevelForDetail.WorldObjectLodOffset = [0, 0, 0];
            bfaObj1.LevelForDetail.WorldObjectLodLength = [0, 0, (uint)bfaObj0.WorldModelObjectPlacementInfo.MODFEntries.Count];

            var mldxEntries = new Dictionary<uint, MLDXEntry>();

            for (var i = 0; i < bfaObj0.ModelPlacementInfo.MDDFEntries.Count; i++)
            {
                var mlddEntry = new MDDFEntry()
                {
                    ScalingFactor = bfaObj0.ModelPlacementInfo.MDDFEntries[i].ScalingFactor,
                    Position = bfaObj0.ModelPlacementInfo.MDDFEntries[i].Position,
                    Rotation = bfaObj0.ModelPlacementInfo.MDDFEntries[i].Rotation,
                    NameId = bfaObj0.ModelPlacementInfo.MDDFEntries[i].NameId,
                    Flags = bfaObj0.ModelPlacementInfo.MDDFEntries[i].Flags,
                    UniqueID = bfaObj0.ModelPlacementInfo.MDDFEntries[i].UniqueID,
                };

                bfaObj1.LevelDoodadDetail.MDDFEntries.Add(mlddEntry);

                var m2BoundingBox = new Warcraft.NET.Files.Structures.BoundingBox();

                var bbID = mlddEntry.NameId;

                if (Settings.TargetVersion == 735)
                {
                    var wotlkModelName = wotlkRootADT.Models.Filenames[(int)wotlkRootADT.ModelPlacementInfo.MDDFEntries[i].NameId].ToLowerInvariant().Replace("\\", "/");
                    if (!Listfile.ReverseMap.TryGetValue(wotlkModelName.ToLower(), out uint fdid))
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Warning: No FDID found for M2 " + wotlkModelName + ", this might indicate an outdated listfile or outdated downported asset filenames.");
                        Console.ResetColor();

                        if (uint.TryParse(Path.GetFileNameWithoutExtension(wotlkModelName), out fdid))
                            Console.WriteLine("Using M2 placeholder filename as FDID: " + fdid);

                        if (fdid == 0)
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

                    bbID = fdid;
                }

                if (!BoundingBoxInfo.boundingBoxBlobDict.TryGetValue(bbID.ToString(), out m2BoundingBox))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("No bounding box found for M2 " + mlddEntry.NameId + ", using empty bounding box. This will cause issues in-game.");
                    Console.ResetColor();
                }

                var recalculatedBoundingBox = MathStuff.CalculateBoundingBox(mlddEntry.Position, new Vector3(mlddEntry.Rotation.Pitch, mlddEntry.Rotation.Yaw, mlddEntry.Rotation.Roll), m2BoundingBox, mlddEntry.ScalingFactor);

                var mldxEntry = new MLDXEntry()
                {
                    BoundingBox = recalculatedBoundingBox,
                    Radius = MathStuff.CalculateMaxRadius(recalculatedBoundingBox)
                };

                mldxEntries.Add(mlddEntry.UniqueID, mldxEntry);
            }

            mldxEntries = mldxEntries.OrderByDescending(e => e.Value.Radius).ToDictionary(e => e.Key, e => e.Value);

            bfaObj1.LevelDoodadDetail.MDDFEntries = bfaObj1.LevelDoodadDetail.MDDFEntries.OrderBy(entry => mldxEntries.Keys.ToList().IndexOf(entry.UniqueID)).ToList();
            bfaObj1.LevelDoodadExtent = new MLDX() { Entries = mldxEntries.Values.ToList() };

            var mlmxEntries = new Dictionary<uint, MLMXEntry>();
            for (var i = 0; i < bfaObj0.WorldModelObjectPlacementInfo.MODFEntries.Count; i++)
            {
                var mlmdEntry = new MLMDEntry()
                {
                    DoodadSet = bfaObj0.WorldModelObjectPlacementInfo.MODFEntries[i].DoodadSet,
                    NameSet = bfaObj0.WorldModelObjectPlacementInfo.MODFEntries[i].NameSet,
                    Scale = bfaObj0.WorldModelObjectPlacementInfo.MODFEntries[i].Scale,
                    Position = bfaObj0.WorldModelObjectPlacementInfo.MODFEntries[i].Position,
                    Rotation = bfaObj0.WorldModelObjectPlacementInfo.MODFEntries[i].Rotation,
                    NameId = bfaObj0.WorldModelObjectPlacementInfo.MODFEntries[i].NameId,
                    Flags = (MLMDFlags)bfaObj0.WorldModelObjectPlacementInfo.MODFEntries[i].Flags,
                    UniqueID = (uint)bfaObj0.WorldModelObjectPlacementInfo.MODFEntries[i].UniqueId,
                };

                // Make sure expected flags are set
                mlmdEntry.Flags |= MLMDFlags.HasScale | MLMDFlags.UseLod;

                bfaObj1.LevelWorldObjectDetail.MLMDEntries.Add(mlmdEntry);

                var wmoBoundingBox = new Warcraft.NET.Files.Structures.BoundingBox();

                var bbID = mlmdEntry.NameId;

                if (Settings.TargetVersion == 735)
                {
                    var wotlkModelName = wotlkRootADT.WorldModelObjects.Filenames[(int)wotlkRootADT.WorldModelObjectPlacementInfo.MODFEntries[i].NameId].ToLowerInvariant().Replace("\\", "/");

                    if (!Listfile.ReverseMap.TryGetValue(wotlkModelName.ToLower(), out uint fdid))
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

                    bbID = fdid;
                }

                if (!BoundingBoxInfo.boundingBoxBlobDict.TryGetValue(bbID.ToString(), out wmoBoundingBox))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("No bounding box found for WMO " + mlmdEntry.NameId + ", using empty bounding box. This will cause issues in-game.");
                    Console.ResetColor();
                }

                var recalculatedBoundingBox = MathStuff.CalculateBoundingBox(mlmdEntry.Position, new Vector3(mlmdEntry.Rotation.Pitch, mlmdEntry.Rotation.Yaw, mlmdEntry.Rotation.Roll), wmoBoundingBox, mlmdEntry.Scale);

                var mlmxEntry = new MLMXEntry()
                {
                    BoundingBox = recalculatedBoundingBox,
                    Radius = MathStuff.CalculateMaxRadius(recalculatedBoundingBox)
                };

                mlmxEntries.Add(mlmdEntry.UniqueID, mlmxEntry);
            }

            mlmxEntries = mlmxEntries.OrderByDescending(e => e.Value.Radius).ToDictionary(e => e.Key, e => e.Value);

            bfaObj1.LevelWorldObjectDetail.MLMDEntries = bfaObj1.LevelWorldObjectDetail.MLMDEntries.OrderBy(entry => mlmxEntries.Keys.ToList().IndexOf(entry.UniqueID)).ToList();
            bfaObj1.LevelWorldObjectExtent = new MLMX() { Entries = mlmxEntries.Values.ToList() };

            return bfaObj1;
        }
    }
}
