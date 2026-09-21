using System.Numerics;

namespace Terraria.Presentation.CombatText;

public readonly record struct CombatTextSpawnCommand
{
  public CombatTextSpawnCommand(
    long eventId,
    string text,
    Vector2 position,
    Vector2 velocity,
    CombatTextColorRole colorRole,
    float scale,
    float rotation,
    bool isCritical,
    bool isDamageOverTime,
    int lifetimeTicks)
  {
    if (string.IsNullOrEmpty(text))
    {
      throw new ArgumentException("Combat text must not be empty.", nameof(text));
    }
    if (lifetimeTicks <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(lifetimeTicks));
    }

    EventId = eventId;
    Text = text;
    Position = position;
    Velocity = velocity;
    ColorRole = colorRole;
    Scale = scale;
    Rotation = rotation;
    IsCritical = isCritical;
    IsDamageOverTime = isDamageOverTime;
    LifetimeTicks = lifetimeTicks;
  }

  public CombatTextColorRole ColorRole { get; }

  public long EventId { get; }

  public bool IsCritical { get; }

  public bool IsDamageOverTime { get; }

  public int LifetimeTicks { get; }

  public Vector2 Position { get; }

  public float Rotation { get; }

  public float Scale { get; }

  public string Text { get; }

  public Vector2 Velocity { get; }
}
