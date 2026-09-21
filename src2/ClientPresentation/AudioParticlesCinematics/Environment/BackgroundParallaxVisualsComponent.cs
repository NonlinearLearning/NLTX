namespace NLTX.ClientPresentation.AudioParticlesCinematics.Environment;

public sealed class BackgroundParallaxVisualsComponent
{
  private readonly int[] _treePositions;
  private readonly int[] _treeStyles;
  private readonly int[] _cavePositions;
  private readonly int[] _caveStyles;

  public BackgroundParallaxVisualsComponent(
    IEnumerable<int>? treePositions = null,
    IEnumerable<int>? treeStyles = null,
    IEnumerable<int>? cavePositions = null,
    IEnumerable<int>? caveStyles = null)
  {
    _treePositions = treePositions?.ToArray() ?? Array.Empty<int>();
    _treeStyles = treeStyles?.ToArray() ?? Array.Empty<int>();
    _cavePositions = cavePositions?.ToArray() ?? Array.Empty<int>();
    _caveStyles = caveStyles?.ToArray() ?? Array.Empty<int>();
  }

  public float EssScale { get; private set; } = 1;

  public int EssDirection { get; private set; } = 1;

  public IReadOnlyList<int> TreePositions => _treePositions;

  public IReadOnlyList<int> TreeStyles => _treeStyles;

  public IReadOnlyList<int> CavePositions => _cavePositions;

  public IReadOnlyList<int> CaveStyles => _caveStyles;

  public int IceBackStyle { get; private set; }

  public int HellBackStyle { get; private set; }

  public int JungleBackStyle { get; private set; }

  public void SetParallax(float scale, int direction)
  {
    if (scale < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(scale));
    }

    if (direction is not (-1 or 1))
    {
      throw new ArgumentOutOfRangeException(nameof(direction));
    }

    EssScale = scale;
    EssDirection = direction;
  }

  public void SetBiomeStyles(int iceBackStyle, int hellBackStyle, int jungleBackStyle)
  {
    IceBackStyle = iceBackStyle;
    HellBackStyle = hellBackStyle;
    JungleBackStyle = jungleBackStyle;
  }
}
