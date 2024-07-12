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

            bfaObj0.ModelPlacementInfo = wotlkRootADT.ModelPlacementInfo;
            for (int i = 0; i < wotlkRootADT.ModelPlacementInfo.MDDFEntries.Count; i++)
            {
                var wotlkModelName = wotlkRootADT.Models.Filenames[(int)wotlkRootADT.ModelPlacementInfo.MDDFEntries[i].NameId].ToLowerInvariant().Replace("\\", "/");
                if (Listfile.ReverseMap.TryGetValue(wotlkModelName, out var m2FDID))
                {
                    bfaObj0.ModelPlacementInfo.MDDFEntries[i].NameId = m2FDID;
                    bfaObj0.ModelPlacementInfo.MDDFEntries[i].Flags |= Warcraft.NET.Files.ADT.Flags.MDDFFlags.NameIdIsFiledataId;
                }
                else
                {
                    throw new Exception("No FDID found for M2 " + wotlkModelName);
                }
            }

            bfaObj0.WorldModelObjectPlacementInfo = wotlkRootADT.WorldModelObjectPlacementInfo;
            for (int i = 0; i < wotlkRootADT.WorldModelObjectPlacementInfo.MODFEntries.Count; i++)
            {
                var wotlkModelName = wotlkRootADT.WorldModelObjects.Filenames[(int)wotlkRootADT.WorldModelObjectPlacementInfo.MODFEntries[i].NameId].ToLowerInvariant().Replace("\\", "/");
                if (Listfile.ReverseMap.TryGetValue(wotlkModelName, out var wmoFDID))
                {
                    bfaObj0.WorldModelObjectPlacementInfo.MODFEntries[i].NameId = wmoFDID;
                    bfaObj0.WorldModelObjectPlacementInfo.MODFEntries[i].Flags |= Warcraft.NET.Files.ADT.Flags.MODFFlags.NameIdIsFiledataId;
                    bfaObj0.WorldModelObjectPlacementInfo.MODFEntries[i].Flags |= Warcraft.NET.Files.ADT.Flags.MODFFlags.HasScale;
                }
                else
                {
                    throw new Exception("No FDID found for WMO " + wotlkModelName);
                }
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
