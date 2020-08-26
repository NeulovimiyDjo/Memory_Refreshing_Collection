using System;
using System.IO;
using System.Net.Http;
using AngleSharp.Parser.Html;

namespace WebScraper.Downloaders.Dnd5eWikidot
{
    public static partial class Downloader
    {
        private const string mainPageUrl = "http://dnd5e.wikidot.com";
        private static readonly string downloadedPagesDir;

        private static readonly HttpClient httpClient = new HttpClient();

        static Downloader()
        {
            string webScraperProjectDir = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
            downloadedPagesDir = webScraperProjectDir + "/../dnd5e_downloaded_pages";
        }


        public static void DownloadAllWikidotPages()
        {
            string mainPageHtml = CommonHelpers.GetHtmlFromUrl(mainPageUrl);

            var parser = new HtmlParser();
            var document = parser.Parse(mainPageHtml);

            CommonHelpers.SavePage(mainPageHtml, "{downloadedPagesDir}/MainPage.html.txt");
            MainPageLinksHelper.DownloadAllPagesInDocument(document);
            SpecialPagesHelper.DownloadAllSpecialPages();
        }
    }
}
