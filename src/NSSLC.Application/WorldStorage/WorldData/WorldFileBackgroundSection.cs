using System;
using System.Collections.Generic;

namespace Terraria.NonAuthoritative.Persistence;

/// <summary>
/// Persisted background styles appended after the Defender's Forge state.
/// </summary>
public sealed class WorldFileBackgroundSection
{
  public const string SectionId = "world.backgrounds";

  public WorldFileBackgroundSection(IReadOnlyList<byte> styles)
  {
    ArgumentNullException.ThrowIfNull(styles);
    if (styles.Count != 5)
    {
      throw new ArgumentException(
        "The background section requires exactly five styles.",
        nameof(styles));
    }

    Styles = Array.AsReadOnly(new List<byte>(styles).ToArray());
  }

  public IReadOnlyList<byte> Styles { get; }

  public static WorldFileBackgroundSection Empty => new(new byte[5]);
}
