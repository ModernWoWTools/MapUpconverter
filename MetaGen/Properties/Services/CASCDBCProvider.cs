using DBCD.Providers;
using MetaGen.Properties.Services;

namespace MetaGen
{
    class CASCDBCProvider : IDBCProvider
    {
        public Stream StreamForTableName(string tableName, string build)
        {
            uint fileDataID = 0;

            switch (tableName)
            {
                case "GroundEffectTexture":
                    fileDataID = 1308499;
                    break;
                case "GroundEffectDoodad":
                    fileDataID = 1308057;
                    break;
                default:
                    throw new Exception("Don't have a FDID mapping for DBC " + tableName);
            }

            if (CASC.FileExists(fileDataID))
            {
                return CASC.OpenFile(fileDataID);
            }
            else
            {
                throw new FileNotFoundException("Could not find " + fileDataID);
            }
        }
    }
}