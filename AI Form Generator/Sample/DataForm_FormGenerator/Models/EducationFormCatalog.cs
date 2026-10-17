namespace DataForm_FormGenerator;

/// <summary>
/// Single source of truth for the supported education types and their
/// generated field sets. The mock and the Azure OpenAI service both
/// delegate here so the rendered form stays identical regardless of
/// which backend is active.
/// </summary>
public static class EducationFormCatalog
{
    public const string HighSchool = "High School";
    public const string Diploma = "Diploma";
    public const string AssociateDegree = "Associate Degree";
    public const string BachelorsDegree = "Bachelor's Degree";
    public const string MastersDegree = "Master's Degree";

    public static readonly IReadOnlyList<string> Options = new[]
    {
        HighSchool,
        Diploma,
        AssociateDegree,
        BachelorsDegree,
        MastersDegree,
    };

    /// <summary>
    /// Returns the schema for <paramref name="educationType"/> if it
    /// matches one of the catalog options (case-/whitespace-insensitive).
    /// Throws <see cref="ArgumentException"/> otherwise.
    /// </summary>
    public static EducationFormSchema Build(string educationType)
    {
        var normalized = (educationType ?? string.Empty).Trim().ToLowerInvariant();
        var normalizedNoApostrophe = normalized.Replace("'", string.Empty);

        if (normalized == HighSchool.ToLowerInvariant() || normalizedNoApostrophe == "highschool")
        {
            return new EducationFormSchema
            {
                Title = "High School",
                Fields =
                [
                    Field("SchoolName",      "School Name",      "string",   required: true,  minLength: 2, maxLength: 80),
                    Field("SchoolDistrict",  "School District",  "string",   required: false, minLength: 2, maxLength: 100),
                    Field("State",           "State",            "dropdown", required: false, options: ["California (CA)", "Texas (TX)", "New York (NY)", "Washington (WA)"]),
                    Field("GraduationYear",  "Graduation Year",  "number",   required: true,  min: 1900, max: 2030),
                    Field("GPA",             "GPA",              "decimal",  required: true,  min: 1,    max: 10),
                    Field("SATScore",        "SAT Score",        "number",   required: false, min: 400,  max: 1600),
                    Field("ACTScore",        "ACT Score",        "number",   required: false, min: 1,    max: 36),
                ]
            };
        }

        if (normalized == Diploma.ToLowerInvariant())
        {
            return new EducationFormSchema
            {
                Title = "Diploma",
                Fields =
                [
                    Field("InstitutionName",     "Institution Name",     "string",   required: true,  minLength: 2, maxLength: 100),
                    Field("ProgramName",         "Program Name",         "string",   required: true,  minLength: 2, maxLength: 120),
                    Field("Specialization",      "Specialization",       "string",   required: false, minLength: 2, maxLength: 120),
                    Field("Duration",            "Duration",             "string",   required: true,  minLength: 1, maxLength: 40, defaultValue: "2 Years"),
                    Field("GraduationYear",      "Graduation Year",      "number",   required: true,  min: 1900, max: 2030),
                    Field("AccreditationStatus", "Accreditation Status", "dropdown", required: false, options: ["Accredited", "Pending", "Not Accredited"]),
                ]
            };
        }

        if (normalized == AssociateDegree.ToLowerInvariant() || normalizedNoApostrophe == "associate")
        {
            return new EducationFormSchema
            {
                Title = "Associate Degree",
                Fields =
                [
                    Field("CollegeName",      "College Name",      "string",   required: true,  minLength: 2, maxLength: 100),
                    Field("DegreeProgram",    "Degree Program",    "string",   required: true,  minLength: 2, maxLength: 120),
                    Field("Major",            "Major",             "string",   required: true,  minLength: 2, maxLength: 80),
                    Field("CreditsCompleted", "Credits Completed", "number",   required: true,  min: 0, max: 300),
                    Field("GPA",              "GPA",               "decimal",  required: true,  min: 1, max: 10),
                    Field("GraduationDate",   "Graduation Date",   "date",     required: false),
                ]
            };
        }

        if (normalized == BachelorsDegree.ToLowerInvariant() || normalizedNoApostrophe == "bachelors")
        {
            return new EducationFormSchema
            {
                Title = "Bachelor's Degree",
                Fields =
                [
                    Field("UniversityName",  "University Name",  "string",   required: true,  minLength: 2, maxLength: 100),
                    Field("DegreeTitle",     "Degree Title",     "string",   required: true,  minLength: 2, maxLength: 80),
                    Field("Major",           "Major",            "string",   required: true,  minLength: 2, maxLength: 80),
                    Field("GraduationYear",  "Graduation Year",  "number",   required: true,  min: 1900,   max: 2030),
                    Field("GPA",             "GPA",              "decimal",  required: true,  min: 1,      max: 10),
                    Field("Country",         "Country",          "dropdown", required: false, options: ["United States", "Canada", "United Kingdom", "Australia", "India"]),
                ]
            };
        }

        if (normalized == MastersDegree.ToLowerInvariant() || normalizedNoApostrophe == "masters")
        {
            return new EducationFormSchema
            {
                Title = "Master's Degree Education",
                Fields =
                [
                    Field("UniversityName",  "University Name",  "string",   required: true,  minLength: 2, maxLength: 100),
                    Field("DegreeTitle",     "Degree Title",     "string",   required: true,  minLength: 2, maxLength: 80),
                    Field("Specialization",  "Specialization",   "string",   required: false, minLength: 2, maxLength: 120),
                    Field("ResearchArea",    "Research Area",    "string",   required: false, minLength: 2, maxLength: 120),
                    Field("GraduationYear",  "Graduation Year",  "number",   required: true,  min: 1900,   max: 2030),
                    Field("GPA",             "GPA",              "decimal",  required: true,  min: 1,      max: 10),
                ]
            };
        }

        throw new ArgumentException(
            $"Unsupported education type: '{educationType}'. Allowed values: {string.Join(", ", Options)}.",
            nameof(educationType));
    }

    /// <summary>
    /// True if <paramref name="educationType"/> matches one of the
    /// catalog options (case-/whitespace-insensitive).
    /// </summary>
    public static bool IsKnown(string educationType)
    {
        var normalized = (educationType ?? string.Empty).Trim().ToLowerInvariant();
        var normalizedNoApostrophe = normalized.Replace("'", string.Empty);
        return normalized == HighSchool.ToLowerInvariant()
            || normalizedNoApostrophe == "highschool"
            || normalized == Diploma.ToLowerInvariant()
            || normalized == AssociateDegree.ToLowerInvariant()
            || normalizedNoApostrophe == "associate"
            || normalized == BachelorsDegree.ToLowerInvariant()
            || normalizedNoApostrophe == "bachelors"
            || normalized == MastersDegree.ToLowerInvariant()
            || normalizedNoApostrophe == "masters";
    }

    private static EducationFieldDefinition Field(
        string name,
        string label,
        string type,
        bool required,
        decimal? min = null,
        decimal? max = null,
        int? minLength = null,
        int? maxLength = null,
        string? defaultValue = null,
        List<string>? options = null)
        => new()
        {
            Name = name,
            Label = label,
            Type = type,
            Required = required,
            Min = min,
            Max = max,
            MinLength = minLength,
            MaxLength = maxLength,
            DefaultValue = defaultValue,
            Options = options,
        };
}