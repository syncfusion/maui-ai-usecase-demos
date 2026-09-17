using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PatientCompanion.Models;
using PatientCompanion.Views;
using System;
using System.Collections.ObjectModel;

namespace PatientCompanion.ViewModels;

public partial class AppointmentDateTimeViewModel : ObservableObject
{
    private readonly AppointmentViewModel? appointmentViewModel;

    [ObservableProperty]
    private Doctor? selectedDoctor;

    [ObservableProperty]
    private string? selectedVisitType;

    [ObservableProperty]
    private DateTime selectedDate = DateTime.Today;

    [ObservableProperty]
    private string? selectedTimeSlot;

    public ObservableCollection<string> VisitTypes { get; } =
    [
        "Clinic Visit",
        "Video Visit"
    ];

    public ObservableCollection<string> AvailableTimeSlots { get; } = [];

    public ObservableCollection<AppointmentDateOption> DateOptions { get; } = [];

    public ObservableCollection<AppointmentTimeOption> TimeOptions { get; } = [];

    public ObservableCollection<string> MonthOptions { get; } = [];

    public DateTime MinimumDate => DateTime.Today;

    public DateTime MaximumDate => DateTime.Today.AddMonths(12);

    public string SelectedMonthText => SelectedDate.ToString("MMMM yyyy");

    public AppointmentDateTimeViewModel()
    {
        GenerateDateOptions();
        GenerateTimeSlots();
        GenerateMonthOptions();
    }

    public AppointmentDateTimeViewModel(AppointmentViewModel appointmentVm)
    {
        appointmentViewModel = appointmentVm;

        SelectedDoctor = appointmentVm.SelectedDoctor;
        SelectedVisitType = string.IsNullOrWhiteSpace(appointmentVm.Booking.VisitType)
            ? null
            : appointmentVm.Booking.VisitType;
        SelectedDate = appointmentVm.Booking.SelectedDate == default
            ? DateTime.Today
            : appointmentVm.Booking.SelectedDate.Date;
        SelectedTimeSlot = string.IsNullOrWhiteSpace(appointmentVm.Booking.SelectedTimeSlot)
            ? null
            : appointmentVm.Booking.SelectedTimeSlot;

        GenerateDateOptions();
        GenerateTimeSlots();
        GenerateMonthOptions();
    }

    partial void OnSelectedVisitTypeChanged(string? value)
    {
        if (appointmentViewModel != null)
        {
            appointmentViewModel.Booking.VisitType = value ?? string.Empty;
        }

        RefreshUi();
    }

    partial void OnSelectedDateChanged(DateTime value)
    {
        UpdateDateSelection();
        OnPropertyChanged(nameof(SelectedMonthText));
        GenerateTimeSlots();

        if (appointmentViewModel != null)
        {
            appointmentViewModel.Booking.SelectedDate = value;
        }

        RefreshUi();
    }

    partial void OnSelectedTimeSlotChanged(string? value)
    {
        UpdateTimeSelection();
        if (appointmentViewModel != null)
        {
            appointmentViewModel.Booking.SelectedTimeSlot = value ?? string.Empty;
        }

        RefreshUi();
    }

    private void RefreshUi()
    {
        OnPropertyChanged(nameof(SelectedAppointmentText));
        OnPropertyChanged(nameof(CanContinue));
    }

    private void GenerateDateOptions()
    {
        DateOptions.Clear();
        var monthStart = new DateTime(SelectedDate.Year, SelectedDate.Month, 1);
        var firstDate = monthStart < DateTime.Today ? DateTime.Today : monthStart;
        var lastDate = new DateTime(
            SelectedDate.Year,
            SelectedDate.Month,
            DateTime.DaysInMonth(SelectedDate.Year, SelectedDate.Month));

        for (var date = firstDate; date <= lastDate; date = date.AddDays(1))
            DateOptions.Add(new AppointmentDateOption(date));

        UpdateDateSelection();
    }

