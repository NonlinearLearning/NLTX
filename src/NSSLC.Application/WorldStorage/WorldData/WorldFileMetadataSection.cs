namespace Terraria.NonAuthoritative.Persistence;

/// <summary>
/// File metadata stored before the pointer table in a WorldFile.
/// </summary>
/// <remarks>
/// This is local file metadata only. It does not describe a cloud provider or a cloud-save path.
/// </remarks>
public sealed class WorldFileMetadataSection
{
  public const string SectionId = "world.metadata";

  public WorldFileMetadataSection(uint revision, bool isFavorite)
  {
    Revision = revision;
    IsFavorite = isFavorite;
  }

  public uint Revision { get; }

  public bool IsFavorite { get; }

  public static WorldFileMetadataSection Empty => new(1, false);
}
