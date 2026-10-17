using System.ComponentModel.DataAnnotations;

namespace DataForm_FormGenerator;

/// <summary>
/// Statically-typed POCO rendered by SfDataForm for the Master's Degree
/// template. Mirrors EducationFormCatalog.
/// </summary>
public sealed class MasterEntry
{
    [Display(Name = "University Name")]
    [Required(ErrorMessage = "University Name is required.")]
    [StringLength(100, MinimumLength = 2)]
    public string? UniversityName { get; set; }

    [Display(Name = "Degree Title")]
    [Required(ErrorMessage = "Degree Title is required.")]
    [StringLength(80, MinimumLength = 2)]
    public string? DegreeTitle { get; set; }

    [Display(Name = "Specialization")]
    [StringLength(120, MinimumLength = 2)]
    public string? Specialization { get; set; }

    [Display(Name = "Research Area")]
    [StringLength(120, MinimumLength = 2)]
    public string? ResearchArea { get; set; }

    [Display(Name = "Graduation Year")]
    [Required(ErrorMessage = "Graduation Year is required.")]
    [Range(1900, 2030)]
    public int? GraduationYear { get; set; }

    [Display(Name = "GPA")]
    [Required(ErrorMessage = "GPA is required.")]
    [Range(1.0, 10.0)]
    public double? GPA { get; set; }
}