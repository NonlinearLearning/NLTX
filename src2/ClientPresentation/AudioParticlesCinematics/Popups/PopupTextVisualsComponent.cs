using System.Numerics;

namespace NLTX.ClientPresentation.AudioParticlesCinematics.Popups;

public sealed class PopupTextVisualsComponent
{
  private Vector2[] _charOffsets;
  private uint[] _charColors;

  public PopupTextVisualsComponent(
    Vector2 position,
    Vector2 velocity,
    float alpha = 0,
    int alphaDirection = 1,
    float scale = 1,
    float rotation = 0,
    uint color = 0,
    bool active = false,
    int lifeTime = 0,
    int framesSinceSpawn = 0,
    bool noStack = false,
    IEnumerable<Vector2>? charOffsets = null,
    IEnumerable<uint>? charColors = null,
    int effectStyleCode = 0,
    int effectIntensity = 0)
  {
    if (lifeTime < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(lifeTime));
    }

    if (framesSinceSpawn < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(framesSinceSpawn));
    }

    if (effectIntensity < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(effectIntensity));
    }

    Position = position;
    Velocity = velocity;
    Alpha = alpha;
    AlphaDirection = alphaDirection;
    Scale = scale;
    Rotation = rotation;
    Color = color;
    Active = active;
    LifeTime = lifeTime;
    FramesSinceSpawn = framesSinceSpawn;
    NoStack = noStack;
    EffectStyleCode = effectStyleCode;
    EffectIntensity = effectIntensity;
    _charOffsets = CopyOrEmpty(charOffsets);
    _charColors = CopyOrEmpty(charColors);
  }

  public Vector2 Position { get; internal set; }

  public Vector2 Velocity { get; internal set; }

  public float Alpha { get; internal set; }

  public int AlphaDirection { get; internal set; }

  public float Scale { get; internal set; }

  public float Rotation { get; internal set; }

  // Packed color value remains an opaque presentation value until the graphics boundary is owned.
  public uint Color { get; internal set; }

  public bool Active { get; internal set; }

  public int LifeTime { get; internal set; }

  public int FramesSinceSpawn { get; internal set; }

  public bool NoStack { get; internal set; }

  public int EffectStyleCode { get; internal set; }

  public int EffectIntensity { get; internal set; }

  public IReadOnlyList<Vector2> CharOffsets => _charOffsets;

  public IReadOnlyList<uint> CharColors => _charColors;

  internal void SetCharacterColors(IEnumerable<Vector2> charOffsets, IEnumerable<uint> charColors)
  {
    ArgumentNullException.ThrowIfNull(charOffsets);
    ArgumentNullException.ThrowIfNull(charColors);

    _charOffsets = charOffsets.ToArray();
    _charColors = charColors.ToArray();
  }

  private static T[] CopyOrEmpty<T>(IEnumerable<T>? values)
  {
    return values?.ToArray() ?? [];
  }
}
