using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SmartVehicleCare.Models;

namespace SmartVehicleCare.ViewModels;

public class AddScheduleViewModel : INotifyPropertyChanged
{
    // ── Vehicle context ───────────────────────────────────────────────────────

    private string _vehicleLabel = string.Empty;
    public string VehicleLabel
    {
        get => _vehicleLabel;
        set => SetProperty(ref _vehicleLabel, value);
    }

    // ── Reminder Type ─────────────────────────────────────────────────────────

    private string _selectedType = string.Empty;
    public string SelectedType
    {
        get => _selectedType;
        set
        {
            if (SetProperty(ref _selectedType, value))
            {
                OnPropertyChanged(nameof(IsOilChangeSelected));
                OnPropertyChanged(nameof(IsTyreRotationSelected));
                OnPropertyChanged(nameof(IsGeneralServiceSelected));
                OnPropertyChanged(nameof(IsCustomSelected));
                Title = value == "Custom" ? string.Empty : value;
                NotifyValidation();
            }
        }
    }

    public bool IsOilChangeSelected      => SelectedType == "Oil & Filter Change";
    public bool IsTyreRotationSelected   => SelectedType == "Tyre & Wheel Service";
    public bool IsGeneralServiceSelected => SelectedType == "General Service";
    public bool IsCustomSelected         => SelectedType == "Custom";

    // ── Title ─────────────────────────────────────────────────────────────────

    private string _title = string.Empty;
    public string Title
    {
        get => _title;
        set { if (SetProperty(ref _title, value)) NotifyValidation(); }
    }

    // ── Due Date ──────────────────────────────────────────────────────────────

    private bool _isDueDateSet;
    private DateTime _dueDate = DateTime.Today.AddDays(7);
    public DateTime DueDate
    {
        get => _dueDate;
        set
        {
            if (SetProperty(ref _dueDate, value))
            {
                _isDueDateSet = true;
                OnPropertyChanged(nameof(DueDateDisplay));
                NotifyValidation();
            }
        }
    }
    public string DueDateDisplay => _isDueDateSet ? _dueDate.ToString("dd MMM yyyy") : string.Empty;

    // ── Due Time ──────────────────────────────────────────────────────────────

    private bool _isDueTimeSet;
    private TimeSpan _dueTime = new(10, 0, 0);
    public TimeSpan DueTime
    {
        get => _dueTime;
        set
        {
            if (SetProperty(ref _dueTime, value))
            {
                _isDueTimeSet = true;
                OnPropertyChanged(nameof(DueTimeDisplay));
            }
        }
    }
    public string DueTimeDisplay => _isDueTimeSet
        ? DateTime.Today.Add(_dueTime).ToString("hh:mm tt")
        : string.Empty;

    // ── Priority ──────────────────────────────────────────────────────────────

    private string _selectedPriority = "High";
    public string SelectedPriority
    {
        get => _selectedPriority;
        set
        {
            if (SetProperty(ref _selectedPriority, value))
            {
                OnPropertyChanged(nameof(IsCriticalSelected));
                OnPropertyChanged(nameof(IsHighPrioritySelected));
                OnPropertyChanged(nameof(IsMediumSelected));
                OnPropertyChanged(nameof(IsLowSelected));
            }
        }
    }
    public bool IsCriticalSelected     => SelectedPriority == "Critical";
    public bool IsHighPrioritySelected => SelectedPriority == "High";
    public bool IsMediumSelected       => SelectedPriority == "Medium";
    public bool IsLowSelected          => SelectedPriority == "Low";

    // ── Remind Before ─────────────────────────────────────────────────────────

    private int _remindBeforeDays = 7;
    public int RemindBeforeDays
    {
        get => _remindBeforeDays;
        set
        {
            if (SetProperty(ref _remindBeforeDays, value))
            {
                OnPropertyChanged(nameof(Is1DaySelected));
                OnPropertyChanged(nameof(Is3DaysSelected));
                OnPropertyChanged(nameof(Is7DaysSelected));
                OnPropertyChanged(nameof(Is30DaysSelected));
            }
        }
    }
    public bool Is1DaySelected   => RemindBeforeDays == 1;
    public bool Is3DaysSelected  => RemindBeforeDays == 3;
    public bool Is7DaysSelected  => RemindBeforeDays == 7;
    public bool Is30DaysSelected => RemindBeforeDays == 30;

