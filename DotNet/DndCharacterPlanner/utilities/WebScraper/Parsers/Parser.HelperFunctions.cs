using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using WebScraper.Models;
using System.Net.Http;

namespace WebScraper.Parsers
{
  static partial class Parser
  {
    private static class HelperFunctions
    {
      public static string GetHtmlFromUrl(string url)
      {
        var httpClient = new HttpClient();
        var request = httpClient.GetAsync(url);
        var response = request.Result.Content.ReadAsStringAsync();


        return response.Result;
      }


      public static string ReadArbitraryElement(AngleSharp.Dom.IElement elem)
      {
        string result = "";

        if (elem.TagName == "TABLE")
        {
          var tbody = elem.Children[0];
          result = ReadTable(tbody);
        }
        else if (elem.TagName == "UL" || elem.TagName == "OL")
        {
          result = ReadList(elem);
        }
        else if (elem.TagName == "P")
        {
          result = "<p>" + elem.TextContent.Trim() + "</p>";
        }
        else
        {
          result = elem.TextContent.Trim();
        }

        return result;
      }

      public static string ReadTable(AngleSharp.Dom.IElement tbody)
      {
        string result = "";

        result = result + "<table>";
        foreach (var row in tbody.Children)
        {
          result = result + "<tr>";
          foreach (var item in row.Children)
          {
            string text = item.TextContent.Trim();
            if (text == "") text = "X";

            if (item.HasAttribute("colspan"))
            {
              var colspan = item.GetAttribute("colspan");
              string val = colspan.ToString();

              result = result + "<td colspan=\"" + val + "\">" + text + "</td>";
            }
            else
            {
              result = result + "<td>" + text + "</td>";
            }
          }
          result = result + "</tr>";
        }
        result = result + "</table>";

        return result;
      }


      public static string ReadList(AngleSharp.Dom.IElement ul)
      {
        string result = "";

        result = result + "<ul style=\"list-style-type: square; padding-left: 2em;\">";
        foreach (var li in ul.Children)
        {
          string text = li.TextContent.Trim();
          result = result + "<li>" + text + "</li>";
        }
        result = result + "</ul>";

        return result;
      }
    }
  }
}