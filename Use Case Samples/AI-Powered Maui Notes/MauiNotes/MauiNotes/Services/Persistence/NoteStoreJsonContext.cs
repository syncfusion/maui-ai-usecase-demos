using System.Text.Json.Serialization;

namespace MauiNotes.Services.Persistence;

[JsonSerializable(typeof(NoteStoreDocument))]
[JsonSerializable(typeof(NoteFolderData))]
[JsonSerializable(typeof(NoteData))]
[JsonSourceGenerationOptions(WriteIndented = true, PropertyNameCaseInsensitive = true)]
public partial class NoteStoreJsonContext : JsonSerializerContext;
