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
        //public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<ChatLog> ChatLogs { get; set; }
        public DbSet<DocumentSaving> DocumentSavings { get; set; }
        public DbSet<AnalyzeResponse> AnalyzeResponses { get; set; }
        public DbSet<ClauseResult> ClauseResults { get; set; }
        public DbSet<Evidence> Evidences { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            var entityTypes = builder.Model.GetEntityTypes();

            // Fix: Ngăn xung đột nhiều cascade delete
            builder.Entity<DocumentSaving>()
                .HasOne(ds => ds.Document)
                .WithMany()
                .HasForeignKey(ds => ds.DocumentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<DocumentSaving>()
                .HasOne(ds => ds.User)
                .WithMany()
                .HasForeignKey(ds => ds.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            //builder.Entity<AnalyzeResponse>()
            //    .HasOne(ar => ar.Document)
            //    .WithMany()
            //    .HasForeignKey(ar => ar.DocumentId)
            //    .OnDelete(DeleteBehavior.Restrict);

            //// Nếu ClauseResult có FK đến AnalyzeResponse thì cũng cấu hình tương tự
            //builder.Entity<ClauseResult>()
            //    .HasOne(c => c.AnalyzeResponse)
            //    .WithMany(a => a.ClauseResults)
            //    .HasForeignKey(c => c.AnalyzeResponseId)
            //    .OnDelete(DeleteBehavior.Restrict);

            SeedRoles(builder);
        }

        private static void SeedRoles(ModelBuilder builder)
        {
            builder.Entity<IdentityRole>().HasData(
                new IdentityRole() { Id = "a673eb1a-c122-44a5-86a3-4a990e8b5404", Name = "Admin", ConcurrencyStamp = "1", NormalizedName = "Admin" },
                new IdentityRole() { Id = "d7f0f9f9-846b-4e57-86d0-21f048ad3246", Name = "User", ConcurrencyStamp = "2", NormalizedName = "User" }
                );

            builder.Entity<Category>().HasData(
                new Category { Id = 1, CategoryName = "Sổ tay chất lượng", Description = "" },
                new Category { Id = 2, CategoryName = "Chính sách chất lượng", Description = "" },
                new Category { Id = 3, CategoryName = "Mục tiêu chất lượng", Description = "" },
                new Category { Id = 4, CategoryName = "Kế hoạch chất lượng", Description = "" },
                new Category { Id = 5, CategoryName = "Hướng dẫn công việc", Description = "" },
                new Category { Id = 6, CategoryName = "Biểu mẫu", Description = "" },
                new Category { Id = 7, CategoryName = "Hồ sơ", Description = "" },
                new Category { Id = 8, CategoryName = "Thủ tục", Description = "" }
            );


        }
    }
}
