namespace Terraria.WorldStorage;

public sealed class WorldSectionState
{
  public const byte LoadedMask = 1 << 0;
  public const byte FramedMask = 1 << 1;
  public const byte MapDrawnMask = 1 << 2;
  public const byte NeedsRefreshMask = 1 << 3;

  private byte[] _flags = Array.Empty<byte>();
  private int _sectionCountX;
  private int _sectionCountY;
  private int _mapSectionsRemaining;
  private SectionIterationState _frameIteration;
  private SectionIterationState _mapIteration;
  private long _revision;

  public int SectionCountX => _sectionCountX;
  public int SectionCountY => _sectionCountY;
  public int Count => _flags.Length;
  public int MapSectionsRemaining => _mapSectionsRemaining;
  public long Revision => _revision;
}
