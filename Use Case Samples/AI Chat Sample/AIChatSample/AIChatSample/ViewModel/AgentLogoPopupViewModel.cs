using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Controls;

namespace AIChatSample
{

    public partial class AgentLogoPopupViewModel : ObservableObject
    {
        #region In-progress selection (what the user picks in the popup)

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsCreateMode))]
        [NotifyPropertyChangedFor(nameof(IsUploadMode))]
        [NotifyPropertyChangedFor(nameof(HasSelectedImage))]
        [NotifyPropertyChangedFor(nameof(IsApplyEnabled))]
        private int selectedTab;            // 0 = Create, 1 = Upload

        [ObservableProperty] private LogoIconModel? selectedIcon;
        [ObservableProperty] private LogoColorModel? selectedColor;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasSelectedImage))]
        [NotifyPropertyChangedFor(nameof(IsApplyEnabled))]
        private string? selectedImage;
        [ObservableProperty] private AgentConfiguration? targetAgent;

        #endregion

        #region Committed avatar (what the host page displays after Apply)

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasAvatar))]
        [NotifyPropertyChangedFor(nameof(IsImageAvatar))]
        [NotifyPropertyChangedFor(nameof(IsIconAvatar))]
        private string? agentAvatar;

        public bool HasAvatar => !string.IsNullOrEmpty(AgentAvatar);
        public bool IsImageAvatar => !string.IsNullOrEmpty(AgentAvatar) && !AgentAvatar!.Contains(';');
        public bool IsIconAvatar => !string.IsNullOrEmpty(AgentAvatar) && AgentAvatar!.Contains(';');

        /// <summary>
        /// Background color extracted from the committed avatar string. Used
        /// by the host page (e.g. Configure Agent avatar circle and Preview)
        /// so the chosen color is only visible AFTER the user taps Apply —
        /// the in-progress popup selection should not leak into the
        /// surrounding UI.
        /// </summary>
        public string AgentColor
        {
            get
            {
                if (string.IsNullOrEmpty(AgentAvatar) || !AgentAvatar!.Contains(';')) return string.Empty;
                var parts = AgentAvatar.Split(';');
                return parts.Length >= 3 ? parts[2] : string.Empty;
            }
        }

        /// <summary>
        /// Builds a FontImageSource from the encoded "FontFamily;Glyph;Color"
        /// committed avatar so the avatar Image can render a Create-mode icon.
        /// The icon glyph itself is rendered in black so the surrounding Border's
        /// <c>BackgroundColor</c> (driven by the selected color swatch) is the
        /// only thing the user sees changing.
        /// </summary>
        public ImageSource? IconAvatar
        {
            get
            {
                if (AgentAvatar is null || !AgentAvatar.Contains(';')) return null;
                var parts = AgentAvatar.Split(';');
                if (parts.Length < 3) return null;
                return new FontImageSource
                {
                    FontFamily = parts[0],
                    Glyph = parts[1],
                    Color = Colors.Black,
                    Size = 40
                };
            }
        }

        partial void OnAgentAvatarChanged(string? value)
        {
            OnPropertyChanged(nameof(IconAvatar));
            OnPropertyChanged(nameof(AgentColor));
        }

        /// <summary>
        /// Live preview avatar that reflects the user's *in-progress* selection
        /// (before Apply is pressed). The Configure page's Preview tab binds
        /// to this so the avatar stays in sync as the user changes icon/color.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(LiveIconAvatar))]
        [NotifyPropertyChangedFor(nameof(LiveIsIconAvatar))]
        private string? _liveIconGlyph;

        [ObservableProperty]
        private string? _liveIconFontFamily;

        [ObservableProperty]
        private string? _liveIconColor;

        public ImageSource? LiveIconAvatar
        {
            get
            {
                if (string.IsNullOrEmpty(LiveIconGlyph) || string.IsNullOrEmpty(LiveIconFontFamily))
                {
                    return null;
                }
                return new FontImageSource
                {
                    FontFamily = LiveIconFontFamily,
                    Glyph = LiveIconGlyph,
                    Color = Colors.Black,
                    Size = 40
                };
            }
        }

        public bool LiveIsIconAvatar => !string.IsNullOrEmpty(LiveIconGlyph);

        #endregion

        #region.IsEnabled toggle

        [ObservableProperty] private bool isApplyEnabled;

        #endregion

        #region Collections

        public ObservableCollection<LogoIconModel> AvailableIcons { get; } = new();
        public ObservableCollection<LogoColorModel> AvailableColors { get; } = new();

        #endregion

        #region Derived

        public bool IsCreateMode => SelectedTab == 0;
        public bool IsUploadMode => SelectedTab == 1;
        public bool HasSelectedImage => !string.IsNullOrEmpty(SelectedImage);

        #endregion

        #region Commands

        public ICommand SwitchToCreateCommand { get; }
        public ICommand SwitchToUploadCommand { get; }
        public ICommand SelectIconCommand { get; }
        public ICommand SelectColorCommand { get; }
        public ICommand SelectImageCommand { get; }
        public ICommand RemoveImageCommand { get; }
        public ICommand ApplyCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand ClosePopupCommand { get; }

        #endregion

        /// <summary>Raised when the user taps Apply / Cancel / Close.</summary>
        public event EventHandler? RequestClose;

        public AgentLogoPopupViewModel()
        {
            // ---- Icons ----
            AvailableIcons.Add(new LogoIconModel { Id = "1", IconName = "Image", IconGlyph = "\ue700", IconFontFamily = "UIKitIcons" });
            AvailableIcons.Add(new LogoIconModel { Id = "2", IconName = "Idea", IconGlyph = "\ue701", IconFontFamily = "UIKitIcons" });
            AvailableIcons.Add(new LogoIconModel { Id = "3", IconName = "Information", IconGlyph = "\ue703", IconFontFamily = "UIKitIcons" });
            AvailableIcons.Add(new LogoIconModel { Id = "4", IconName = "Checklist", IconGlyph = "\ue70d", IconFontFamily = "UIKitIcons" });
            AvailableIcons.Add(new LogoIconModel { Id = "5", IconName = "Document", IconGlyph = "\ue716", IconFontFamily = "UIKitIcons" });
            AvailableIcons.Add(new LogoIconModel { Id = "6", IconName = "Institution", IconGlyph = "\ue72d", IconFontFamily = "UIKitIcons" });
            AvailableIcons.Add(new LogoIconModel { Id = "7", IconName = "Puzzle", IconGlyph = "\ue72e", IconFontFamily = "UIKitIcons" });
            AvailableIcons.Add(new LogoIconModel { Id = "8", IconName = "Headset", IconGlyph = "\ue71e", IconFontFamily = "UIKitIcons" });
            AvailableIcons.Add(new LogoIconModel { Id = "9", IconName = "Flow", IconGlyph = "\ue715", IconFontFamily = "UIKitIcons" });
            AvailableIcons.Add(new LogoIconModel { Id = "10", IconName = "Users", IconGlyph = "\ue714", IconFontFamily = "UIKitIcons" });
            AvailableIcons[0].IsSelected = true;
            SelectedIcon = AvailableIcons[0];

            // ---- Colors ----
            AvailableColors.Add(new LogoColorModel { Id = "1", ColorName = "Pink", ColorHex = "#D84A8C" });
            AvailableColors.Add(new LogoColorModel { Id = "2", ColorName = "Yellow", ColorHex = "#F5D442" });
            AvailableColors.Add(new LogoColorModel { Id = "3", ColorName = "Green", ColorHex = "#5BC248" });
            AvailableColors.Add(new LogoColorModel { Id = "4", ColorName = "Blue", ColorHex = "#5567E8" });
            AvailableColors.Add(new LogoColorModel { Id = "5", ColorName = "Red", ColorHex = "#D65B5B" });
            AvailableColors.Add(new LogoColorModel { Id = "6", ColorName = "Purple", ColorHex = "#8B5AE0" });
            AvailableColors[5].IsSelected = true;
            SelectedColor = AvailableColors[5];

            // ---- Commands ----
            SwitchToCreateCommand = new Command(() => SelectedTab = 0);
            SwitchToUploadCommand = new Command(() => SelectedTab = 1);
            SelectIconCommand = new Command<LogoIconModel?>(OnSelectIcon);
            SelectColorCommand = new Command<LogoColorModel?>(OnSelectColor);
            SelectImageCommand = new Command(OnSelectImage);
            RemoveImageCommand = new Command(OnRemoveImage);
            ApplyCommand = new Command(OnApply, () => IsApplyEnabled || IsCreateMode || HasSelectedImage);
            CancelCommand = new Command(OnCancel);
            ClosePopupCommand = new Command(OnClose);

            RefreshApplyEnabled();
        }

        /// <summary>Resets in-progress state so Create mode shows fresh on each open.</summary>
        public void ResetForOpen(AgentConfiguration? agent = null)
        {
            TargetAgent = agent;
            SelectedTab = 0;                                          // Create by default
            SelectedImage = agent?.AvatarSource;
            // Restore icon/color selection defaults
            foreach (var i in AvailableIcons) i.IsSelected = (i == AvailableIcons[0]);
            SelectedIcon = AvailableIcons[0];
            foreach (var c in AvailableColors) c.IsSelected = (c == AvailableColors[5]);
            SelectedColor = agent is null
                ? AvailableColors[5]
                : AvailableColors.FirstOrDefault(c => c.ColorHex == agent.Color) ?? AvailableColors[5];
            RefreshApplyEnabled();
        }

        partial void OnSelectedTabChanged(int value) => RefreshApplyEnabled();
        partial void OnSelectedImageChanged(string? value) => RefreshApplyEnabled();
        partial void OnSelectedIconChanged(LogoIconModel? value)
        {
            // TwoWay SelectedItem binding on the CollectionView only sets
            // SelectedIcon; the underlying LogoIconModel.IsSelected flags
            // are the binding source for the per-cell highlight, so keep
            // them in lock-step here (single-selection across the list).
            foreach (var i in AvailableIcons) i.IsSelected = ReferenceEquals(i, value);
            RefreshApplyEnabled();
        }
        partial void OnSelectedColorChanged(LogoColorModel? value)
        {
            // Same single-selection deselect for the color row.
            foreach (var c in AvailableColors) c.IsSelected = ReferenceEquals(c, value);
            LiveIconColor = value?.ColorHex;
            RefreshApplyEnabled();
        }

        private void RefreshApplyEnabled()
        {
            IsApplyEnabled = IsCreateMode || HasSelectedImage;
            (ApplyCommand as Command)?.ChangeCanExecute();
            OnPropertyChanged(nameof(IsApplyEnabled));
        }

        private void OnSelectIcon(LogoIconModel? icon)
        {
            if (icon is null) return;
            foreach (var i in AvailableIcons) i.IsSelected = false;
            icon.IsSelected = true;
            SelectedIcon = icon;
        }

        private void OnSelectColor(LogoColorModel? color)
        {
            if (color is null) return;
            foreach (var c in AvailableColors) c.IsSelected = false;
            color.IsSelected = true;
            SelectedColor = color;
        }

        private async void OnSelectImage()
        {
            try
            {
                var result = await Microsoft.Maui.Storage.FilePicker.Default.PickAsync(
                    new Microsoft.Maui.Storage.PickOptions
                    {
                        PickerTitle = "Select agent logo",
                        FileTypes = Microsoft.Maui.Storage.FilePickerFileType.Images
                    });

                if (result is null) return;

                var ext = System.IO.Path.GetExtension(result.FileName)?.ToLowerInvariant() ?? "";
                if (ext is not (".png" or ".jpg" or ".jpeg" or ".svg"))
                    return;                         // Hook: surface validation error

                SelectedImage = result.FullPath ?? result.FileName;
            }
            catch (TaskCanceledException)
            {
                // User dismissed the picker — nothing to do.
            }
            catch (Exception ex)
            {
                // Hook: surface validation error.
                System.Diagnostics.Debug.WriteLine($"Logo picker failed: {ex.Message}");
            }
        }

        private void OnRemoveImage() => SelectedImage = null;

        /// <summary>
        /// Apply: COMMIT the current selection to <see cref="AgentAvatar"/> so the
        /// host page's avatar Image updates, then request the popup to close.
        /// Cancel/Close do NOT commit — they just close.
        /// </summary>
        private void OnApply()
        {
            if (IsUploadMode && !string.IsNullOrEmpty(SelectedImage))
            {
                AgentAvatar = SelectedImage;
                if (TargetAgent is not null)
                {
                    TargetAgent.AvatarSource = SelectedImage;
                    TargetAgent.AvatarGlyph = null;
                    TargetAgent.Color = SelectedColor?.ColorHex ?? TargetAgent.Color;
                }
            }
            else if (IsCreateMode && SelectedIcon is { } icon)
            {
                AgentAvatar = $"{icon.IconFontFamily};{icon.IconGlyph};{SelectedColor?.ColorHex ?? "#8B5AE0"}";
                if (TargetAgent is not null)
                {
                    TargetAgent.AvatarGlyph = AgentAvatar;
                    TargetAgent.AvatarSource = null;
                    TargetAgent.Color = SelectedColor?.ColorHex ?? TargetAgent.Color;
                }
            }
            RequestClose?.Invoke(this, EventArgs.Empty);
        }

        private void OnCancel() => RequestClose?.Invoke(this, EventArgs.Empty);
        private void OnClose() => RequestClose?.Invoke(this, EventArgs.Empty);
    }
}