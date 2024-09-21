namespace HangFire.Web
{
    public class ConfigSettings
    {
        public required string Directory { get; set; }
        public required string RssFilename { get; set; }
        public required string RssUrl { get; set; }
        public string TempPath => Path.Combine(Directory, RssFilename);
    }
}