    // ── Repeat ───────────────────────────────────────────────────────────────

    private bool _isRepeat;
    public bool IsRepeat
    {
        get => _isRepeat;
        set => SetProperty(ref _isRepeat, value);
    }

    // ── Validation ────────────────────────────────────────────────────────────

    public bool CanSave =>
        !string.IsNullOrWhiteSpace(SelectedType) &&
        !string.IsNullOrWhiteSpace(Title) &&
        _isDueDateSet;

    private void NotifyValidation()
    {
        OnPropertyChanged(nameof(CanSave));
        // Command.CanExecute must be refreshed explicitly; PropertyChanged alone is not enough
        ((Command)SaveCommand).ChangeCanExecute();
    }

    // ── Commands ──────────────────────────────────────────────────────────────

    public ICommand SelectTypeCommand        { get; }
    public ICommand SelectPriorityCommand    { get; }
    public ICommand SelectRemindBeforeCommand { get; }
    public ICommand SaveCommand              { get; }
    public ICommand CloseCommand             { get; }

    // ── Events ───────────────────────────────────────────────────────────────

    public event Action<ScheduleReminder>? OnScheduleSaved;
    public event Action? OnCloseRequested;

    // ── Constructor ───────────────────────────────────────────────────────────

    public AddScheduleViewModel()
    {
        SelectTypeCommand        = new Command<string>(t => SelectedType = t);
        SelectPriorityCommand    = new Command<string>(p => SelectedPriority = p);
        SelectRemindBeforeCommand = new Command<string>(d =>
        {
            if (int.TryParse(d, out var days)) RemindBeforeDays = days;
        });

        SaveCommand = new Command(() =>
        {
            if (!CanSave) return;
            OnScheduleSaved?.Invoke(Build());
            Reset();
        }, () => CanSave);

        CloseCommand = new Command(() => OnCloseRequested?.Invoke());
    }

    private ScheduleReminder Build() => new()
    {
        ReminderType    = SelectedType,
        Title           = Title,
        DueDate         = _dueDate,
        DueTime         = _dueTime,
        Priority        = SelectedPriority,
        RemindBeforeDays = RemindBeforeDays,
        IsRepeat        = IsRepeat,
    };

    public void Reset()
    {
        _vehicleLabel     = string.Empty;
        _selectedType     = string.Empty;
        _title            = string.Empty;
        _isDueDateSet     = false;
        _dueDate          = DateTime.Today.AddDays(7);
        _isDueTimeSet     = false;
        _dueTime          = new TimeSpan(10, 0, 0);
        _selectedPriority = "High";
        _remindBeforeDays = 7;
        _isRepeat         = false;

        foreach (var p in new[]
        {
            nameof(VehicleLabel), nameof(SelectedType), nameof(Title),
            nameof(DueDate), nameof(DueDateDisplay),
            nameof(DueTime), nameof(DueTimeDisplay),
            nameof(SelectedPriority), nameof(RemindBeforeDays), nameof(IsRepeat),
            nameof(CanSave),
            nameof(IsOilChangeSelected), nameof(IsTyreRotationSelected),
            nameof(IsGeneralServiceSelected), nameof(IsCustomSelected),
            nameof(IsCriticalSelected), nameof(IsHighPrioritySelected),
            nameof(IsMediumSelected), nameof(IsLowSelected),
            nameof(Is1DaySelected), nameof(Is3DaysSelected),
            nameof(Is7DaysSelected), nameof(Is30DaysSelected),
        })
            OnPropertyChanged(p);
        ((Command)SaveCommand).ChangeCanExecute();
    }

    // ── INotifyPropertyChanged ────────────────────────────────────────────────

    public event PropertyChangedEventHandler? PropertyChanged;

    protected bool SetProperty<T>(ref T store, T value, [CallerMemberName] string prop = "")
    {
        if (EqualityComparer<T>.Default.Equals(store, value)) return false;
        store = value;
        OnPropertyChanged(prop);
        return true;
    }

    protected void OnPropertyChanged([CallerMemberName] string prop = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
}
