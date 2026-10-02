namespace Terraria.Network.Section;

public sealed class WorldSectionStreamingStateComponent
{
  public const int BitIndexSectionLoaded = 0;

  public const int BitIndexSectionFramed = 1;

  public const int BitIndexSectionMapDrawn = 2;

  public const int BitIndexSectionNeedsRefresh = 3;

  private readonly byte[] _sectionFlags;

  public WorldSectionStreamingStateComponent(
    int width,
    int height,
    byte[]? sectionFlags = null,
    int? mapSectionsLeft = null)
  {
    if (width <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }

    if (height <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(height));
    }

    int sectionCount = checked(width * height);
    if (sectionFlags is not null && sectionFlags.Length != sectionCount)
    {
      throw new ArgumentException(
        "Section flags must contain one entry for every section.",
        nameof(sectionFlags));
    }

    int remainingSections = mapSectionsLeft ?? sectionCount;
    if (remainingSections < 0 || remainingSections > sectionCount)
    {
      throw new ArgumentOutOfRangeException(nameof(mapSectionsLeft));
    }

    Width = width;
    Height = height;
    MapSectionsLeft = remainingSections;
    _sectionFlags = sectionFlags is null
      ? new byte[sectionCount]
      : (byte[])sectionFlags.Clone();
  }

  public int Width { get; }

  public int Height { get; }

  public int SectionCount => _sectionFlags.Length;

  public int MapSectionsLeft { get; }

  public byte[] SectionFlags => (byte[])_sectionFlags.Clone();
}
