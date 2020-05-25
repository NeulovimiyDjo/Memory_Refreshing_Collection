using System;
using System.Collections.Generic;
using System.Linq;
using server.Models;

namespace server.Services
{
  public class DndDatabase
  {
    public string Data { get; }


    public DndDatabase()
    {   
      string appDir = System.IO.Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName;
      string dataDir;
      if (System.IO.Directory.Exists(appDir + "/Data")) // published
      {
        dataDir = appDir + "/Data";
      }
      else // build
      {
        dataDir = appDir + "/../../../Data";
      }


      Data = System.IO.File.ReadAllText(dataDir + "/database.json");
    }
  }
}