    private void GenerateMonthOptions()
    {
        MonthOptions.Clear();
        var month = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

        for (var index = 0; index <= 12; index++)
            MonthOptions.Add(month.AddMonths(index).ToString("MMMM yyyy"));
    }

    private void UpdateDateSelection()
    {
        foreach (var option in DateOptions)
            option.IsSelected = option.Date == SelectedDate.Date;
    }

    private void UpdateTimeSelection()
    {
        foreach (var option in TimeOptions)
            option.IsSelected = option.Value == SelectedTimeSlot;
    }

    private void GenerateTimeSlots()
    {
        AvailableTimeSlots.Clear();

        var slots = new[]
        {
            "9:00 AM",
            "10:00 AM",
            "11:00 AM",
            "12:00 PM",
            "1:00 PM",
            "2:00 PM",
            "3:00 PM",
            "4:00 PM",
            "5:00 PM"
        };

        foreach (var slot in slots)
            AvailableTimeSlots.Add(slot);

        TimeOptions.Clear();
        foreach (var slot in slots)
            TimeOptions.Add(new AppointmentTimeOption(slot));

        UpdateTimeSelection();
    }

    public bool CanContinue =>
        !string.IsNullOrWhiteSpace(SelectedVisitType) &&
        !string.IsNullOrWhiteSpace(SelectedTimeSlot);

    public bool IsClinicVisitSelected =>
        SelectedVisitType == AppointmentBooking.ClinicVisit;

    public bool IsVideoVisitSelected =>
        SelectedVisitType == AppointmentBooking.VideoVisit;

    public void ApplyVisitType(string? visitType)
    {
        SelectVisitType(visitType);
    }

    public void ApplyTimeOption(AppointmentTimeOption? option)
    {
        SelectTime(option);
    }

    public void SetMonth(DateTime monthDate)
    {
        var monthStart = new DateTime(monthDate.Year, monthDate.Month, 1);
        SelectedDate = monthStart < DateTime.Today ? DateTime.Today : monthStart;
        GenerateDateOptions();
        GenerateTimeSlots();
    }

    public void SetMonth(string? monthText)
    {
        if (DateTime.TryParseExact(
            monthText,
            "MMMM yyyy",
            null,
            System.Globalization.DateTimeStyles.None,
            out var month))
        {
            SetMonth(month);
        }
    }

    public string SelectedAppointmentText
    {
        get
        {
            if (string.IsNullOrWhiteSpace(SelectedVisitType))
            {
                return "Select Visit Type";
            }

            if (string.IsNullOrWhiteSpace(SelectedTimeSlot))
            {
                return $"{SelectedVisitType} • Select Time";
            }

            return $"{SelectedVisitType} • {SelectedDate:dd MMM yyyy} • {SelectedTimeSlot}";
        }
    }

    [RelayCommand]
    private void SelectVisitType(string? visitType)
    {
        if (visitType != AppointmentBooking.ClinicVisit &&
            visitType != AppointmentBooking.VideoVisit)
        {
            return;
        }

        SelectedVisitType = visitType;
        OnPropertyChanged(nameof(IsClinicVisitSelected));
        OnPropertyChanged(nameof(IsVideoVisitSelected));
    }

    [RelayCommand]
    private void SelectDate(AppointmentDateOption? option)
    {
        if (option == null)
            return;

        SelectedDate = option.Date;
    }

    [RelayCommand]
    private void SelectTime(AppointmentTimeOption? option)
    {
        if (option == null)
            return;

        SelectedTimeSlot = option.Value;
    }

    [RelayCommand]
    private void ContinueToReview()
    {
        if (!CanContinue || appointmentViewModel == null)
            return;

        appointmentViewModel.Booking.VisitType =
            SelectedVisitType ?? string.Empty;

        appointmentViewModel.Booking.SelectedDate =
            SelectedDate;

        appointmentViewModel.Booking.SelectedTimeSlot =
            SelectedTimeSlot ?? string.Empty;

        appointmentViewModel.CurrentStep = 3;

        appointmentViewModel.CurrentStepView =
            new AppointmentReviewView(
                appointmentViewModel);
    }
}