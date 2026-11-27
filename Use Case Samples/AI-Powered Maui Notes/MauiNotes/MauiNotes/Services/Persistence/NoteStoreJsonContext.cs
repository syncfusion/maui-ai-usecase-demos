using System.Text.Json.Serialization;

namespace MauiNotes.Services.Persistence;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(NoteStoreDocument))]
internal partial class NoteStoreJsonContext : JsonSerializerContext
{
}
