using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AIFileSearchTreeView;

/// <summary>
/// Base model for tree nodes. Uses change notifications because the TreeView
/// updates as nodes are filtered or highlighted.
/// </summary>
public class FileManager : INotifyPropertyChanged
{
    private string _itemName = string.Empty;
    private ImageSource? _imageIcon;
    private ObservableCollection<FileManager> _subFiles = new();

    public string ItemName
    {
        get => _itemName;
        set => SetField(ref _itemName, value);
    }

    public ImageSource? ImageIcon
    {
        get => _imageIcon;
        set => SetField(ref _imageIcon, value);
    }

    public ObservableCollection<FileManager> SubFiles
    {
        get => _subFiles;
        set => SetField(ref _subFiles, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}