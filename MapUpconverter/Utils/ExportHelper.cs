namespace MapUpconverter.Utils
{
    public static class ExportHelper
    {
        public static string GetExportDirectory()
        {
            if (Settings.ExportTarget == "Epsilon")
                return Path.Combine(Settings.EpsilonDir, "_retail_", "Patches", Settings.EpsilonPatchName);
            else if (Settings.ExportTarget == "Arctium")
                return Path.Combine(Settings.ArctiumDir, "files", Settings.ArctiumPatchName);
            else
                return Settings.OutputDir;
        }
    }
}
