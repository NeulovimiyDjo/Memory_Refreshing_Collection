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
    private static class WikidotMainPageParser
    {
      private enum Type : byte
      {
        Race,
        Background,
        Class,
        Subclass,
        Feat
      }


      public static (Dictionary<string, Race>, Dictionary<string, Class>) ScrapeAll(Mode mode)
      {
        string mainPageHtml = GetMainPageHtml(mode);
        
        if (mode == Mode.Download)
        {
          Directory.CreateDirectory(Config.DownloadedPagesDir + "/race_pages");
          Directory.CreateDirectory(Config.DownloadedPagesDir + "/background_pages");
          Directory.CreateDirectory(Config.DownloadedPagesDir + "/class_pages");
          Directory.CreateDirectory(Config.DownloadedPagesDir + "/subclass_pages");
          File.WriteAllText(Config.DownloadedPagesDir + "/MainPage.html.txt", mainPageHtml);
        }


        var parser = new HtmlParser();
        var document = parser.Parse(mainPageHtml);


        var races = DoRaces(document, mode);

        DoBackgrounds(document, mode);

        var classes = DoClassesSubclasses(document, mode);


        return (races, classes);
      }


      private static Dictionary<string, Race> DoRaces(IHtmlDocument document, Mode mode)
      {
        Dictionary<string, Race> races = null;
        if (mode == Mode.ScrapeFiles)
        {
          races = new Dictionary<string, Race>();
        }


        int firstClassNum = 1;
        int lastClassNum = 3;
        for (int i = firstClassNum; i <= lastClassNum; ++i)
        {
          var header = document.QuerySelector("#toc" + i);
          string raceType = header.TextContent.Trim();


          header = header.NextElementSibling;
          var anchors = header.QuerySelectorAll("a");

          for (int k = 0; k < anchors.Count(); k++)
          {
            string url = ((IHtmlAnchorElement)anchors[k]).Href;
            var raceFileName = raceType + "__" + anchors[k].TextContent.Trim();

            Race race = HandleLink(url, raceFileName, mode, Type.Race);

            if (mode == Mode.ScrapeFiles)
            {
              if (race.name.Contains(" HB") || race.name.Contains("(HB)") || !raceType.Contains("Common Races") && race.name != "Tabaxi") continue;

              race.type = raceType;
              races.Add(race.name, race);
            }
          }
        }


        return races;
      }

      private static void DoBackgrounds(IHtmlDocument document, Mode mode)
      {
        //List<Class> classes = null;
        if (mode == Mode.ScrapeFiles)
        {
          //classes = new List<Class>();
        }


        var header = document.QuerySelector("#toc" + 4);

        header = header.NextElementSibling;
        var anchors = header.QuerySelectorAll("a");

        for (int k = 0; k < anchors.Count(); k++)
        {
          string url = ((IHtmlAnchorElement)anchors[k]).Href;
          var backgroundFileName = anchors[k].TextContent.Trim();

          HandleLink(url, backgroundFileName, mode, Type.Background);  
        }


        header = header.ParentElement.NextElementSibling.Children[1];
        anchors = header.QuerySelectorAll("a");

        for (int k = 0; k < anchors.Count(); k++)
        {
          string url = ((IHtmlAnchorElement)anchors[k]).Href;
          var backgroundFileName = anchors[k].TextContent.Trim();

          HandleLink(url, backgroundFileName, mode, Type.Background);  
        }


        //return classes;
      }

      private static Dictionary<string, Class> DoClassesSubclasses(IHtmlDocument document, Mode mode)
      {
        Dictionary<string, Class> classes = null;
        if (mode == Mode.ScrapeFiles)
        {
          classes = new Dictionary<string, Class>();
        }


        int firstClassNum = 5;
        int lastClassNum = 18;
        for (int i = firstClassNum; i <= lastClassNum; ++i)
        {
          var header = document.QuerySelector("#toc" + i);
          var anchors = header.QuerySelectorAll("a");

          string url = ((IHtmlAnchorElement)anchors[0]).Href;
          string classFileName = anchors[0].TextContent.Trim();

          if (mode == Mode.ScrapeFiles)
          {
            if (classFileName.Contains("Artificer") || classFileName.Contains("Rune Scribe")) continue;
          }

          Class cls = HandleLink(url, classFileName, mode, Type.Class);

          if (mode == Mode.ScrapeFiles)
          {
            classes.Add(cls.name, cls);
            cls.subclasses = new Dictionary<string, Subclass>();
          }

          if (i == 14) continue; // no subclasses for Rune Scribe (UA)
          header = header.NextElementSibling.NextElementSibling;
          anchors = header.QuerySelectorAll("a");

          for (int k = 0; k < anchors.Count(); k++)
          {
            url = ((IHtmlAnchorElement)anchors[k]).Href;
            string subclassName = anchors[k].TextContent.Trim();
            var subclassFileName = classFileName + "__" + subclassName;

            var subclass = HandleLink(url, subclassFileName, mode, Type.Subclass);

            if (mode == Mode.ScrapeFiles)
            {
              if (subclassName.Contains("(UA)") || subclassName.Contains("(Amonkhet)") || subclassName.Contains("(HB)") || subclassName.Contains(" HB")) continue;

              subclass.name = subclassName;
              cls.subclasses.Add(subclass.name, subclass);
            }
          }
        }


        return classes;
      }

      private static void DoFeats(IHtmlDocument document, Mode mode)
      {
        //List<Class> classes = null;
        if (mode == Mode.ScrapeFiles)
        {
          //classes = new List<Class>();
        }


        var header = document.QuerySelector("#toc" + 20);
        string featsType = header.TextContent.Trim();

        header = header.NextElementSibling;
        var anchors = header.QuerySelectorAll("a");

        for (int k = 0; k < anchors.Count(); k++)
        {
          string url = ((IHtmlAnchorElement)anchors[k]).Href;
          var backgroundFileName = featsType + "__" + anchors[k].TextContent.Trim();

          HandleLink(url, backgroundFileName, mode, Type.Feat);  
        }


        header = header.ParentElement.NextElementSibling.Children[1];
        anchors = header.QuerySelectorAll("a");

        for (int k = 0; k < anchors.Count(); k++)
        {
          string url = ((IHtmlAnchorElement)anchors[k]).Href;
          var backgroundFileName = featsType + "__" + anchors[k].TextContent.Trim();

          HandleLink(url, backgroundFileName, mode, Type.Feat);  
        }


        header = header.ParentElement.NextElementSibling.Children[1];
        anchors = header.QuerySelectorAll("a");

        for (int k = 0; k < anchors.Count(); k++)
        {
          string url = ((IHtmlAnchorElement)anchors[k]).Href;
          var backgroundFileName = featsType + "__" + anchors[k].TextContent.Trim();

          HandleLink(url, backgroundFileName, mode, Type.Feat);  
        }





        header = document.QuerySelector("#toc" + 21);
        featsType = header.TextContent.Trim();

        header = header.NextElementSibling;
        anchors = header.QuerySelectorAll("a");

        for (int k = 0; k < anchors.Count(); k++)
        {
          string url = ((IHtmlAnchorElement)anchors[k]).Href;
          var backgroundFileName = featsType + "__" + anchors[k].TextContent.Trim();

          HandleLink(url, backgroundFileName, mode, Type.Feat);  
        }


        header = header.ParentElement.NextElementSibling.Children[1];
        anchors = header.QuerySelectorAll("a");

        for (int k = 0; k < anchors.Count(); k++)
        {
          string url = ((IHtmlAnchorElement)anchors[k]).Href;
          var backgroundFileName = featsType + "__" + anchors[k].TextContent.Trim();

          HandleLink(url, backgroundFileName, mode, Type.Feat);  
        }


        //return classes;
      }


      private static dynamic HandleLink(string url, string fileName, Mode mode, Type type)
      {
        string subDirectory;
        switch (type)
        {
          case Type.Race:
            subDirectory = "race_pages";
            break;
          case Type.Background:
            subDirectory = "background_pages";
            break;
          case Type.Class:
            subDirectory = "class_pages";
            break;
          case Type.Subclass:
            subDirectory = "subclass_pages";
            break;
          case Type.Feat:
            subDirectory = "feat_pages";
            break;
          default:
            throw new Exception("Invalid Type");
        }

        if (fileName.Contains('/')) fileName = fileName.Replace("/", "");
        if (fileName.Contains(' ')) fileName = fileName.Replace(" ", "_");

        fileName = Config.DownloadedPagesDir + "/" + subDirectory + "/" + fileName + ".html.txt";


        string html;
        if (mode == Mode.ScrapeFiles)
        {
          html = File.ReadAllText(fileName);
        }
        else
        {
          html = HelperFunctions.GetHtmlFromUrl(url);
        }


        if (mode == Mode.ScrapeFiles)
        {
          if (!Config.Silent) Console.WriteLine("parsing: " + url);

          switch (type)
          {
            case Type.Race:
              return RacePageParser.ParseRacePage(html);
            case Type.Background:
              return null;
            case Type.Class:
              return ClassPageParser.ParseClassPage(html);
            case Type.Subclass:
              return SubclassPageParser.ParseSubclassPage(html);
            case Type.Feat:
              return null;
            default:
              throw new Exception("Invalid Type");
          }
        }
        else
        {
          if (!Config.Silent) Console.WriteLine("downloading: " + url);
          File.WriteAllText(fileName, html);
        }


        return null;
      }


      private static string GetMainPageHtml(Mode mode)
      {
        if (mode == Mode.ScrapeFiles)
        {
          return File.ReadAllText(Config.DownloadedPagesDir + "/MainPage.html.txt");
        }
        else
        {
          string mainPageUrl = "http://dnd5e.wikidot.com";
          return HelperFunctions.GetHtmlFromUrl(mainPageUrl);
        }
      }
    }
  }
}
