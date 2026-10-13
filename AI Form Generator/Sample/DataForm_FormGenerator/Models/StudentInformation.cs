using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace DataForm_FormGenerator;

/// <summary>
/// POCO for the always-visible "Student Information" section. Lightweight
/// INotifyPropertyChanged implementation directly on this class so the
/// bound text fields update the master VM when the user edits them.
/// </summary>
public sealed class StudentInformation : INotifyPropertyChanged
{
    private string? fullLegalName;
    private string? emailAddress;
    private string? mobileCountryCode = "+1";
    private string? mobileNumber;
    private string? nationality;
    private string? gender;
    private DateTime? dateOfBirth;

    public static readonly IReadOnlyList<string> DefaultCountryCodes = new[]
    {
        "+1", "+44", "+91", "+61", "+49", "+33", "+81", "+86", "+971", "+966"
    };

    public static readonly IReadOnlyList<string> DefaultNationalities = new[]
    {
        "United States", "Canada", "United Kingdom", "Australia", "India",
        "Germany", "France", "Japan", "China", "United Arab Emirates"
    };

    public static readonly IReadOnlyList<string> DefaultGenders = new[]
    {
        "Female", "Male", "Non-binary", "Prefer not to say"
    };

    [Display(Name = "Full Legal Name")]
    [Required(ErrorMessage = "Full legal name is required.")]
    public string? FullLegalName
    {
        get => this.fullLegalName;
        set => SetField(ref this.fullLegalName, value);
    }

    [Display(Name = "Email Address")]
    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public string? EmailAddress
    {
        get => this.emailAddress;
        set => SetField(ref this.emailAddress, value);
    }

    [Display(Name = "Mobile Number")]
    [Required(ErrorMessage = "Mobile number is required.")]
    [Phone(ErrorMessage = "Please enter a valid phone number.")]
    public string? MobileNumber
    {
        get => this.mobileNumber;
        set => SetField(ref this.mobileNumber, value);
    }

    [Display(Name = "Nationality")]
    [Required(ErrorMessage = "Nationality is required.")]
    public string? Nationality
    {
        get => this.nationality;
        set => SetField(ref this.nationality, value);
    }

    [Display(Name = "Gender")]
    [Required(ErrorMessage = "Gender is required.")]
    public string? Gender
    {
        get => this.gender;
        set => SetField(ref this.gender, value);
    }

    [Display(Name = "Date of Birth")]
    [DataType(DataType.Date)]
    [Required(ErrorMessage = "Date of birth is required.")]
    public DateTime? DateOfBirth
    {
        get => this.dateOfBirth;
        set => SetField(ref this.dateOfBirth, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (!EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}