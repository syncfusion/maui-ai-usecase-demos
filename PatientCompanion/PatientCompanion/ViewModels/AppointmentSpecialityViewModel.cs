using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PatientCompanion;
using PatientCompanion.Models;
using PatientCompanion.Views;
using System.Collections.ObjectModel;

namespace PatientCompanion.ViewModels;

public partial class AppointmentSpecialityViewModel : ObservableObject
{
    private readonly AppointmentViewModel appointmentViewModel;

    [ObservableProperty]
    private int selectedStep;

    [ObservableProperty]
    private SpecialtyItem? selectedSpecialty;

    public ObservableCollection<SpecialtyItem> Specialties { get; }

    public bool CanContinue => SelectedSpecialty != null;

    public AppointmentSpecialityViewModel(
        AppointmentViewModel appointmentViewModel)
    {
        this.appointmentViewModel = appointmentViewModel;

        Specialties = new ObservableCollection<SpecialtyItem>
        {
            new SpecialtyItem("Cardiology", MaterialIcons.Favorite),
            new SpecialtyItem("Dermatology", MaterialIcons.Face),
            new SpecialtyItem("Pediatrics", MaterialIcons.ChildCare),
            new SpecialtyItem("General", MaterialIcons.MedicalServices),
            new SpecialtyItem("Neurology", MaterialIcons.Psychology),
            new SpecialtyItem("Ophthalmology", MaterialIcons.Visibility),
            new SpecialtyItem("Other", MaterialIcons.MoreHoriz)
        };
    }

    [RelayCommand]
    private Task ContinueToDoctors()
    {
        if (SelectedSpecialty == null)
            return Task.CompletedTask;

        appointmentViewModel.Booking.SelectedSpeciality = SelectedSpecialty;
        appointmentViewModel.CurrentStep = 1;
        appointmentViewModel.CurrentStepView = new AppointmentDoctorSelectionView(appointmentViewModel);

        return Task.CompletedTask;
    }

    [RelayCommand]
    private void SelectSpecialty(SpecialtyItem? specialty)
    {
        if (specialty == null)
            return;

        foreach (var item in Specialties)
            item.IsSelected = ReferenceEquals(item, specialty);

        SelectedSpecialty = specialty;
        OnPropertyChanged(nameof(CanContinue));
    }
}