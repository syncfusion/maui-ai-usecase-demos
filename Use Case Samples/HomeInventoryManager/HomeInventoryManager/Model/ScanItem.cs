using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace HomeInventoryManager.Model
{
	// Represents an item scanned/added through the Scan & Add Item module.
	// Holds fields that get extracted from invoices/warranty cards/product labels
	// and the AI classification results that the user can validate before saving.
	public class ScanItem : INotifyPropertyChanged
	{
		private int scanId;
		private string productName = string.Empty;
		private string brand = string.Empty;
		private string model = string.Empty;
		private string serialNumber = string.Empty;
		private string invoiceNumber = string.Empty;
		private string vendor = string.Empty;
		private DateTime purchaseDate = DateTime.Now;
		private decimal amount;
		private string warrantyTerms = string.Empty;
		private string warrantyProvider = string.Empty;
		private DateTime warrantyEndDate = DateTime.Now;
		private string coverageType = string.Empty;
		private string category = string.Empty;
		private string subcategory = string.Empty;
		private int estimatedLifespan;
		private string maintenanceFrequency = string.Empty;

		// PDFs / receipts / warranty documents go here.
		private string documentUrl = string.Empty;

		// Captured or picked photos go here.
		private string imagePreview = string.Empty;

		private string icon = string.Empty;
		private string source = string.Empty;
		private ScanStatus status;

		public event PropertyChangedEventHandler? PropertyChanged;

		private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		public int ScanId
		{
			get => scanId;
			set
			{
				if (scanId == value) return;
				scanId = value;
				OnPropertyChanged();
			}
		}

		public string ProductName
		{
			get => productName;
			set
			{
				if (productName == value) return;
				productName = value ?? string.Empty;
				OnPropertyChanged();
			}
		}

		public string Brand
		{
			get => brand;
			set
			{
				if (brand == value) return;
				brand = value ?? string.Empty;
				OnPropertyChanged();
			}
		}

		public string Model
		{
			get => model;
			set
			{
				if (model == value) return;
				model = value ?? string.Empty;
				OnPropertyChanged();
			}
		}

		public string SerialNumber
		{
			get => serialNumber;
			set
			{
				if (serialNumber == value) return;
				serialNumber = value ?? string.Empty;
				OnPropertyChanged();
			}
		}

		public string InvoiceNumber
		{
			get => invoiceNumber;
			set
			{
				if (invoiceNumber == value) return;
				invoiceNumber = value ?? string.Empty;
				OnPropertyChanged();
			}
		}

		public string Vendor
		{
			get => vendor;
			set
			{
				if (vendor == value) return;
				vendor = value ?? string.Empty;
				OnPropertyChanged();
			}
		}

		public DateTime PurchaseDate
		{
			get => purchaseDate;
			set
			{
				if (purchaseDate == value) return;
				purchaseDate = value;
				OnPropertyChanged();
			}
		}

		public decimal Amount
		{
			get => amount;
			set
			{
				if (amount == value) return;
				amount = value;
				OnPropertyChanged();
			}
		}

		public string WarrantyTerms
		{
			get => warrantyTerms;
			set
			{
				if (warrantyTerms == value) return;
				warrantyTerms = value ?? string.Empty;
				OnPropertyChanged();
			}
		}

		public string WarrantyProvider
		{
			get => warrantyProvider;
			set
			{
				if (warrantyProvider == value) return;
				warrantyProvider = value ?? string.Empty;
				OnPropertyChanged();
			}
		}

		public DateTime WarrantyEndDate
		{
			get => warrantyEndDate;
			set
			{
				if (warrantyEndDate == value) return;
				warrantyEndDate = value;
				OnPropertyChanged();
			}
		}

		public string CoverageType
		{
			get => coverageType;
			set
			{
				if (coverageType == value) return;
				coverageType = value ?? string.Empty;
				OnPropertyChanged();
			}
		}

		public string Category
		{
			get => category;
			set
			{
				if (category == value) return;
				category = value ?? string.Empty;
				OnPropertyChanged();
			}
		}

		public string Subcategory
		{
			get => subcategory;
			set
			{
				if (subcategory == value) return;
				subcategory = value ?? string.Empty;
				OnPropertyChanged();
			}
		}

		public int EstimatedLifespan
		{
			get => estimatedLifespan;
			set
			{
				if (estimatedLifespan == value) return;
				estimatedLifespan = value;
				OnPropertyChanged();
			}
		}

		public string MaintenanceFrequency
		{
			get => maintenanceFrequency;
			set
			{
				if (maintenanceFrequency == value) return;
				maintenanceFrequency = value ?? string.Empty;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Path or URL to an attached PDF / receipt / warranty document.
		/// Use this for non-image attachments (e.g. <c>UploadInvoicePdf</c>).
		/// </summary>
		public string DocumentUrl
		{
			get => documentUrl;
			set
			{
				if (documentUrl == value) return;
				documentUrl = value ?? string.Empty;
				OnPropertyChanged();
				OnPropertyChanged(nameof(HasDocument));
				OnPropertyChanged(nameof(HasAttachment));
				OnPropertyChanged(nameof(AttachmentDisplayName));
				OnPropertyChanged(nameof(DocumentKind));
			}
		}

		/// <summary>
		/// Path or filename for a captured or picked image (camera / gallery).
		/// </summary>
		public string ImagePreview
		{
			get => imagePreview;
			set
			{
				if (imagePreview == value) return;
				imagePreview = value ?? string.Empty;
				OnPropertyChanged();
				OnPropertyChanged(nameof(HasImage));
				OnPropertyChanged(nameof(HasAttachment));
				OnPropertyChanged(nameof(AttachmentDisplayName));
				OnPropertyChanged(nameof(DocumentKind));
			}
		}

		/// <summary>Icon shown on the extracted-fields card header.</summary>
		public string Icon
		{
			get => icon;
			set
			{
				if (icon == value) return;
				icon = value ?? string.Empty;
				OnPropertyChanged();
			}
		}

		/// <summary>The scan method (Invoice / Warranty Card / Product Label / Manual).</summary>
		public string Source
		{
			get => source;
			set
			{
				if (source == value) return;
				source = value ?? string.Empty;
				OnPropertyChanged();
			}
		}

		public ScanStatus Status
		{
			get => status;
			set
			{
				if (status == value) return;
				status = value;
				OnPropertyChanged();
			}
		}

		// ------------------------------------------------------------
		//  Helpers used by the Scan page UI
		// ------------------------------------------------------------

		public bool HasImage => !string.IsNullOrWhiteSpace(imagePreview);
		public bool HasDocument => !string.IsNullOrWhiteSpace(documentUrl);
		public bool HasAttachment => HasImage || HasDocument;

		/// <summary>File name only, for display in the UI.</summary>
		public string AttachmentDisplayName
		{
			get
			{
				if (HasDocument) return Path.GetFileName(documentUrl);
				if (HasImage) return Path.GetFileName(imagePreview);
				return string.Empty;
			}
		}

		/// <summary>What kind of file is attached (or <see cref="DocumentKind.None"/>).</summary>
		public DocumentKind DocumentKind
		{
			get
			{
				if (HasDocument) return DocumentKind.Pdf;
				if (HasImage) return DocumentKind.Photo;
				return DocumentKind.None;
			}
		}

		// ------------------------------------------------------------
		//  Factory helpers
		// ------------------------------------------------------------

		/// <summary>Attach a PDF / document to this item.</summary>
		public void AttachDocument(string path)
		{
			if (string.IsNullOrWhiteSpace(path)) return;
			DocumentUrl = path;
			if (string.IsNullOrWhiteSpace(InvoiceNumber))
				InvoiceNumber = $"PDF-{Path.GetFileNameWithoutExtension(path)}";
		}

		/// <summary>Attach a photo to this item.</summary>
		public void AttachImage(string path)
		{
			if (string.IsNullOrWhiteSpace(path)) return;
			ImagePreview = path;
		}
	}

	public enum ScanStatus
	{
		Pending,
		Scanning,
		Extracted,
		Validated,
		Saved
	}

	/// <summary>
	/// What kind of file (if any) is attached to a <see cref="ScanItem"/>.
	/// Lives in the <see cref="HomeInventoryManager.Model"/> namespace so
	/// XAML data bindings can resolve it as <c>DocumentKind</c> when an
	/// xmlns for the model namespace is declared.
	/// </summary>
	public enum DocumentKind
	{
		None,
		Photo,
		Pdf
	}

	/// <summary>
	/// Represents one of the scan entry methods shown on the Scan Item page
	/// (Invoice Scan, Warranty Card Scan, Product Label Scan, Manual Entry).
	/// </summary>
	public class ScanOption
	{
		public string Title { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public string Icon { get; set; } = string.Empty;
		public string BackgroundColor { get; set; } = "#FFFFFF";
		public string IconColor { get; set; } = "#0D33B8";
	}
}