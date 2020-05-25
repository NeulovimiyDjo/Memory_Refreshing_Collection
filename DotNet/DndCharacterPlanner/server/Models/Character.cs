using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Models
{
  public class Character
  {
    [Key]
    public long Id { get; set; }

    public string Config { get; set; }
  }
}