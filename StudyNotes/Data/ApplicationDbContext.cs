using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using StudyNotes.Data.Models;

namespace StudyNotes.Data;

public class ApplicationDbContext : DbContext
{
    private static readonly ValueConverter<DateTime, DateTime> UtcDateTimeConverter =
        new(v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

    private static readonly ValueConverter<DateTime?, DateTime?> UtcNullableDateTimeConverter =
        new(v => v.HasValue ? (v.Value.Kind == DateTimeKind.Utc ? v.Value : v.Value.ToUniversalTime()) : v,
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Note> Notes => Set<Note>();
    public DbSet<Subject> Subjects => Set<Subject>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        foreach (var property in modelBuilder.Model.GetEntityTypes()
            .SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == typeof(DateTime) || p.ClrType == typeof(DateTime?)))
        {
            property.SetValueConverter(property.ClrType == typeof(DateTime)
                ? UtcDateTimeConverter
                : UtcNullableDateTimeConverter);
        }

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
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-2)
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
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            }
        );
    }
}