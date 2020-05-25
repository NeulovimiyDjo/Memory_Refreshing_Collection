using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using WebScraper.Models;


namespace WebScraper.Parsers
{
  static partial class Parser
  {
    private static class FeatsParser
    {
      public static List<Option> ParseFeats()
      {
        string html = File.ReadAllText(Config.DownloadedPagesDir + "/FeatsPage.html.txt");


        var parser = new HtmlParser();
        var document = parser.Parse(html);


        var feats = new List<Option>();

        var elem = document.QuerySelector("#toc2");
        while (elem != null && elem.NodeName != "H1")
        {
          string name = elem.TextContent.Trim();

          elem = elem.NextElementSibling;
          string description = "";
          while (elem != null && elem.NodeName != "H2" && elem.NodeName != "H1")
          {
            description = description + HelperFunctions.ReadArbitraryElement(elem);

            elem = elem.NextElementSibling;
          }

          var option = new Option { name = name, description = description };
          feats.Add(option);
        }



        return feats;
      }
    }
  }
}