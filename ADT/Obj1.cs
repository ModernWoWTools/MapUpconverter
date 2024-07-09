using MapUpconverter.Utils;
using Warcraft.NET.Files.ADT.Chunks;

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



            return bfaObj1;
        }
    }
}
