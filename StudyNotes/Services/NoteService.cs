using Microsoft.EntityFrameworkCore;
using StudyNotes.Data;
using StudyNotes.Data.Models;

namespace StudyNotes.Services;

public class NoteService
{
    private readonly IDbContextFactory<ApplicationDbContext> _factory;

    public NoteService(IDbContextFactory<ApplicationDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<List<Note>> GetAllNotesAsync(string? searchTerm = null, int? subjectId = null)
    {
        using var context = _factory.CreateDbContext();
        var query = context.Notes.Include(n => n.Subject).AsQueryable();

        if (subjectId.HasValue && subjectId.Value > 0)
            query = query.Where(n => n.SubjectId == subjectId.Value);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(n =>
                n.Title.ToLower().Contains(term) ||
                n.Content.ToLower().Contains(term) ||
                n.Tags.ToLower().Contains(term) ||
                (n.Subject != null && n.Subject.Name.ToLower().Contains(term)));
        }

        return await query.OrderByDescending(n => n.UpdatedAt).ToListAsync();
    }

    public async Task<Note?> GetNoteByIdAsync(int id)
    {
        using var context = _factory.CreateDbContext();
        return await context.Notes.Include(n => n.Subject).FirstOrDefaultAsync(n => n.Id == id);
    }

    public async Task<List<Note>> GetPinnedNotesAsync()
    {
        using var context = _factory.CreateDbContext();
        return await context.Notes.Include(n => n.Subject).Where(n => n.IsPinned).OrderByDescending(n => n.UpdatedAt).ToListAsync();
    }

    public async Task<List<Note>> GetFavoriteNotesAsync()
    {
        using var context = _factory.CreateDbContext();
        return await context.Notes.Include(n => n.Subject).Where(n => n.IsFavorite).OrderByDescending(n => n.UpdatedAt).ToListAsync();
    }

    public async Task SaveNoteAsync(Note note)
    {
        using var context = _factory.CreateDbContext();
        note.UpdatedAt = DateTime.UtcNow;

        if (note.Id == 0)
        {
            note.CreatedAt = DateTime.UtcNow;
            context.Notes.Add(note);
        }
        else
        {
            context.Notes.Update(note);
        }

        await context.SaveChangesAsync();
    }

    public async Task TogglePinAsync(int id)
    {
        using var context = _factory.CreateDbContext();
        var note = await context.Notes.FindAsync(id);
        if (note != null)
        {
            note.IsPinned = !note.IsPinned;
            await context.SaveChangesAsync();
        }
    }

    public async Task ToggleFavoriteAsync(int id)
    {
        using var context = _factory.CreateDbContext();
        var note = await context.Notes.FindAsync(id);
        if (note != null)
        {
            note.IsFavorite = !note.IsFavorite;
            await context.SaveChangesAsync();
        }
    }

    public async Task DeleteNoteAsync(int id)
    {
        using var context = _factory.CreateDbContext();
        var note = await context.Notes.FindAsync(id);
        if (note != null)
        {
            context.Notes.Remove(note);
            await context.SaveChangesAsync();
        }
    }
}