using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HomeInventoryManager.Model;
using HomeInventoryManager.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HomeInventoryManager.ViewModel
{
	/// <summary>
	/// Discrete stages of the two-half scan-and-add flow. The top half of the
	/// page renders the capture hero (ChooseMethod), switches to a captured
	/// image preview with Retake/Cancel/Continue (ConfirmCapture), then
	/// collapses and the bottom half with the form becomes visible (Review).
	/// </summary>
	public enum CaptureStage
	{
		/// <summary>Top half shows the vibrant capture hero.</summary>
		ChooseMethod,

		/// <summary>Top half shows the captured image and Retake/Cancel/Continue.</summary>
		ConfirmCapture,

		/// <summary>Top half collapses; bottom half shows the extracted details + Save/Cancel.</summary>
		Review
	}

	/// <summary>Backs the Scan and Add Item page. Surfaces the scan entry methods, simulates an AI extraction pass, and holds the editable/validated fields that a user can modify before saving the new inventory item.</summary>
	public partial class ScanViewModel : ObservableObject
	{
		private readonly InventoryViewModel _inventoryViewModel;
		private readonly IAIService _aiService;
		private ScanItem currentItem = new();

		[ObservableProperty]
		private CaptureStage stage = CaptureStage.ChooseMethod;

		[ObservableProperty]
		private ScanStatus scanStatus = ScanStatus.Pending;

		[ObservableProperty]
		private bool isScanning;

		[ObservableProperty]
		private string scanStatusText = "Choose a method to start scanning";

		[ObservableProperty]
		private string validationMessage = string.Empty;

		[ObservableProperty]
		private bool hasValidationError;

		[ObservableProperty]
		private bool hasResult;

		[ObservableProperty]
		private string? invoicePdfPath;

		[ObservableProperty]
		private string? productPhotoPath;

		[ObservableProperty]
		private int? warrantyOffsetDaysFromToday;


		/// <summary>Raised after a successful save so the page can navigate away.</summary>
		public event EventHandler<InventoryItem>? ItemSaved;

		/// <summary>
		/// Raised when the user taps Cancel on either the capture stage or
		/// the review stage. The page handles navigation; the VM just resets
		/// its state.
		/// </summary>
		public event EventHandler? CancelRequested;

		public ScanViewModel(InventoryViewModel inventoryViewModel) : this(inventoryViewModel, new MockAIService())
		{
		}

		public ScanViewModel(InventoryViewModel inventoryViewModel, IAIService aiService)
		{
			_inventoryViewModel = inventoryViewModel;
			_aiService = aiService;
			SeedScanOptions();
		}

		// Design-time / parameterless constructor for XAML preview only.
		public ScanViewModel() : this(new InventoryViewModel(), new MockAIService())
		{
		}

		public ObservableCollection<ScanOption> ScanOptions { get; } = new();

		public ObservableCollection<string> Categories { get; } =
		[
			"Electronics",
			"Appliances",
			"Furniture",
			"Kitchen",
			"Smart Devices",
			"Outdoor Equipment",
			"Decor",
			"Others"
		];

		public ObservableCollection<string> CoverageTypes { get; } =
		[
			"Comprehensive",
			"Standard",
			"Extended",
			"Manufacturer",
			"None"
		];

		public string PurchaseDateString
		{
			get => CurrentItem.PurchaseDate.ToString("dd/MM/yyyy");
			set
			{
				if (DateTime.TryParseExact(
					value,
					"dd/MM/yyyy",
					CultureInfo.InvariantCulture,
					DateTimeStyles.None,
					out var dt))
				{
					CurrentItem.PurchaseDate = dt;
				}
			}
		}

		public string WarrantyEndDateString
		{
			get => CurrentItem.WarrantyEndDate.ToString("dd/MM/yyyy");
			set
			{
				if (DateTime.TryParseExact(
					value,
					"dd/MM/yyyy",
					CultureInfo.InvariantCulture,
					DateTimeStyles.None,
					out var dt))
				{
					CurrentItem.WarrantyEndDate = dt;
				}
			}
		}

		public ScanItem CurrentItem
		{
			get => currentItem;
			set
			{
				if (ReferenceEquals(currentItem, value))
					return;

				currentItem = value ?? new ScanItem();
				OnPropertyChanged();
				OnPropertyChanged(nameof(CanSave));
				OnPropertyChanged(nameof(HasManualData));
				OnPropertyChanged(nameof(HasResult));
			}
		}

		partial void OnScanStatusChanged(ScanStatus value)
		{
			OnPropertyChanged(nameof(ShowExtractedFields));
			OnPropertyChanged(nameof(CanSave));
			OnPropertyChanged(nameof(HasResult));
			OnPropertyChanged(nameof(HasScanResult));
			OnPropertyChanged(nameof(IsInReviewStage));
			OnPropertyChanged(nameof(IsInChooseStage));
			OnPropertyChanged(nameof(IsInConfirmStage));
		}

		partial void OnStageChanged(CaptureStage value)
		{
			OnPropertyChanged(nameof(IsInChooseStage));
			OnPropertyChanged(nameof(IsInConfirmStage));
			OnPropertyChanged(nameof(IsInReviewStage));
			OnPropertyChanged(nameof(CanSave));
		}

		partial void OnIsScanningChanged(bool value)
		{
			OnPropertyChanged(nameof(HasResult));
			OnPropertyChanged(nameof(HasScanResult));
		}

		partial void OnHasValidationErrorChanged(bool value)
		{
			OnPropertyChanged(nameof(HasResult));
			OnPropertyChanged(nameof(HasScanResult));
		}

		partial void OnHasResultChanged(bool value)
		{
			OnPropertyChanged(nameof(HasScanResult));
		}

		partial void OnValidationMessageChanged(string value)
		{
			OnPropertyChanged(nameof(HasValidationError));
		}

		partial void OnWarrantyOffsetDaysFromTodayChanged(int? value)
		{
			if (value.HasValue)
				CurrentItem.WarrantyEndDate = DateTime.Today.AddDays(value.Value).Date;
			else
				CurrentItem.WarrantyEndDate = DateTime.Now;
		}

		public bool ShowExtractedFields =>
			ScanStatus == ScanStatus.Extracted || ScanStatus == ScanStatus.Validated;

		/// <summary>Top half renders the vibrant capture/CTA hero.</summary>
		public bool IsInChooseStage => Stage == CaptureStage.ChooseMethod;

		/// <summary>Top half renders the captured image preview + Retake/Cancel/Continue.</summary>
		public bool IsInConfirmStage => Stage == CaptureStage.ConfirmCapture;

		/// <summary>Bottom half (extracted-fields form + Save/Cancel) is visible.</summary>
		public bool IsInReviewStage => Stage == CaptureStage.Review;

		public bool HasScanResult => HasResult || ShowExtractedFields || HasValidationError || HasManualData || ScanStatus == ScanStatus.Scanning;

		public bool HasManualData =>
			!string.IsNullOrWhiteSpace(CurrentItem.ProductName) ||
			!string.IsNullOrWhiteSpace(CurrentItem.Brand) ||
			!string.IsNullOrWhiteSpace(CurrentItem.Category) ||
			CurrentItem.Amount > 0m ||
			!string.IsNullOrWhiteSpace(CurrentItem.ImagePreview);

		public bool CanSave => IsInReviewStage && (ShowExtractedFields || HasManualData);

		private void SeedScanOptions()
		{
			ScanOptions.Add(new ScanOption
			{
				Title = "Invoice Scan",
				Description = "Extract product, brand, price & warranty from a purchase invoice",
				Icon = "🧾",
				BackgroundColor = "#EAF0FF",
				IconColor = "#0D33B8"
			});

			ScanOptions.Add(new ScanOption
			{
				Title = "Warranty Card",
				Description = "Capture warranty period, coverage type & provider details",
				Icon = "🛡️",
				BackgroundColor = "#FDF2E9",
				IconColor = "#EA580C"
			});

			ScanOptions.Add(new ScanOption
			{
				Title = "Product Label",
				Description = "Read serial number, model & manufacturer from the label",
				Icon = "🏷️",
				BackgroundColor = "#F0FDF4",
				IconColor = "#16A34A"
			});

			ScanOptions.Add(new ScanOption
			{
				Title = "Manual Entry",
				Description = "Add an asset by typing the details yourself",
				Icon = "⌨️",
				BackgroundColor = "#FAF5FF",
				IconColor = "#7C3AED"
			});
		}

		/// <summary>
		/// Simulates the AI invoice/warranty/label extraction. In a real build this
		/// would call the AI-01 Invoice Reader / AI-02 Auto Categorization services.
		/// </summary>
		public async Task StartScanAsync(ScanOption option)
		{
			await StartScanAsync(option, attachmentPath: null, attachmentIsPhoto: false);
		}

		public async Task StartScanAsync(ScanOption option, string? attachmentPath, bool attachmentIsPhoto)
		{
			IsScanning = true;
			ScanStatus = ScanStatus.Scanning;
			ScanStatusText = attachmentPath is null
				? $"{option.Title} selected. {option.Description}"
				: $"{option.Title} attachment received. Extracting item details…";

			// Simulate AI processing time (NFR: <= 5 seconds)
			await Task.Delay(1200);

			IsScanning = false;
			ScanStatus = ScanStatus.Extracted;
			ScanStatusText = attachmentPath is null
				? $"{option.Title} ready. Review and validate the fields below."
				: $"{option.Title} extraction complete. Review and validate the fields below.";
		}

		/// <summary>User completes validation; mark ready to persist.</summary>
		public void ValidateItem()
		{
			ScanStatus = ScanStatus.Validated;
			ScanStatusText = "Validated. Tap Save to add this asset to your inventory.";
		}

		/// <summary>
		/// Validate and persist the extracted item to the shared
		/// <see cref="InventoryViewModel"/>, then raise <see cref="ItemSaved"/>.
		/// </summary>
		[RelayCommand]
		public void Save()
		{
			// The Save button is only enabled while the user is in the Review
			// stage, but guard here too so a stray wired-up button can't sneak
			// past validation by saving prematurely.
			if (Stage != CaptureStage.Review)
				return;

			HasValidationError = false;
			ValidationMessage = string.Empty;

			if (string.IsNullOrWhiteSpace(CurrentItem.ProductName))
			{
				HasValidationError = true;
				ValidationMessage = "Product name is required.";
				return;
			}

			if (string.IsNullOrWhiteSpace(CurrentItem.Brand))
			{
				HasValidationError = true;
				ValidationMessage = "Brand is required.";
				return;
			}

			if (string.IsNullOrWhiteSpace(CurrentItem.Category))
			{
				HasValidationError = true;
				ValidationMessage = "Please pick a category.";
				return;
			}

			if (CurrentItem.Amount <= 0m)
			{
				HasValidationError = true;
				ValidationMessage = "Purchase price is required.";
				return;
			}

			if (string.IsNullOrWhiteSpace(CurrentItem.ImagePreview))
			{
				HasValidationError = true;
				ValidationMessage = "Image is required.";
				return;
			}

			ScanStatus = ScanStatus.Validated;

			var warrantyEndDate = CurrentItem.WarrantyEndDate;
			var status = ComputeWarrantyStatus(warrantyEndDate);
			var nextId = _inventoryViewModel.AllItems.Any()
				? _inventoryViewModel.AllItems.Max(x => x.InventoryId) + 1
				: 1;

			var newItem = new InventoryItem
			{
				InventoryId = nextId,
				Name = CurrentItem.ProductName.Trim(),
				Brand = CurrentItem.Brand.Trim(),
				Category = CurrentItem.Category,
				PurchasePrice = CurrentItem.Amount,
				Image = string.IsNullOrWhiteSpace(CurrentItem.ImagePreview) ? "dotnet_bot.png" : CurrentItem.ImagePreview,
				WarrantyEndDate = warrantyEndDate,
				WarrantyStatus = status,
				StatusColor = status switch
				{
					"Expired" => "#DC2626",
					"Expiring Soon" => "#EA580C",
					_ => "#1D4ED8"
				}
			};

			_inventoryViewModel.AddItem(newItem);
			ScanStatusText = "Saved! Redirecting to your inventory…";
			Stage = CaptureStage.ChooseMethod;
			ItemSaved?.Invoke(this, newItem);
		}

		[RelayCommand]
		public void Reset()
		{
			CurrentItem = new ScanItem();
			ScanStatus = ScanStatus.Pending;
			ScanStatusText = "Choose a method to start scanning";
			ValidationMessage = string.Empty;
			HasValidationError = false;
			InvoicePdfPath = null;
			ProductPhotoPath = null;
			WarrantyOffsetDaysFromToday = null;
			HasResult = false;
			Stage = CaptureStage.ChooseMethod;
		}

		/// <summary>
		/// Called by the page once a photo (captured or uploaded) has been
		/// stored on the current item.
		/// Moves the top half from ChooseMethod → ConfirmCapture and asks the
		/// AI service to extract any details it can from the image so the
		/// user can quickly Continue into review.
		/// </summary>
		public async Task PhotoCapturedAsync(string imagePath)
		{
			if (string.IsNullOrWhiteSpace(imagePath))
				return;

			if (string.IsNullOrWhiteSpace(CurrentItem.ImagePreview))
				CurrentItem.ImagePreview = imagePath;

			Stage = CaptureStage.ConfirmCapture;
			ScanStatus = ScanStatus.Extracted;
			ScanStatusText = "Photo attached. Continue to let AI extract the details.";

			HasResult = true;
		}

		/// <summary>User accepted the captured photo; reveal the review form.</summary>
		[RelayCommand]
		public void ContinueToReview()
		{
			if (Stage != CaptureStage.ConfirmCapture)
				return;
			Stage = CaptureStage.Review;
			ScanStatusText = "Review the extracted details below, then tap Save.";
			OnPropertyChanged(nameof(IsInReviewStage));
			OnPropertyChanged(nameof(CanSave));
		}

		/// <summary>
		/// User wants to abandon the entire capture/add flow. Resets the
		/// VM state and raises <see cref="CancelRequested"/> so the page
		/// can navigate back.
		/// </summary>
		[RelayCommand]
		public void CancelFlow()
		{
			Reset();
			CancelRequested?.Invoke(this, EventArgs.Empty);
		}

		/// <summary>
		/// User wants to retake the photo. Goes back to ChooseMethod and
		/// clears the previously captured preview so the page can prompt
		/// again.
		/// </summary>
		[RelayCommand]
		public void RetakeCapture()
		{
			CurrentItem.ImagePreview = string.Empty;
			CurrentItem.ProductName = string.Empty;
			CurrentItem.Brand = string.Empty;
			CurrentItem.Amount = 0m;
			Stage = CaptureStage.ChooseMethod;
			ScanStatus = ScanStatus.Pending;
			ScanStatusText = "Capture or upload a photo to continue.";
		}

		/// <summary>Raised when the user picks one of the 5 capture options on the page.</summary>
		public event EventHandler<ScanOption>? CaptureRequested;

		[RelayCommand]
		public Task OpenInvoiceScan() => TriggerCaptureAsync("Invoice Scan");

		[RelayCommand]
		public Task OpenProductLabel() => TriggerCaptureAsync("Product Label");

		[RelayCommand]
		public Task OpenWarrantyCard() => TriggerCaptureAsync("Warranty Card");

		// ====================================================================
		//  Page-driven ingestion (called from ScanPage.xaml.cs handlers)
		// ====================================================================

		/// <summary>
		/// Handles an invoice image picked or captured by the page. Performs the
		/// same simulated AI extraction as the "Invoice Scan" capture option.
		/// </summary>
		public async Task IngestScannedInvoice(string imagePath)
		{
			if (string.IsNullOrWhiteSpace(imagePath))
				return;
			HasValidationError = false;
			ValidationMessage = string.Empty;

			var option = FindOption("Invoice Scan") ?? BuildFallbackOption("Invoice Scan");
			await StartScanAsync(option, imagePath, attachmentIsPhoto: true);
		}

		/// <summary>
		/// Handles a product-label image picked or captured by the page.
		/// </summary>
		public async Task IngestScannedProductLabel(string imagePath)
		{
			if (string.IsNullOrWhiteSpace(imagePath))
				return;
			HasValidationError = false;
			ValidationMessage = string.Empty;

			var option = FindOption("Product Label") ?? BuildFallbackOption("Product Label");
			await StartScanAsync(option, imagePath, attachmentIsPhoto: true);
		}

		/// <summary>
		/// Handles a warranty-card image picked or captured by the page.
		/// </summary>
		public async Task IngestScannedWarrantyCard(string imagePath)
		{
			if (string.IsNullOrWhiteSpace(imagePath))
				return;
			HasValidationError = false;
			ValidationMessage = string.Empty;

			var option = FindOption("Warranty Card") ?? BuildFallbackOption("Warranty Card");
			await StartScanAsync(option, imagePath, attachmentIsPhoto: true);
		}

		/// <summary>
		/// Stores a user-picked invoice PDF path and surfaces it on the current item
		/// so the user can see which document was attached.
		/// </summary>
		public async Task AttachInvoicePdf(string pdfPath)
		{
			if (string.IsNullOrWhiteSpace(pdfPath))
				return;

			InvoicePdfPath = pdfPath;
			CurrentItem.InvoiceNumber ??= $"PDF-{System.IO.Path.GetFileNameWithoutExtension(pdfPath)}";

			var option = FindOption("Invoice Scan") ?? BuildFallbackOption("Invoice Scan");
			await StartScanAsync(option, pdfPath, attachmentIsPhoto: false);

			CurrentItem.InvoiceNumber ??= $"PDF-{System.IO.Path.GetFileNameWithoutExtension(pdfPath)}";
			HasResult = true;
		}

		/// <summary>
		/// Stores a user-picked product photo path and uses it as the preview image.
		/// </summary>
		public async Task AttachProductPhoto(string photoPath)
		{
			if (string.IsNullOrWhiteSpace(photoPath))
				return;

			ProductPhotoPath = photoPath;
			CurrentItem.ImagePreview = photoPath;
			CurrentItem.Source = "Product Photo Upload";
			CurrentItem.Icon = "🖼";
			CurrentItem.Status = ScanStatus.Extracted;
			HasResult = true;
			ScanStatus = ScanStatus.Extracted;
			ScanStatusText = "Image added. Add the remaining details if needed.";
		}

		[RelayCommand]
		public async Task UploadInvoicePdf()
		{
			// Simulate uploading a PDF and immediately AI-extracting its contents.
			var option = FindOption("Invoice Scan");
			if (option is null)
				return;

			CaptureRequested?.Invoke(this, option);
			await StartScanAsync(option);
		}

		[RelayCommand]
		public async Task UploadProductPhoto()
		{
			// Simulate uploading a product photo. The user fills the rest.
			IsScanning = true;
			ScanStatus = ScanStatus.Scanning;
			ScanStatusText = "Product photo upload selected. Uploading photo…";
			ValidationMessage = string.Empty;
			HasValidationError = false;

			await Task.Delay(1500);

			var photoItem = new ScanItem
			{
				ScanId = new Random().Next(1000, 9999),
				Icon = "🖼",
				Source = "Product Photo Upload",
				ImagePreview = "dotnet_bot.png"
			};

			CurrentItem = photoItem;
			IsScanning = false;
			ScanStatus = ScanStatus.Extracted;
			ScanStatusText = "Product photo upload complete. Review the details and tap Save.";
		}

		[RelayCommand]
		public async Task HeroCapture() => await TriggerCaptureAsync("Invoice Scan");

		// ====================================================================
		//  Internals
		// ====================================================================

		//private void CurrentItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
		//{
		//	if (e.PropertyName == nameof(ScanItem.WarrantyEndDate))
		//	{
		//		if (CurrentItem.WarrantyEndDate.HasValue)
		//		{
		//			var offset = (CurrentItem.WarrantyEndDate.Value.Date - DateTime.Today).Days;
		//			if (WarrantyOffsetDaysFromToday != offset)
		//				WarrantyOffsetDaysFromToday = offset;
		//		}
		//		else if (WarrantyOffsetDaysFromToday.HasValue)
		//		{
		//			WarrantyOffsetDaysFromToday = null;
		//		}
		//	}

		//	OnPropertyChanged(nameof(CanSave));
		//	OnPropertyChanged(nameof(HasManualData));
		//	OnPropertyChanged(nameof(HasResult));
		//}

		private void ApplyExtractedToCurrentItem(ScanItem extracted, bool attachmentIsPhoto, string? attachmentPath)
		{
			if (!string.IsNullOrWhiteSpace(extracted.ProductName))
				CurrentItem.ProductName = extracted.ProductName;
			if (!string.IsNullOrWhiteSpace(extracted.Brand))
				CurrentItem.Brand = extracted.Brand;
			if (!string.IsNullOrWhiteSpace(extracted.Category))
				CurrentItem.Category = extracted.Category;
			if (extracted.Amount > 0m)
				CurrentItem.Amount = extracted.Amount;

			if (!string.IsNullOrWhiteSpace(extracted.ImagePreview))
				CurrentItem.ImagePreview = extracted.ImagePreview;
			else if (attachmentIsPhoto && !string.IsNullOrWhiteSpace(attachmentPath))
				CurrentItem.ImagePreview = attachmentPath;

			if (!string.IsNullOrWhiteSpace(extracted.Source))
				CurrentItem.Source = extracted.Source;
			if (!string.IsNullOrWhiteSpace(extracted.Icon))
				CurrentItem.Icon = extracted.Icon;
			CurrentItem.Status = extracted.Status;
		}

		private async Task TriggerCaptureAsync(string optionTitle)
		{
			var option = FindOption(optionTitle);
			if (option is null)
				return;

			CaptureRequested?.Invoke(this, option);
			await StartScanAsync(option);
		}

		private ScanOption? FindOption(string titleContains) =>
			ScanOptions.FirstOrDefault(o =>
				(o.Title ?? string.Empty).Contains(titleContains, StringComparison.OrdinalIgnoreCase));

		private ScanOption BuildFallbackOption(string title) => new()
		{
			Title = title,
			Description = $"Captured via {title}.",
			Icon = "📄",
			BackgroundColor = "#EEF1F7",
			IconColor = "#1D4ED8"
		};

		//private DateTime? ComputeWarrantyEndDate() => ComputeWarrantyEndDateFrom(CurrentItem);

		//private static DateTime? ComputeWarrantyEndDateFrom(ScanItem item)
		//{
		//	if (item.WarrantyEndDate is not null)
		//		return item.WarrantyEndDate.Value.Date;

		//	if (item.PurchaseDate is null)
		//		return null;

		//	var terms = (item.WarrantyTerms ?? string.Empty).Trim();
		//	if (string.IsNullOrWhiteSpace(terms))
		//		return null;

		//	var lower = terms.ToLowerInvariant();
		//	var digits = new string(lower.Where(char.IsDigit).ToArray());
		//	if (!int.TryParse(digits, out var quantity) || quantity <= 0)
		//		return null;

		//	if (lower.Contains("year"))
		//		return item.PurchaseDate.Value.Date.AddYears(quantity);
		//	if (lower.Contains("month"))
		//		return item.PurchaseDate.Value.Date.AddMonths(quantity);
		//	if (lower.Contains("day"))
		//		return item.PurchaseDate.Value.Date.AddDays(quantity);

		//	return null;
		//}

		private string ComputeWarrantyStatus(DateTime? warrantyEndDate)
		{
			if (warrantyEndDate is null)
				return "Protected";

			var days = (warrantyEndDate.Value - DateTime.Today).TotalDays;
			if (days < 0) return "Expired";
			if (days < 90) return "Expiring Soon";
			return "Protected";
		}

		private string Categorize(string productName, string brand)
		{
			var name = (productName ?? string.Empty).ToLowerInvariant();
			if (name.Contains("refrigerator") || name.Contains("washing") ||
				name.Contains("microwave") || name.Contains("conditioner") ||
				name.Contains("vacuum"))
				return "Appliances";

			if (name.Contains("phone") || name.Contains("watch") ||
				name.Contains("robot") || name.Contains("smart"))
				return "Smart Devices";

			if (name.Contains("table") || name.Contains("sofa") ||
				name.Contains("chair") || name.Contains("bed"))
				return "Furniture";

			if (name.Contains("coffee") || name.Contains("oven") ||
				name.Contains("grill") || name.Contains("espresso"))
				return "Kitchen";

			return "Electronics";
		}

		private int EstimateLifespan(string category) => category switch
		{
			"Appliances" => 12,
			"Electronics" => 8,
			"Smart Devices" => 5,
			"Kitchen" => 10,
			"Furniture" => 20,
			_ => 10
		};
	}
}