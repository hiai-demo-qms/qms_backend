using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using WebApplication1.Domain.Entities;

namespace WebApplication1.Infrastructure.Data
{
    public class QmsDbContext : IdentityDbContext<User>
    {
        public QmsDbContext(DbContextOptions<QmsDbContext> options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<ChatLog> ChatLogs { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            //builder.ApplyConfigurationsFromAssembly(typeof(QmsDbContext).Assembly);
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            var entityTypes = builder.Model.GetEntityTypes();

            SeedRoles(builder);
        }

        private static void SeedRoles(ModelBuilder builder)
        {
            //builder.Entity<IdentityRole>().HasData(
            //    new IdentityRole() { Name = "Admin", ConcurrencyStamp = "1", NormalizedName = "Admin" },
            //    new IdentityRole() { Name = "User", ConcurrencyStamp = "2", NormalizedName = "User" }
            //    );

            builder.Entity<Category>().HasData(
                new Category { Id = 1, CategoryName = "Quality Management", Description = "Category related to quality management systems" },
                new Category { Id = 2, CategoryName = "Risk Management", Description = "Category related to identifying and managing risks" },
                new Category { Id = 3, CategoryName = "Auditing", Description = "Category related to audit processes and procedures" },
                new Category { Id = 4, CategoryName = "Documentation", Description = "Category related to document control and records" },
                new Category { Id = 5, CategoryName = "Management", Description = "Category for general management practices and policies" },
                new Category { Id = 6, CategoryName = "Improvement", Description = "Category for continuous improvement and corrective actions" },
                new Category { Id = 7, CategoryName = "Environmental", Description = "Category for environmental management and compliance" },
                new Category { Id = 8, CategoryName = "Security", Description = "Category related to information and physical security" }
            );
        }
    }
}
