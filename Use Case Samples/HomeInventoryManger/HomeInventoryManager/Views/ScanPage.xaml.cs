using HomeInventoryManager.Model;
using HomeInventoryManager.ViewModel;

namespace HomeInventoryManager.Views;

public partial class ScanPage : ContentPage
{
	private readonly ScanViewModel? _vm;

	public ScanPage()
		: this(new ScanViewModel())
	{
	}

	/// <summary>DI constructor used in production.</summary>
	public ScanPage(ScanViewModel vm)
	{
		InitializeComponent();
		_vm = vm;
		BindingContext = vm;
		_vm.ItemSaved += Vm_ItemSaved;
		_vm.CancelRequested += Vm_CancelRequested;
	}

	protected override void OnDisappearing()
	{
		if (_vm is not null)
		{
			_vm.ItemSaved -= Vm_ItemSaved;
			_vm.CancelRequested -= Vm_CancelRequested;
		}
		base.OnDisappearing();
	}

	private async void Vm_CancelRequested(object? sender, EventArgs e)
	{
		Application.Current.Windows[0].Page = new ScanPage();
	}

	// ---------- Navigation ----------
	private async void Vm_ItemSaved(object? sender, InventoryItem e)
	{
		Application.Current.Windows[0].Page = new InventoryPage();

	}

	private async void Back_Tapped(object? sender, TappedEventArgs e)
	{
		Application.Current.Windows[0].Page = new InventoryPage();

	}

