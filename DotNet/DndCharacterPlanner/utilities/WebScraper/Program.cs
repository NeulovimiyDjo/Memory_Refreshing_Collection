using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using WebScraper.Models;
using WebScraper.Parsers;

namespace WebScraper
{
  class Program
  {
    static void Main(string[] args)
    {
      if (args.Length < 1) return;
      if (args.Length > 1 && args[1] == "--silent") Config.Silent = true;


      string webScraperProjectDir = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;

      Config.DownloadedPagesDir = webScraperProjectDir + "/../webscraper_downloaded_pages";
      Config.ServerDataDir = webScraperProjectDir + "/../../server/Data";




      if (args[0] == "--scrape-files")
      {
        Directory.CreateDirectory(Config.ServerDataDir);

        Database db = Parser.ScrapeFiles();
        SaveObject(db, "database.json");
      }
      else if (args[0] == "--download-pages")
      {
        Parser.DownloadPages();
      }
      else if (args[0] == "--print")
      {
        Database db = Parser.ScrapeFiles();

        PrintSpells(db.spells);
        PrintClasses(db.classes);
      }
    }


    private static void SaveObject(dynamic obj, string fileName)
    {
      using (var fs = File.CreateText(Config.ServerDataDir + "/" + fileName))
      {
        var serializer = new JsonSerializer();
        serializer.Serialize(fs, obj);
      }
    }


    private static void PrintSpells(List<Spell> spells)
    {
      foreach (var spell in spells)
      {
        Console.WriteLine("Name: " + spell.name);

        foreach (var category in spell.categories)
        {
          Console.WriteLine("Category: " + category);
        }

        Console.WriteLine("Time: " + spell.time);
        Console.WriteLine("Range: " + spell.range);
        Console.WriteLine("Components: " + spell.components);
        Console.WriteLine("Duration: " + spell.duration);

        Console.WriteLine("Description: " + spell.description);

        Console.WriteLine("Save: " + spell.save);
        Console.WriteLine("Ritual: " + spell.ritual);
        Console.WriteLine("Concentration: " + spell.concentration);

        Console.WriteLine();
      }
    }


    private static void PrintClasses(Dictionary<string, Class> classes)
    {
      foreach (var cls in classes.Values)
      {
        Console.WriteLine("=======================================================================");

        Console.WriteLine("Class Name: " + cls.name);
        Console.WriteLine("Description: " + cls.description);
        Console.WriteLine("Requirement: " + cls.requirement);

        Console.Write("Feats Increases: [");
        bool first = true;
        foreach (var lvl in cls.feats)
        {
          if (first) first = false; else Console.Write(",");
          Console.Write(lvl);
        }
        Console.WriteLine("]");

        foreach (var ability in cls.abilities)
        {
          Console.WriteLine();
          Console.WriteLine("Ability: " + ability.name + " (" + ability.level + ")");
          Console.WriteLine(ability.description);

          foreach (var option in ability.options)
          {
            Console.WriteLine("Option: " + option.name);
            Console.WriteLine(option.description);
          }
        }

        foreach (var subclass in cls.subclasses.Values)
        {
          Console.WriteLine("------------------------------------------------------------------");

          Console.WriteLine("Subclass Name: " + subclass.name);
          Console.WriteLine("Subclass Description: " + subclass.description);

          foreach (var ability in subclass.abilities)
          {
            Console.WriteLine();
            Console.WriteLine("Subclass Ability: " + ability.name + " (" + ability.level + ")");
            Console.WriteLine(ability.description);

            foreach (var option in ability.options)
            {
              Console.WriteLine("Option: " + option.name);
              Console.WriteLine(option.description);
            }
          }

          Console.WriteLine("------------------------------------------------------------------");
          Console.WriteLine();
        }

        Console.WriteLine("=======================================================================");
        Console.WriteLine();
        Console.WriteLine();
      }
    }
  }
}
