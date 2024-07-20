using Newtonsoft.Json;

namespace MapUpconverter.Utils
{
    public static class Downloads
    {
        public static string ListfileURL { get; private set; }
        public static string HeightTextureInfoURL { get; private set; }
        public static string GroundEffectInfoURL { get; private set; }
        public static string ModelBlobURL { get; private set; }

        private static HttpClient client = new();

        public static void Initialize(string toolPath)
        {
            if (!File.Exists(Path.Combine(toolPath, "urls.json")))
                throw new FileNotFoundException("urls.json does not exist");

            var manifest = JsonConvert.DeserializeObject<URLManifest>(File.ReadAllText(Path.Combine(toolPath, "urls.json")));

            ListfileURL = manifest.ListfileURL;
            HeightTextureInfoURL = manifest.HeightTextureInfoURL;
            GroundEffectInfoURL = manifest.GroundEffectInfoURL;
            ModelBlobURL = manifest.ModelBlobURL;

            if(!Directory.Exists(Path.Combine(toolPath, "meta")))
                Directory.CreateDirectory(Path.Combine(toolPath, "meta"));
        }

        public async static Task<bool> DownloadListfile(string toolPath)
        {
            if (string.IsNullOrEmpty(ListfileURL))
                throw new Exception("Listfile URL is not set or nor empty");

            var listfileStream = await client.GetAsync(ListfileURL);
            using (var file = File.Create(Path.Combine(toolPath, "meta", "listfile.csv")))
            {
                await listfileStream.Content.CopyToAsync(file);
            }

            return true;
        }

        public async static Task<bool> DownloadHeightTextureInfo(string toolPath)
        {
            if (string.IsNullOrEmpty(HeightTextureInfoURL))
                throw new Exception("Height texture info URL is not set or nor empty");

            var heightTextureInfoStream = await client.GetAsync(HeightTextureInfoURL);
            using (var file = File.Create(Path.Combine(toolPath, "meta", "TextureInfoByFilePath.json")))
            {
                await heightTextureInfoStream.Content.CopyToAsync(file);
            }

            return true;
        }

        public async static Task<bool> DownloadGroundEffectInfo(string toolPath)
        {
            if (string.IsNullOrEmpty(GroundEffectInfoURL))
                throw new Exception("Ground effect info URL is not set or nor empty");

            var groundEffectInfoStream = await client.GetAsync(GroundEffectInfoURL);
            using (var file = File.Create(Path.Combine(toolPath, "meta", "GroundEffectIDsByTextureFileID.json")))
            {
                await groundEffectInfoStream.Content.CopyToAsync(file);
            }

            return true;
        }

        public async static Task<bool> DownloadModelBlob(string toolPath)
        {
            if (string.IsNullOrEmpty(ModelBlobURL))
                throw new Exception("Model blob URL is not set or nor empty");

            var modelBlobStream = await client.GetAsync(ModelBlobURL);
            using (var file = File.Create(Path.Combine(toolPath, "meta", "blob.json")))
            {
                await modelBlobStream.Content.CopyToAsync(file);
            }

            return true;
        }

        private struct URLManifest
        {
            public string ListfileURL { get; set; }
            public string HeightTextureInfoURL { get; set; }
            public string GroundEffectInfoURL { get; set; }
            public string ModelBlobURL { get; set; }
        }
    }
}
