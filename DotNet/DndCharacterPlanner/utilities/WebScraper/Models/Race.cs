using System;
using System.Collections.Generic;
using Newtonsoft.Json;


namespace WebScraper.Models
{
  public class Race
  {
    public int id;

    public string name;
    public string description;

    public List<Ability> abilities;
    public Dictionary<string, Subrace> subraces;

    public string type;

    public void FillSubracesDescriptions()
    {
      foreach (var sub in subraces.Values)
      {
        string desc = "";
        foreach (var a in sub.abilities)
        {
          desc = desc + "<p>" + a.name + ":" + a.description + "</p>";
        }

        sub.description = desc;
      }
    }
  }
}