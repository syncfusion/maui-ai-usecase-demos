using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MauiNotes.Models;

public class NoteTreeNode : INotifyPropertyChanged
{
    public Guid Id { get; init; } = Guid.NewGuid();

    private string title = string.Empty;
    private bool isSelected;
    private bool isHovered;
    private bool isRenaming;
    private bool isRenameInvalid;
    private int noteCount;
    private string renameText = string.Empty;
    private string renameValidationMessage = string.Empty;
    private NoteModel? note;
    private NoteTreeNode? parent;

    public ObservableCollection<NoteTreeNode> Children { get; } = new();
    public ObservableCollection<NoteModel> Notes { get; } = new();
    public NoteTreeNode? Parent
    {
        get => parent;
        set
        {
            if (ReferenceEquals(parent, value))
                return;

            parent = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Parent)));
            NotifyFolderPathChanged();
        }
    }
    public bool IsNote => Note is not null;
    public bool IsFolder => Note is null;
    public string FilterTitle => Note?.Title ?? string.Empty;
    public string FolderPath => Parent is null ? Title : $"{Parent.FolderPath} / {Title}";
    public bool IsLocked => Note?.IsLocked ?? false;

    public NoteModel? Note
    {
        get => note;
        set
        {
            if (ReferenceEquals(note, value))
                return;

            if (note is not null)
                note.PropertyChanged -= OnNotePropertyChanged;

            note = value;

            if (note is not null)
                note.PropertyChanged += OnNotePropertyChanged;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Note)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsNote)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsFolder)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterTitle)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SubtitleText)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PreviewText)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PreviewLine)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EditorTimestampText)));
        }
    }

    public int NoteCount
    {
        get => noteCount;
        set => SetProperty(ref noteCount, value);
    }

    public string Title
    {
        get => Note?.Title ?? title;
        set
        {
            if (Note is not null)
                Note.Title = value;
            else if (SetProperty(ref title, value))
                NotifyFolderPathChanged();
        }
    }

    public string SubtitleText => Note?.SubtitleText ?? string.Empty;

    public string PreviewText => Note?.PreviewText ?? string.Empty;

    public string PreviewLine => Note?.PreviewLine ?? string.Empty;

    public string EditorTimestampText => Note?.EditorTimestampText ?? string.Empty;

    public bool IsSelected
    {
        get => Note?.IsSelected ?? isSelected;
        set
        {
            if (Note is not null)
                Note.IsSelected = value;
            else
                SetProperty(ref isSelected, value);
        }
    }

    public bool IsHovered
    {
        get => isHovered;
        set => SetProperty(ref isHovered, value);
    }

    public bool IsRenaming
    {
        get => isRenaming;
        set => SetProperty(ref isRenaming, value);
    }

    public bool IsRenameInvalid
    {
        get => isRenameInvalid;
        set => SetProperty(ref isRenameInvalid, value);
    }

    public string RenameText
    {
        get => renameText;
        set => SetProperty(ref renameText, value);
    }

    public string RenameValidationMessage
    {
        get => renameValidationMessage;
        set => SetProperty(ref renameValidationMessage, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }

    private void OnNotePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(NoteModel.Title))
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterTitle)));
        }
        else if (e.PropertyName == nameof(NoteModel.SubtitleText))
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SubtitleText)));
        else if (e.PropertyName == nameof(NoteModel.PreviewText))
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PreviewText)));
        else if (e.PropertyName == nameof(NoteModel.PreviewLine))
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PreviewLine)));
        else if (e.PropertyName == nameof(NoteModel.IsSelected))
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
        else if (e.PropertyName == nameof(NoteModel.IsLocked))
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLocked)));
        else if (e.PropertyName == nameof(NoteModel.EditorTimestampText))
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EditorTimestampText)));
    }

    private void NotifyFolderPathChanged()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FolderPath)));
        foreach (NoteTreeNode childFolder in Children.Where(child => child.IsFolder))
            childFolder.NotifyFolderPathChanged();
    }
}