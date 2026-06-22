using Microsoft.EntityFrameworkCore;
using Shortenkai.Models;

namespace Shortenkai.Database
{
    public class ShortenkaiUrlDb : DbContext
    {
        public ShortenkaiUrlDb(DbContextOptions<ShortenkaiUrlDb> options) : base(options) {}

        public DbSet<ShortenkaiUrl> Urls => Set<ShortenkaiUrl>();
    }
}
