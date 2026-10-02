using Terraria.NonAuthoritative.Platform;

namespace Terraria.NonAuthoritative.Persistence;

public readonly record struct SaveFileReferenceSnapshot
{
  public SaveFileReferenceSnapshot(
    string path,
    bool isCloudSave,
    SaveFileType type,
    bool isFavorite)
  {
    Path = string.IsNullOrWhiteSpace(path)
      ? throw new ArgumentException("A save file path is required.", nameof(path))
      : path;
    if (!FileMetadataCodec.IsKnownType(type))
    {
      throw new ArgumentOutOfRangeException(nameof(type), "A known save file type is required.");
    }

    IsCloudSave = isCloudSave;
    Type = type;
    IsFavorite = isFavorite;
    Name = FilePathParsingAdapter.GetFileName(path);
  }

  public string Path { get; }

  public bool IsCloudSave { get; }

  public SaveFileType Type { get; }

  public bool IsFavorite { get; }

  public string Name { get; }

  public string GetFileName(bool includeExtension = true)
  {
    return FilePathParsingAdapter.GetFileName(Path, includeExtension);
  }
}
