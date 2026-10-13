using Syncfusion.Maui.DataForm;
using System.ComponentModel.DataAnnotations;

namespace DataForm_FormGenerator;

/// <summary>
/// Statically-typed POCO rendered by SfDataForm for the High School (USA)
/// template. Mirrors EducationFormCatalog.
/// </summary>
public sealed class HighSchoolEntry
{
    [Display(Name = "School Name")]
    [Required(ErrorMessage = "School Name is required.")]
    [StringLength(80, MinimumLength = 2)]
    public string? SchoolName { get; set; }

    [Display(Name = "School District")]
    [StringLength(100, MinimumLength = 2)]
    public string? SchoolDistrict { get; set; }

    [Display(Name = "Graduation Year")]
    [Required(ErrorMessage = "Graduation Year is required.")]
    [Range(1900, 2030)]
    public int? GraduationYear { get; set; }

    [Display(Name = "GPA")]
    [Required(ErrorMessage = "GPA is required.")]
    [Range(1.0, 10.0)]
    public double? GPA { get; set; }

    [Display(Name = "State")]
    public string? State { get; set; }

    [Display(Name = "SAT Score")]
    [Range(400, 1600, ErrorMessage = "SAT Score must be 400..1600.")]
    public int? SATScore { get; set; }

    [Display(Name = "ACT Score")]
    [Range(1, 36, ErrorMessage = "ACT Score must be 1..36.")]
    public int? ACTScore { get; set; }
}