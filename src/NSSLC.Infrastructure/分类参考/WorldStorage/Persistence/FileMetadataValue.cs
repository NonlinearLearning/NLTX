namespace Terraria.NonAuthoritative.Persistence;

public readonly record struct FileMetadataValue
{
  public SaveFileType Type { get; }

  public uint Revision { get; }

  public bool IsFavorite { get; }

  public FileMetadataValue(
    SaveFileType type,
    uint revision,
    bool isFavorite)
    : this()
  {
    if (!FileMetadataCodec.IsKnownType(type))
    {
      throw new ArgumentOutOfRangeException(nameof(type), "A known save file type is required.");
    }

    Type = type;
    Revision = revision;
    IsFavorite = isFavorite;
  }
}
