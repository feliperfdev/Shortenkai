using Microsoft.EntityFrameworkCore;
using Shortenkai.Domain.Models;

namespace Shortenkai.Infrastructure.Database
{
    public class ShortenkaiUrlDb : DbContext
    {
        public ShortenkaiUrlDb(DbContextOptions<ShortenkaiUrlDb> options) : base(options) {}

        public DbSet<ShortenkaiUrl> Urls => Set<ShortenkaiUrl>();
    }
}
