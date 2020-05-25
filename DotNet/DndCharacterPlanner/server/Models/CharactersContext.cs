using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace server.Models
{
  public class CharactersContext : DbContext
  {
    public DbSet<Character> Characters { get; set; }

    public CharactersContext(DbContextOptions<CharactersContext> options) : base(options)
    {
      Database.EnsureCreated();
    }
  }
}