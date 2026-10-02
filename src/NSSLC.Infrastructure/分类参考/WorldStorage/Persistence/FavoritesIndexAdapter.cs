using System.Text;
using System.Text.Json;
using Terraria.NonAuthoritative.Platform;

namespace Terraria.NonAuthoritative.Persistence;

public sealed class FavoritesIndexAdapter
{
  private readonly FilePlatformAdapter? _platform;
  private readonly string? _path;
  private readonly bool _isCloudSave;
  private readonly Dictionary<SaveFileType, Dictionary<string, bool>> _favorites = new();

  public FavoritesIndexAdapter()
  {
  }

  public FavoritesIndexAdapter(FilePlatformAdapter platform, string path, bool isCloudSave)
  {
    _platform = platform ?? throw new ArgumentNullException(nameof(platform));
    _path = string.IsNullOrWhiteSpace(path)
      ? throw new ArgumentException("A favorites path is required.", nameof(path))
      : path;
    _isCloudSave = isCloudSave;
  }

  public bool IsFavorite(SaveFileReferenceSnapshot file)
  {
    return _favorites.TryGetValue(file.Type, out Dictionary<string, bool>? files) &&
      files.TryGetValue(file.GetFileName(), out bool isFavorite) &&
      isFavorite;
  }

  public FilePlatformOperationResult SetFavorite(SaveFileReferenceSnapshot file, bool isFavorite)
  {
    SetFavoriteInMemory(file, isFavorite);
    return Save();
  }

  public FilePlatformOperationResult SetFavorite(
    SaveFileReferenceSnapshot file,
    bool isFavorite,
    bool persist)
  {
    SetFavoriteInMemory(file, isFavorite);
    return persist ? Save() : FilePlatformOperationResult.Success;
  }

  public FilePlatformOperationResult Load()
  {
    FilePlatformAdapter platform = RequirePlatform();
    FileExistenceResult existence = platform.Exists(RequirePath(), _isCloudSave);
    if (!existence.Exists)
    {
      return existence.Failure.Kind == FilePlatformFailureKind.None
        ? ClearAndReturnSuccess()
        : FilePlatformOperationResult.Failed(existence.Failure);
    }

    FileReadResult read = platform.ReadAllBytes(RequirePath(), _isCloudSave);
    if (!read.Succeeded || read.Data is null)
    {
      return FilePlatformOperationResult.Failed(read.Failure);
    }

    string json;
    try
    {
      json = DecodeText(read.Data);
    }
    catch (DecoderFallbackException exception)
    {
      return FilePlatformOperationResult.Failed(
        FilePlatformFailure.Create(FilePlatformFailureKind.InvalidData, exception.Message));
    }

    Dictionary<SaveFileType, Dictionary<string, bool>> loaded;
    try
    {
      loaded = Parse(json);
    }
    catch (JsonException exception)
    {
      return FilePlatformOperationResult.Failed(
        FilePlatformFailure.Create(FilePlatformFailureKind.InvalidData, exception.Message));
    }

    _favorites.Clear();
    foreach ((SaveFileType type, Dictionary<string, bool> values) in loaded)
    {
      _favorites.Add(type, values);
    }

    return FilePlatformOperationResult.Success;
  }

  public FilePlatformOperationResult Save()
  {
    FilePlatformAdapter platform = RequirePlatform();
    Dictionary<string, Dictionary<string, bool>> document = new(StringComparer.Ordinal);
    foreach ((SaveFileType type, Dictionary<string, bool> values) in _favorites)
    {
      document[type.ToString()] = new Dictionary<string, bool>(values, StringComparer.Ordinal);
    }

    string json = JsonSerializer.Serialize(document, new JsonSerializerOptions { WriteIndented = true });
    byte[] bytes = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true).GetBytes(json);
    return platform.WriteAllBytes(RequirePath(), bytes, _isCloudSave);
  }

  public IReadOnlyDictionary<string, bool> Snapshot(SaveFileType type)
  {
    if (!_favorites.TryGetValue(type, out Dictionary<string, bool>? files))
    {
      return new Dictionary<string, bool>();
    }

    return new Dictionary<string, bool>(files, StringComparer.Ordinal);
  }

  private static string DecodeText(byte[] bytes)
  {
    try
    {
      return new UTF8Encoding(encoderShouldEmitUTF8Identifier: true, throwOnInvalidBytes: true).GetString(bytes);
    }
    catch (DecoderFallbackException)
    {
      return Encoding.ASCII.GetString(bytes);
    }
  }

  private static Dictionary<SaveFileType, Dictionary<string, bool>> Parse(string json)
  {
    Dictionary<string, Dictionary<string, bool>>? document =
      JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, bool>>>(json);
    Dictionary<SaveFileType, Dictionary<string, bool>> result = new();
    if (document is null)
    {
      return result;
    }

    foreach ((string typeName, Dictionary<string, bool>? values) in document)
    {
      if (!Enum.TryParse(typeName, ignoreCase: true, out SaveFileType type) ||
        !FileMetadataCodec.IsKnownType(type) ||
        values is null)
      {
        continue;
      }

      result[type] = new Dictionary<string, bool>(values, StringComparer.Ordinal);
    }

    return result;
  }

  private FilePlatformOperationResult ClearAndReturnSuccess()
  {
    _favorites.Clear();
    return FilePlatformOperationResult.Success;
  }

  private void SetFavoriteInMemory(SaveFileReferenceSnapshot file, bool isFavorite)
  {
    if (!_favorites.TryGetValue(file.Type, out Dictionary<string, bool>? files))
    {
      files = new Dictionary<string, bool>(StringComparer.Ordinal);
      _favorites.Add(file.Type, files);
    }

    files[file.GetFileName()] = isFavorite;
  }

  private FilePlatformAdapter RequirePlatform()
  {
    return _platform ?? throw new InvalidOperationException(
      "A file platform adapter is required for persisted favorites operations.");
  }

  private string RequirePath()
  {
    return _path ?? throw new InvalidOperationException(
      "A favorites path is required for persisted favorites operations.");
  }
}
