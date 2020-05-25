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
    private static class SeparateOptionsPageParser
    {
      public static List<Option> ParseOptions(string fileName)
      {
        string html = File.ReadAllText(Config.DownloadedPagesDir + "/" +  fileName);


        var parser = new HtmlParser();
        var document = parser.Parse(html);


        var options = new List<Option>();

        var elem = document.QuerySelector("#toc0");
        while (elem != null)
        {
          string name = elem.TextContent.Trim();

          elem = elem.NextElementSibling;
          string description = "";
          while (elem != null && elem.NodeName != "H6")
          {
            description = description + HelperFunctions.ReadArbitraryElement(elem);

            elem = elem.NextElementSibling;
          }

          var option = new Option { name = name, description = description };
          options.Add(option);
        }

        

        return options;
      }
    }
  }
}