	private static async Task SafeGoToAsync(string route)
	{
		try
		{
			if (Shell.Current is not null)
				await Shell.Current.GoToAsync(route);
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"[ScanPage] Navigation failed: {ex.Message}");
		}
	}

	// ---------- Hero (top half capture hero) ----------
	// The hero card itself is now informational (a vibrant CTA tile). The
	// two big buttons below it drive the actual capture / upload flow and
	// move the page from ChooseMethod → ConfirmCapture.

	private async void TakePhoto_Clicked(object? sender, EventArgs e)
	{
		try
		{
			var imagePath = await CapturePhotoAsync();
			if (string.IsNullOrEmpty(imagePath))
			{
				await ShowAlertAsync("Capture cancelled", "No photo was captured.");
				return;
			}

			if (_vm is not null)
				await _vm.PhotoCapturedAsync(imagePath);
		}
		catch (FeatureNotSupportedException)
		{
			await ShowAlertAsync("Not supported", "This device has no camera. Try uploading a photo instead.");
		}
		catch (PermissionException pEx)
		{
			await ShowAlertAsync("Permission denied", pEx.Message);
		}
		catch (Exception ex)
		{
			await ShowAlertAsync("Camera failed", ex.Message);
		}
	}

	private async void UploadPhoto_Clicked(object? sender, EventArgs e)
	{
		try
		{
			var imagePath = await PickPhotoAsync();
			if (string.IsNullOrEmpty(imagePath))
			{
				await ShowAlertAsync("Upload cancelled", "No photo was selected.");
				return;
			}

			if (_vm is not null)
				await _vm.PhotoCapturedAsync(imagePath);
		}
		catch (Exception ex)
		{
			await ShowAlertAsync("Upload failed", ex.Message);
		}
	}

	private void Retake_Clicked(object? sender, EventArgs e)
	{
		if (_vm is null) return;
		if (_vm.RetakeCaptureCommand.CanExecute(null))
			_vm.RetakeCaptureCommand.Execute(null);
	}

	private void Continue_Clicked(object? sender, EventArgs e)
	{
		try
		{
			if (_vm is null) return;
			if (_vm.ContinueToReviewCommand.CanExecute(null))
				_vm.ContinueToReviewCommand.Execute(null);
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"[ScanPage] Continue_Clicked failed: {ex.Message}");
		}
	}

	private void CancelCapture_Clicked(object? sender, EventArgs e)
	{
		if (_vm is null) return;
		if (_vm.CancelFlowCommand.CanExecute(null))
			_vm.CancelFlowCommand.Execute(null);
	}

	private void CancelReview_Clicked(object? sender, EventArgs e)
	{
		if (_vm is null) return;
		if (_vm.CancelFlowCommand.CanExecute(null))
			_vm.CancelFlowCommand.Execute(null);
	}

	// ---------- Capture Options ----------
	private async void InvoiceScan_Tapped(object? sender, TappedEventArgs e)
	{
		// Let the user choose camera or gallery for an invoice image.
		var source = await AskSourceAsync("Scan Invoice");
		if (source is null) return;
		await RunScanAsync(ScanKind.Invoice, source.Value);
	}

	private async void ProductLabel_Tapped(object? sender, TappedEventArgs e)
	{
		var source = await AskSourceAsync("Product Label");
		if (source is null) return;
		await RunScanAsync(ScanKind.ProductLabel, source.Value);
	}

	private async void WarrantyCard_Tapped(object? sender, TappedEventArgs e)
	{
		var source = await AskSourceAsync("Warranty Card");
		if (source is null) return;
		await RunScanAsync(ScanKind.WarrantyCard, source.Value);
	}

	// ---------- Uploads ----------
	private async void UploadPdf_Tapped(object? sender, TappedEventArgs e)
	{
		try
		{
			var pdfType = new FilePickerFileType(
				new Dictionary<DevicePlatform, IEnumerable<string>>
				{
					{ DevicePlatform.Android, new[] { "application/pdf" } },
					{ DevicePlatform.iOS,     new[] { "com.adobe.pdf" } },
					{ DevicePlatform.WinUI,   new[] { ".pdf" } },
					{ DevicePlatform.MacCatalyst,   new[] { "pdf" } },
				});

			var result = await FilePicker.Default.PickAsync(new PickOptions
			{
				PickerTitle = "Select invoice PDF",
				FileTypes = pdfType,
			});

			if (result is null)
			{
				await ShowAlertAsync("Upload cancelled", "No PDF was selected.");
				return;
			}

			// Stream the file briefly to confirm it's readable; store path in VM.
			await using (var stream = await result.OpenReadAsync())
			{
				// We don't keep the stream alive; just confirm the file opens.
				_ = stream.Length;
			}

			if (_vm is not null)
				await _vm.AttachInvoicePdf(result.FullPath);
		}
		catch (Exception ex)
		{
			await ShowAlertAsync("Upload failed", ex.Message);
		}
	}

	private async void UploadPhoto_Tapped(object? sender, TappedEventArgs e)
	{
		try
		{
			var imageType = new FilePickerFileType(
				new Dictionary<DevicePlatform, IEnumerable<string>>
				{
					{ DevicePlatform.Android, new[] { "image/png", "image/jpeg", "image/jpg", "image/webp" } },
					{ DevicePlatform.iOS,     new[] { "public.png", "public.jpeg", "public.image" } },
					{ DevicePlatform.WinUI,   new[] { ".png", ".jpg", ".jpeg", ".webp" } },
					{ DevicePlatform.MacCatalyst,   new[] { "png", "jpeg", "jpg", "webp" } },
				});

			var result = await FilePicker.Default.PickAsync(new PickOptions
			{
				PickerTitle = "Select product photo",
				FileTypes = imageType,
			});

			if (result is null)
			{
				await ShowAlertAsync("Upload cancelled", "No photo was selected.");
				return;
			}

			if (_vm is not null)
				await _vm.AttachProductPhoto(result.FullPath);
		}
		catch (Exception ex)
		{
			await ShowAlertAsync("Upload failed", ex.Message);
		}
	}

	// ---------- Action buttons ----------
	private void Save_Clicked(object? sender, EventArgs e)
	{
		if (_vm?.SaveCommand is null) return;
		if (!_vm.SaveCommand.CanExecute(null)) return;
		_vm.SaveCommand.Execute(null);
	}

	private void Reset_Clicked(object? sender, EventArgs e)
	{
		_vm?.ResetCommand?.Execute(null);
	}

	// ============================================================
	//  Helpers
	// ============================================================

	private enum ScanKind { Invoice, ProductLabel, WarrantyCard }
	private enum CaptureSource { Camera, Gallery }

	private async Task<CaptureSource?> AskSourceAsync(string title)
	{
		try
		{
			var choice = await DisplayActionSheetAsync(
				$"Add {title} from…",
				"Cancel",
				null,
				"📷  Camera",
				"🖼  Photo Library");

			return choice switch
			{
				"📷  Camera" => CaptureSource.Camera,
				"🖼  Photo Library" => CaptureSource.Gallery,
				_ => (CaptureSource?)null,
			};
		}
		catch
		{
			// If actionsheet is unsupported, default to gallery.
			return CaptureSource.Gallery;
		}
	}

	private async Task RunScanAsync(ScanKind kind, CaptureSource source)
	{
		try
		{
			var imagePath = source switch
			{
				CaptureSource.Camera => await CapturePhotoAsync(),
				CaptureSource.Gallery => await PickPhotoAsync(),
				_ => null,
			};

			if (string.IsNullOrEmpty(imagePath))
			{
				await ShowAlertAsync("Scan cancelled", "No image was captured.");
				return;
			}

			switch (kind)
			{
				case ScanKind.Invoice:
					if (_vm is not null)
						await _vm.IngestScannedInvoice(imagePath);
					break;
				case ScanKind.ProductLabel:
					if (_vm is not null)
						await _vm.IngestScannedProductLabel(imagePath);
					break;
				case ScanKind.WarrantyCard:
					if (_vm is not null)
						await _vm.IngestScannedWarrantyCard(imagePath);
					break;
			}
		}
		catch (FeatureNotSupportedException)
		{
			await ShowAlertAsync("Not supported", "This feature is not available on this device.");
		}
		catch (PermissionException pEx)
		{
			await ShowAlertAsync("Permission denied", pEx.Message);
		}
		catch (Exception ex)
		{
			await ShowAlertAsync("Scan failed", ex.Message);
		}
	}

	private async Task<string?> CapturePhotoAsync()
	{
		if (!MediaPicker.Default.IsCaptureSupported)
		{
			// Fallback to gallery when device has no camera API.
			return await PickPhotoAsync();
		}

		var status = await EnsureCameraPermissionAsync();
		if (status != PermissionStatus.Granted)
			throw new PermissionException("Camera access is required to capture photos.");

		var photo = await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions
		{
			Title = "Capture invoice or product",
		});
		return photo?.FullPath;
	}

	private static async Task<string?> PickPhotoAsync()
	{
		var imageType = new FilePickerFileType(
			new Dictionary<DevicePlatform, IEnumerable<string>>
			{
				{ DevicePlatform.Android, new[] { "image/png", "image/jpeg", "image/jpg", "image/webp" } },
				{ DevicePlatform.iOS,     new[] { "public.png", "public.jpeg", "public.image" } },
				{ DevicePlatform.WinUI,   new[] { ".png", ".jpg", ".jpeg", ".webp" } },
				{ DevicePlatform.MacCatalyst,   new[] { "png", "jpeg", "jpg", "webp" } },
			});

		var result = await FilePicker.Default.PickAsync(new PickOptions
		{
			PickerTitle = "Pick image",
			FileTypes = imageType,
		});
		return result?.FullPath;
	}

	private static async Task<PermissionStatus> EnsureCameraPermissionAsync()
	{
		var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
		if (status == PermissionStatus.Granted)
			return status;

		if (status == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.iOS)
		{
			// Prompting on iOS the first time.
			status = await Permissions.RequestAsync<Permissions.Camera>();
			return status;
		}

		if (Permissions.ShouldShowRationale<Permissions.Camera>())
		{
			// Caller should display rationale; we just request.
		}

		status = await Permissions.RequestAsync<Permissions.Camera>();
		return status;
	}

	private Task ShowAlertAsync(string title, string message) =>
		DisplayAlertAsync(title, message, "OK");
}