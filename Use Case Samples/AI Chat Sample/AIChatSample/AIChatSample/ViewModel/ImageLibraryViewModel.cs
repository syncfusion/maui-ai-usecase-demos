using System.Collections.ObjectModel;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;

namespace AIChatSample;

/// <summary>
/// View-model for the Media Gallery / Image Library page. Owns the
/// image-card collection, search-text state, and the upload / open /
/// delete commands the page binds to.
/// </summary>
public partial class ImageLibraryViewModel : ObservableObject
{
    /// <summary>Current search-text input.</summary>
    [ObservableProperty] private string? searchText;

    /// <summary>True while the page is loading data.</summary>
    [ObservableProperty] private bool isLoading;

    /// <summary>Currently selected image (multi-select grid).</summary>
    [ObservableProperty] private ImageItemModel? selectedImage;

    /// <summary>Full set of image-library rows rendered by the page.</summary>
    public ObservableCollection<ImageItemModel> ImageLibraryItems { get; } = new();

    /// <summary>Filtered subset of <see cref="ImageLibraryItems"/> after search.</summary>
    public ObservableCollection<ImageItemModel> FilteredImages { get; } = new();

    /// <summary>True when <see cref="ImageLibraryItems"/> has any rows.</summary>
    public bool HasImages => ImageLibraryItems.Count > 0;

    /// <summary>Command that runs the search-text filter.</summary>
    public ICommand SearchCommand { get; } = new Command(_ => { });

    /// <summary>Command raised when the user selects an image card.</summary>
    public ICommand ImageSelectedCommand { get; } = new Command(_ => { });

    /// <summary>Command that opens the tapped image in a detail view.</summary>
    public ICommand OpenImageCommand { get; } = new Command(_ => { });

    /// <summary>Command that deletes the tapped image.</summary>
    public ICommand DeleteImageCommand { get; } = new Command(_ => { });

    /// <summary>Command that opens the file picker to add a new image.</summary>
    public ICommand UploadImageCommand { get; } = new Command(() => { });

    /// <summary>Command that re-fetches the image library.</summary>
    public ICommand RefreshLibraryCommand { get; } = new Command(() => { });

    /// <summary>
    /// Builds the demo image-library data set. Real apps would back this
    /// with a file-system / cloud-storage query.
    /// </summary>
    public ImageLibraryViewModel()
    {
        ImageLibraryItems.Add(new ImageItemModel
        {
            Id = "1",
            ImageName = "Mountain Lake",
            Category = "Landscape",
            ImagePath = "library1.png",
            ThumbnailPath = "background.png",
            Tags = "landscape, nature, outdoors",
            CreatedDate = "Mon",
            Description = "A wide landscape shot used as a background reference."
        });
        ImageLibraryItems.Add(new ImageItemModel
        {
            Id = "2",
            ImageName = "Abstract women Artwork",
            Category = "Artwork",
            ImagePath = "library2.png",
            ThumbnailPath = "review1.png",
            Tags = "abstract, portrait, art",
            CreatedDate = "Mon",
            Description = "Abstract portrait artwork sample card."
        });
        ImageLibraryItems.Add(new ImageItemModel
        {
            Id = "3",
            ImageName = "Portrait women",
            Category = "Portrait",
            ImagePath = "library3.png",
            ThumbnailPath = "review2.png",
            Tags = "portrait, photography, people",
            CreatedDate = "Mon",
            Description = "Portrait photography sample card."
        });
        ImageLibraryItems.Add(new ImageItemModel
        {
            Id = "4",
            ImageName = "Fox Photography",
            Category = "Animals",
            ImagePath = "library4.png",
            ThumbnailPath = "review3.png",
            Tags = "fox, wildlife, photography",
            CreatedDate = "Mon",
            Description = "Fox photography sample card."
        });
        ImageLibraryItems.Add(new ImageItemModel
        {
            Id = "5",
            ImageName = "Studio women",
            Category = "Portrait",
            ImagePath = "library5.png",
            ThumbnailPath = "product.png",
            Tags = "studio, portrait, people",
            CreatedDate = "Mon",
            Description = "Studio portrait sample card."
        });
    }
}
