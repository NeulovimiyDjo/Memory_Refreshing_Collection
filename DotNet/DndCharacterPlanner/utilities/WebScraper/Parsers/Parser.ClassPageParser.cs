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
    private static class ClassPageParser
    {
      public static Class ParseClassPage(string html)
      {
        var parser = new HtmlParser();
        var document = parser.Parse(html);

        var mainDiv = document.QuerySelector("#page-content");


        string description = mainDiv.Children[0].TextContent.Trim();
        string requirement = mainDiv.Children[1].TextContent.Trim();

        int pos = 2;
        if (mainDiv.Children[2].NodeName == "BR")
          ++pos;
        var classTable = mainDiv.Children[pos].Children[0];

        (string className, List<int> featsIncreases, List<int> cantripsIncreases, List<int> spellsIncreases,
          string subclassAbilityName, List<int> subclassAbilityLevels, List<Ability> abilitiesList) = ReadClassTable(classTable);


        FillAbilitiesDescriptions(abilitiesList, mainDiv);


        return new Class { name = className, description = description, requirement = requirement,
          feats = featsIncreases, cantrips = cantripsIncreases, spells = spellsIncreases,
          subclassAbilityName = subclassAbilityName, subclassAbilityLevels = subclassAbilityLevels,
          abilities = abilitiesList};
      }


      private static (string, List<int>, List<int>, List<int>, string, List<int>, List<Ability>)
        ReadClassTable(AngleSharp.Dom.IElement table)
      {
        string className = table.Children[0].Children[0].TextContent.Trim();
        className = className.Replace("The ", "");


        int featuresColNum = 0;
        int cantripsColNum = 0;
        int spellsColNum = 0;
        int counter = 0;
        foreach (var col in table.Children[1].Children)
        {
          if (col.TextContent.Trim() == "Features")
            featuresColNum = counter;
          else if (col.TextContent.Trim() == "Cantrips Known")
            cantripsColNum = counter;
          else if (col.TextContent.Trim() == "Spells Known")
            spellsColNum = counter;

          ++counter;
        }

        int prevCantripsAmount = 0;
        int prevSpellsAmount = 0;



        var featsIncreases = new List<int>();
        var cantripsIncreases = new List<int>();
        var spellsIncreases = new List<int>();
        string subclassAbilityName = "";
        var subclassAbilityLevels = new List<int>();
        var abilitiesList = new List<Ability>();
        for (int rowNum = 2; rowNum < table.Children.Length; ++rowNum)
        {
          int level = rowNum - 1;

          string featureNamesString = table.Children[rowNum].Children[featuresColNum].TextContent.Trim();
          string[] featureNames = featureNamesString.Split(',');
          foreach (var initialName in featureNames)
          {
            string[] nameParts = initialName.Split('(');
            string name = nameParts[0].Trim();
            if (name == "Rune Master") name = "Rune Mastery"; // misstype on site


            if (name == "Ability Score Improvement")
            {
              featsIncreases.Add(level);
            }
            else if (name.Contains(" feature"))
            {
              subclassAbilityName = name.Replace(" feature", "");
              if (subclassAbilityName == "Path") subclassAbilityName = "Primal Path"; // exception for barbarian table
              subclassAbilityLevels.Add(level);
            }
            else if (name.Contains(" improvement")) continue; // not a separate ability
            else if (name != "" && name != "-" && !abilitiesList.Exists(a => a.name == name))
            {
              abilitiesList.Add(new Ability { level = level, name = name });
            }
          }

          if (cantripsColNum > 0)
          {
            string s = table.Children[rowNum].Children[cantripsColNum].TextContent.Trim();
            if (s == "" || s == "-") continue;

            int cantripsAmount = Int32.Parse(s);
            for (int i = 0; i < cantripsAmount - prevCantripsAmount; ++i)
            {
              cantripsIncreases.Add(level);
            }
            prevCantripsAmount = cantripsAmount;
          }

          if (spellsColNum > 0)
          {
            string s = table.Children[rowNum].Children[spellsColNum].TextContent.Trim();
            if (s == "" || s == "-") continue;

            int spellsAmount = Int32.Parse(s);
            for (int i = 0; i < spellsAmount - prevSpellsAmount; ++i)
            {
              spellsIncreases.Add(level);
            }
            prevSpellsAmount = spellsAmount;
          }
        }

        return (className, featsIncreases, cantripsIncreases, spellsIncreases,
          subclassAbilityName, subclassAbilityLevels, abilitiesList);
      }


      private static void FillAbilitiesDescriptions(List<Ability> abilitiesList, AngleSharp.Dom.IElement mainDiv)
      {
        var abilitiesHeaders = mainDiv.QuerySelectorAll("h3");

        foreach (var header in abilitiesHeaders)
        {
          string abilityName = header.TextContent.Trim();
          if (abilityName == "Beast Shapes") abilityName = "Beast Spells"; // misstype on site

          var ability = abilitiesList.Find(a => a.name == abilityName);
          if (ability == null) continue;

          string description = "";
          var elem = header.NextElementSibling;
          while (elem != null && elem.NodeName != "H3")
          {
            description = description + HelperFunctions.ReadArbitraryElement(elem);

            elem = elem.NextElementSibling;
          }

          ability.description = description;

          FillOptions(ability, header);
        }
      }


      public static void FillOptions(Ability ability, AngleSharp.Dom.IElement header)
      {
        var options = new List<Option>();
        ability.options = options;

        if (ability.name == "Eldritch Invocations")
          ability.options = SeparateOptionsPageParser.ParseOptions("EldritchInvocations.html.txt");

        if (ability.name == "Combat Superiority")
          ability.options = SeparateOptionsPageParser.ParseOptions("BattlemasterManeuvers.html.txt");

        switch (ability.name)
        {
          case "Metamagic":
          case "Fighting Style":
            break;
          default:
            return;
        }



        string description = "";
        string optionName = "";
        string optionDescription = "";
        var elem = header.NextElementSibling;
        bool textBeforeOptions = true;
        while (elem != null && elem.NodeName != "H3")
        {
          if (elem.NodeName != "P") // header with option name
          {
            if (textBeforeOptions)
              textBeforeOptions = false;
            else
              options.Add(new Option { name = optionName, description = optionDescription }); // write previous one

            optionName = elem.TextContent.Trim();
            if (!char.IsLetterOrDigit(optionName[0]))
            {
              optionName = optionName.Remove(0, 1);
              optionName = optionName.Trim();
            }
            optionDescription = "";
          }
          else // just description
          {
            if (textBeforeOptions)
              description = description + HelperFunctions.ReadArbitraryElement(elem);
            else
              optionDescription = optionDescription + HelperFunctions.ReadArbitraryElement(elem);
          }


          elem = elem.NextElementSibling;
        }
        options.Add(new Option { name = optionName, description = optionDescription }); // write last one

        // replace old one with new one without options text
        ability.description = description;
      }
    }
  }
}