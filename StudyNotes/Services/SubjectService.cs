using Microsoft.EntityFrameworkCore;
using StudyNotes.Data;
using StudyNotes.Data.Models;

namespace StudyNotes.Services;

public class SubjectService
{
    private readonly IDbContextFactory<ApplicationDbContext> _factory;

    public SubjectService(IDbContextFactory<ApplicationDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<List<Subject>> GetAllSubjectsAsync()
    {
        using var context = _factory.CreateDbContext();
        return await context.Subjects.Include(s => s.Notes).ToListAsync();
    }

    public async Task SaveSubjectAsync(Subject subject)
    {
        using var context = _factory.CreateDbContext();
        if (subject.Id == 0)
        {
            context.Subjects.Add(subject);
        }
        else
        {
            context.Subjects.Update(subject);
        }
        await context.SaveChangesAsync();
    }

    public async Task DeleteSubjectAsync(int id)
    {
        using var context = _factory.CreateDbContext();
        var subject = await context.Subjects.FindAsync(id);
        if (subject != null)
        {
            context.Subjects.Remove(subject);
            await context.SaveChangesAsync();
        }
    }
}