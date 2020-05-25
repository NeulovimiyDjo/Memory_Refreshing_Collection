using System;
using System.Collections.Generic;
using Newtonsoft.Json;


namespace WebScraper.Models
{
  public class Subrace
  {
    public int id;

    public string name;
    public string description;

    public List<Ability> abilities;
  }
}