using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace DataFormAISolution
{
	/// <summary>
	/// Holds user input and generated fields for the form-generation flow.
	/// </summary>
	public partial class FormViewModel : ObservableObject
	{
		/// <summary>
		/// Creates a new instance of the <see cref="FormViewModel"/> class.
		/// </summary>
		private readonly IAIFormService _aiFormService;

		private CancellationTokenSource? _cancellationTokenSource;

		/// <summary>
		/// Gets the generated fields.
		/// </summary>
		public ObservableCollection<AIField> GeneratedFields
		{
			get;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FormViewModel"/> class.
		/// </summary>
		public FormViewModel()
		{
			_aiFormService = new AzureFormAIService();
			GeneratedFields =
new ObservableCollection<AIField>();
		}

		/// <summary>
		/// Gets or sets the user input.
		/// </summary>
		[ObservableProperty]
		private string userInput = string.Empty;

		/// <summary>
		/// Gets or sets a value indicating whether the viewmodel is busy.
		/// </summary>
		[ObservableProperty]
		private bool isBusy;

		/// <summary>
		/// Gets or sets a value indicating whether the form is visible.
		/// </summary>
		[ObservableProperty]
		private bool showForm;

		private int _formGenerationVersion;

		/// <summary>
		/// Gets the current form generation version.
		/// </summary>
		public int FormGenerationVersion
		{
			get => _formGenerationVersion;

			private set
			{
				SetProperty(
				ref _formGenerationVersion,
				value);
			}
		}

		/// <summary>
		/// Generates a form from the current user input.
		/// </summary>
		[RelayCommand]
		public async Task GenerateFormAsync()
		{
			if (string.IsNullOrWhiteSpace(UserInput))
				return;

			IsBusy = true;
			ShowForm = false;

			try
			{
				_cancellationTokenSource?.Cancel();

				_cancellationTokenSource =
				new CancellationTokenSource();

				var response =
				await _aiFormService.GenerateFormAsync(
				UserInput,
				_cancellationTokenSource.Token);

				if (response == null ||
				response.Fields == null ||
				response.Fields.Count == 0)
				{
					Debug.WriteLine(
					"AI returned no form fields.");

					return;
				}

				GeneratedFields.Clear();

				foreach (var field in response.Fields)
				{
					Debug.WriteLine(
					$"{field.FieldName} | " +
					$"{field.Value} | " +
					$"{field.FieldType}");

					GeneratedFields.Add(field);
				}

				ShowForm = true;

				// Notify View that a completely new
				// AI-generated schema is available.
				FormGenerationVersion++;
			}
			catch (OperationCanceledException)
			{
				Debug.WriteLine(
				"AI form generation cancelled.");
			}
			catch (Exception ex)
			{
				Debug.WriteLine(
				$"GenerateFormAsync Error: {ex}");
			}
			finally
			{
				IsBusy = false;
				ShowForm = true;
			}
		}
	}

}
