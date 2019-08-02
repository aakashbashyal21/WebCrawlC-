using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebCrawling.Models;

namespace WebCrawling.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
           : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<EntryModel> EntryModels { get; set; }
        public DbSet<NewsArticle> NewsArticles { get; set; }
        public DbSet<CrawlFrequency> CrawlFrequency { get; set; }

    }
    public class ScrapContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<AppDbContext>();
            builder.UseSqlServer("Server=localhost;Database=NewsPortalScrapp;Trusted_Connection=True;MultipleActiveResultSets=true");
            return new AppDbContext(builder.Options);
        }
    }
}
