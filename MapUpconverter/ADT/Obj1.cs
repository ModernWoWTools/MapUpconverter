using MapUpconverter.Utils;
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

                var m2BoundingBox = BoundingBoxInfo.boundingBoxBlobDict.TryGetValue(mlddEntry.NameId.ToString(), out var boundingBox) ? boundingBox : new Warcraft.NET.Files.Structures.BoundingBox();
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

                var wmoBoundingBox = BoundingBoxInfo.boundingBoxBlobDict.TryGetValue(mlmdEntry.NameId.ToString(), out var boundingBox) ? boundingBox : new Warcraft.NET.Files.Structures.BoundingBox();
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
