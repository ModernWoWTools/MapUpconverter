using CASCLib;

namespace MetaGen.Services
{
    public static class CASC
    {
        private static CASCHandler cascHandler;

        public static void Initialize(string program, string? wowFolderOrCDNHost)
        {
            CASCConfig.ValidateData = false;
            CASCConfig.ThrowOnFileNotFound = false;
            CASCConfig.UseWowTVFS = false;

            LocaleFlags locale = LocaleFlags.enUS;

            if (wowFolderOrCDNHost == null)
            {
                Console.WriteLine("Initializing CASC from Blizzard CDN for program " + program + " and locale " + locale);
                cascHandler = CASCHandler.OpenOnlineStorage(program, "eu");
            }
            else if(wowFolderOrCDNHost.StartsWith("http://") || wowFolderOrCDNHost.StartsWith("https://"))
            {
                CASCConfig.OverrideCDNHost = wowFolderOrCDNHost;
                Console.WriteLine("Initializing CASC from CDN with host " + wowFolderOrCDNHost + " and program " + program + " and locale " + locale);
                cascHandler = CASCHandler.OpenOnlineStorage(program, "eu");
            }
            else if(Directory.Exists(wowFolderOrCDNHost))
            {
                Console.WriteLine("Initializing CASC from local disk with basedir " + wowFolderOrCDNHost + " and program " + program + " and locale " + locale);
                cascHandler = CASCHandler.OpenLocalStorage(wowFolderOrCDNHost, program);
            }
            else
            {
                throw new Exception("Invalid WoW folder or CDN host given!");
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
