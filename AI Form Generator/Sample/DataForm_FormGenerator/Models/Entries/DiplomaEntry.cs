using System.ComponentModel.DataAnnotations;
using Syncfusion.Maui.DataForm;

namespace DataForm_FormGenerator;

/// <summary>
/// Statically-typed POCO rendered by SfDataForm for the Diploma template.
/// </summary>
public sealed class DiplomaEntry
{
    [Display(Name = "Institution Name")]
    [Required(ErrorMessage = "Institution Name is required.")]
    [StringLength(100, MinimumLength = 2)]
    public string? InstitutionName { get; set; }

    [Display(Name = "Program Name")]
    [Required(ErrorMessage = "Program Name is required.")]
    [StringLength(120, MinimumLength = 2)]
    public string? ProgramName { get; set; }

    [Display(Name = "Specialization")]
    [StringLength(120, MinimumLength = 2)]
    public string? Specialization { get; set; }

    [Display(Name = "Duration")]
    [Required(ErrorMessage = "Duration is required.")]
    [StringLength(40, MinimumLength = 1)]
    public string? Duration { get; set; }

    [Display(Name = "Graduation Year")]
    [Required(ErrorMessage = "Graduation Year is required.")]
    [Range(1900, 2030)]
    public int? GraduationYear { get; set; }

    [Display(Name = "Accreditation Status")]
    public string? AccreditationStatus { get; set; }
}