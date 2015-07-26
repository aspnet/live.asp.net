using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using live.asp.net.Models;
using Microsoft.Data.Entity;

namespace live.asp.net.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<LiveShowDetails> LiveShowDetails { get; set; }
    }
}
