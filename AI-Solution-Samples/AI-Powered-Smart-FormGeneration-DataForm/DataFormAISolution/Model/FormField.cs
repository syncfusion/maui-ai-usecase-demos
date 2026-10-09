using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace DataFormAISolution
{
	/// <summary>
	/// Represents a simple sample model.
	/// </summary>
	public class TestModel
	{
		/// <summary>
		/// Gets or sets the sample name.
		/// </summary>
		public string Name { get; set; } = "Karthi";

		/// <summary>
		/// Gets or sets the sample percentage.
		/// </summary>
		public double SSLCPercentage { get; set; } = 60;
	}

	/// <summary>
	/// Represents a generated field returned by the AI service.
	/// </summary>
	public class AIField
	{
		/// <summary>
		/// Gets or sets the field name.
		/// </summary>
		public string FieldName { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets the field value.
		/// </summary>
		public string Value { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets the field type.
		/// </summary>
		public string FieldType { get; set; } = "Text";
	}

	/// <summary>
	/// Represents the AI response payload containing generated fields.
	/// </summary>
	public class AIFormResponse
	{
		/// <summary>
		/// Gets or sets the generated fields.
		/// </summary>
		public List<AIField> Fields { get; set; } = new();
	}
}
