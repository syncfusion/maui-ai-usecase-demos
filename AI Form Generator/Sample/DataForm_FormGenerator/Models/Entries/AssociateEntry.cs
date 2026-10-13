using System.ComponentModel.DataAnnotations;
using Syncfusion.Maui.DataForm;

namespace DataForm_FormGenerator;

/// <summary>
/// Statically-typed POCO rendered by SfDataForm for the Associate Degree
/// template.
/// </summary>
public sealed class AssociateEntry
{
    [Display(Name = "College Name")]
    [Required(ErrorMessage = "College Name is required.")]
    [StringLength(100, MinimumLength = 2)]
    public string? CollegeName { get; set; }

    [Display(Name = "Degree Program")]
    [Required(ErrorMessage = "Degree Program is required.")]
    [StringLength(120, MinimumLength = 2)]
    public string? DegreeProgram { get; set; }

    [Display(Name = "Major")]
    [Required(ErrorMessage = "Major is required.")]
    [StringLength(80, MinimumLength = 2)]
    public string? Major { get; set; }

    [Display(Name = "Credits Completed")]
    [Required(ErrorMessage = "Credits Completed is required.")]
    [Range(0, 300)]
    public int? CreditsCompleted { get; set; }

    [Display(Name = "GPA")]
    [Required(ErrorMessage = "GPA is required.")]
    [Range(1.0, 10.0)]
    public double? GPA { get; set; }

    [Display(Name = "Graduation Date")]
    [DataType(DataType.Date)]
    public DateTime? GraduationDate { get; set; }
}