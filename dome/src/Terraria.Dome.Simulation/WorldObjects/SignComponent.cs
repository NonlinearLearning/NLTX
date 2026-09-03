using System;

namespace Terraria.Dome.Simulation.WorldObjects;

public readonly record struct SignComponent(
  int SignId,
  int TileX,
  int TileY,
  string Text,
  long Revision = 1)
{
  public const int MaximumTextLength = 100;

  public bool IsActive => SignId > 0;

  public SignComponent Validate()
  {
    if (SignId <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(SignId));
    }

    ArgumentNullException.ThrowIfNull(Text);
    if (Text.Length > MaximumTextLength)
    {
      throw new ArgumentOutOfRangeException(nameof(Text));
    }
    if (Revision < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(Revision));
    }

    return this;
  }

  public bool TryEdit(string text, SignAuthorizationPolicy authorization, out SignComponent edited)
  {
    edited = this;
    if (!authorization.IsAllowed ||
        text is null ||
        text.Length > MaximumTextLength ||
        Revision == long.MaxValue)
    {
      return false;
    }

    edited = this with { Text = text, Revision = Revision + 1 };
    return true;
  }

  public bool TryDelete(long expectedRevision, out SignLifecycleTransition transition)
  {
    if (expectedRevision != Revision || Revision == long.MaxValue)
    {
      transition = default;
      return false;
    }

    transition = SignLifecycleTransition.Delete(Revision + 1);
    return true;
  }
}
