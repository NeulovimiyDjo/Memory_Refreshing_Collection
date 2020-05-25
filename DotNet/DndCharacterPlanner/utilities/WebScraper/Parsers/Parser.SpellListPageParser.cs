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
    private static class SpellListPageParser
    {
      public static List<Spell> ScrapeAllSpells(Mode mode)
      {
        string allSpellsPageHtml = GetAllSpellsPage(mode);
        
        if (mode == Mode.Download)
        {
          Directory.CreateDirectory(Config.DownloadedPagesDir + "/spell_pages");
          File.WriteAllText(Config.DownloadedPagesDir + "/SpellsList.html.txt", allSpellsPageHtml);
        }    


        List<Spell> spells = null;
        if (mode == Mode.ScrapeFiles)
        {
          spells = new List<Spell>();
        }

        var parser = new HtmlParser();
        var document = parser.Parse(allSpellsPageHtml);
        var tableElements = document.QuerySelectorAll("table");

        foreach (var table in tableElements)
        {
          var tbody = table.Children[0];
          var anchors = tbody.QuerySelectorAll("a"); // AngleSharp can't find <a> as children of table rows => hack it
          int anchorIndex = -1; // anchors[i] correspond to row[i+1]

          foreach (var row in tbody.Children)
          {
            if (anchorIndex == -1) // head of the table
            {
              anchorIndex = 0;
              continue;
            }


            var anchor = anchors[anchorIndex++];
            Spell spell = HandleRow(row, anchor, mode);


            if (mode == Mode.ScrapeFiles)
            {
              spells.Add(spell);
            }
          }
        }


        return spells;
      }

   
      private static string GetAllSpellsPage(Mode mode)
      {
        if (mode == Mode.ScrapeFiles)
        {
          return File.ReadAllText(Config.DownloadedPagesDir + "/SpellsList.html.txt");
        }
        else
        {
          string allSpellsListUrl = "https://dnd5e.fandom.com/wiki/List_of_Spells";
          return HelperFunctions.GetHtmlFromUrl(allSpellsListUrl);
        }
      }


      private static Spell HandleRow(AngleSharp.Dom.IElement row, AngleSharp.Dom.IElement anchor, Mode mode)
      {
        Spell spell = null;

        for (int i = 0; i < row.Children.Length; i++)
        {
          var item = row.Children[i];
          var itemText = item.TextContent.Trim();

          string url = ((IHtmlAnchorElement)anchor).Href;
          url = System.Text.RegularExpressions.Regex.Replace(url, @"about://", "");
          url = "https://dnd5e.fandom.com" + url;


          if (i == 0) // name and link
          {
            spell = HandleFirstItem(itemText, url, mode);
          }           


          if (mode == Mode.ScrapeFiles)
          {
            HandleOtherItems(i, row.Children.Length, itemText, spell);
          }
        }

        return spell;
      }


      private static Spell HandleFirstItem(string itemText, string url, Mode mode)
      {
        string fileName = "";
        if (mode == Mode.Download || mode == Mode.ScrapeFiles)
        {
          fileName = itemText;
          if (fileName.Contains('/')) fileName = fileName.Replace("/", "");
          if (fileName.Contains(' ')) fileName = fileName.Replace(" ", "_");

          fileName = Config.DownloadedPagesDir + "/spell_pages/" + fileName + ".html.txt";
        }


        string html;
        if (mode == Mode.ScrapeFiles)
        {
          html = File.ReadAllText(fileName);
        }
        else
        {
          html = HelperFunctions.GetHtmlFromUrl(url);
        }


        Spell spell = null;
        if (mode == Mode.ScrapeFiles)
        {
          if (!Config.Silent) Console.WriteLine("parsing: " + url);
          spell = SpellPageParser.ParseSpellPage(html);
        }
        else
        {
          if (!Config.Silent) Console.WriteLine("downloading: " + url);
          File.WriteAllText(fileName, html);
        }

        return spell;
      }

      private static void HandleOtherItems(int i, int rowLength, string itemText, Spell spell)
      {
        if (rowLength == 5 && i == 4) // no ritual column for cantrips
        {
          spell.ritual = false;
          i = 5;
        }

        switch (i)
        {
          case 0: // was already handled by HandleFirstItem()
            break;
          case 1: // school
            break;
          case 2: // time
            break;
          case 3: // save
            spell.save = itemText;
            break;
          case 4: // ritual
            spell.ritual = itemText != "";
            break;
          case 5: // concentration
            spell.concentration = itemText != "";
            break;
          default:
            break;
        }
      }
    }
  }
}
