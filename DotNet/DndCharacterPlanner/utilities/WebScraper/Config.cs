namespace WebScraper
{
  static class Config
  {
    public static bool Silent { get; set; } = false;
    
    public static string DownloadedPagesDir { get; set; }
    public static string ServerDataDir { get; set; }
  }
}