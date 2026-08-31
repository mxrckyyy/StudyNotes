using System.ComponentModel.DataAnnotations;

namespace StudyNotes.Data.Models;

public class Subject
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Subject name is required.")]
    [StringLength(50, ErrorMessage = "Subject name cannot exceed 50 characters.")]
    public string Name { get; set; } = string.Empty;

    public List<Note> Notes { get; set; } = new();
}