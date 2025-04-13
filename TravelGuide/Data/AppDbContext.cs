using Microsoft.EntityFrameworkCore;
using TravelGuide.Model;
using TravelGuide.Model.Location;

namespace TravelGuide.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<UserModel> Users { get; set; }


    }
}
