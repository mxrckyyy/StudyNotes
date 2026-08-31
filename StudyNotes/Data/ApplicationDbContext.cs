using Microsoft.EntityFrameworkCore;
using StudyNotes.Data.Models;
using System.Reflection.Emit;

namespace StudyNotes.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Note> Notes => Set<Note>();
    public DbSet<Subject> Subjects => Set<Subject>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Seed Default Subjects
        modelBuilder.Entity<Subject>().HasData(
            new Subject { Id = 1, Name = "Programming" },
            new Subject { Id = 2, Name = "Database" },
            new Subject { Id = 3, Name = "Web Development" },
            new Subject { Id = 4, Name = "Networking" },
            new Subject { Id = 5, Name = "Mathematics" }
        );

        // Seed Default Notes
        modelBuilder.Entity<Note>().HasData(
            new Note
            {
                Id = 1,
                Title = "Introduction to C#",
                Content = "C# is a modern, object-oriented programming language developed by Microsoft.",
                SubjectId = 1,
                Tags = "#reviewer #csharp",
                IsPinned = true,
                IsFavorite = true,
                CreatedAt = DateTime.Now.AddDays(-2),
                UpdatedAt = DateTime.Now.AddDays(-2)
            },
            new Note
            {
                Id = 2,
                Title = "Database Normalization",
                Content = "1NF requires atomic values. 2NF removes partial dependencies. 3NF removes transitive dependencies.",
                SubjectId = 2,
                Tags = "#exam #db",
                IsPinned = true,
                IsFavorite = false,
                CreatedAt = DateTime.Now.AddDays(-1),
                UpdatedAt = DateTime.Now.AddDays(-1)
            }
        );
    }
}