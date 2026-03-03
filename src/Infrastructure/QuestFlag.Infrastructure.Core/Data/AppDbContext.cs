using Microsoft.EntityFrameworkCore;

namespace QuestFlag.Infrastructure.Core.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
}
