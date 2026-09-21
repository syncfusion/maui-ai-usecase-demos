using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace PatientCompanion.Models;

public partial class SpecialtyItem : ObservableObject
{
    public SpecialtyItem(string name, string icon)
    {
        Name = name;
        Icon = icon;
    }

    public string Name { get; }

    public string Icon { get; }

    [ObservableProperty]
    private bool isSelected;
}
