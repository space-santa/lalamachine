using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LalaDb.Data
{
    public class LalaContextFactory : IDesignTimeDbContextFactory<LalaContext>
    {
        public LalaContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<LalaContext>();
            optionsBuilder.UseSqlite("Data Source=dummy.db");
            return new LalaContext(optionsBuilder.Options);
        }
    }
}
