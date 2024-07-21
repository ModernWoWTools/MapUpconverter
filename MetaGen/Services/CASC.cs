using CASCLib;

namespace MetaGen.Services
{
    public static class CASC
    {
        private static CASCHandler cascHandler;

        public static void Initialize(string program, string? wowFolder)
        {
            CASCConfig.ValidateData = false;
            CASCConfig.ThrowOnFileNotFound = false;
            CASCConfig.UseWowTVFS = false;

            LocaleFlags locale = LocaleFlags.enUS;

            if (wowFolder == null)
            {
                Console.WriteLine("Initializing CASC from web for program " + program + " and locale " + locale);
                cascHandler = CASCHandler.OpenOnlineStorage(program, "eu");
            }
            else
            {
                Console.WriteLine("Initializing CASC from local disk with basedir " + wowFolder + " and program " + program + " and locale " + locale);
                cascHandler = CASCHandler.OpenLocalStorage(wowFolder, program);
            }

            cascHandler.Root.SetFlags(locale);
        }

        public static Stream OpenFile(uint fileDataID)
        {
            return cascHandler.OpenFile((int)fileDataID);
        }

        public static bool FileExists(uint fileDataID)
        {
            return cascHandler.FileExists((int)fileDataID);
        }
    }
}
