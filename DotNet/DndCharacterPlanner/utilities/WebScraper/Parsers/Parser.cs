using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using WebScraper.Models;

namespace WebScraper.Parsers
{
  static partial class Parser
  {
    private enum Mode : byte
    {
      Download,
      ScrapeFiles
    }


    public static void DownloadPages()
    {
      //SpellListPageParser.ScrapeAllSpells(Mode.Download);
      //WikidotMainPageParser.ScrapeAll(Mode.Download);


      string html;

      //html = HelperFunctions.GetHtmlFromUrl("http://gdnd.wikidot.com/feats");
      //File.WriteAllText(Config.DownloadedPagesDir + "/FeatsPage.html.txt", html);


      //html = HelperFunctions.GetHtmlFromUrl("http://dnd5e.wikidot.com/warlock:eldritch-invocations");
      //File.WriteAllText(Config.DownloadedPagesDir + "/EldritchInvocations.html.txt", html);

      //html = HelperFunctions.GetHtmlFromUrl("http://dnd5e.wikidot.com/fighter:battle-master:maneuvers");
      //File.WriteAllText(Config.DownloadedPagesDir + "/BattlemasterManeuvers.html.txt", html);

      //html = HelperFunctions.GetHtmlFromUrl("http://dnd5e.wikidot.com/artificer-infusions");
      //File.WriteAllText(Config.DownloadedPagesDir + "/ArtificerInfusions.html.txt", html);

      //html = HelperFunctions.GetHtmlFromUrl("http://dnd5e.wikidot.com/monk:four-elements:disciplines");
      //File.WriteAllText(Config.DownloadedPagesDir + "/FourElementsDisciplines.html.txt", html);
    }



    public static Database ScrapeFiles()
    {
      (Dictionary<string, Race> races, Dictionary<string, Class> classes) = WikidotMainPageParser.ScrapeAll(Mode.ScrapeFiles);
      var feats = FeatsParser.ParseFeats();
      var spellsAndCantrips = SpellListPageParser.ScrapeAllSpells(Mode.ScrapeFiles);


      var db = new Database { races = races, classes = classes, feats = feats, spellsAndCantrips = spellsAndCantrips};
      db.DoPostParsing();

      return db;
    }
  }
}
