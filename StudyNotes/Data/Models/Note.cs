using System.ComponentModel.DataAnnotations;

namespace StudyNotes.Data.Models;

public class Note
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Content is required.")]
    public string Content { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Please select a subject.")]
    public int SubjectId { get; set; }

    public Subject? Subject { get; set; }

    public string Tags { get; set; } = string.Empty;

    public bool IsPinned { get; set; }
    public bool IsFavorite { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}