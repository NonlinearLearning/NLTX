using System;
using System.Collections.Generic;

namespace Terraria.NonAuthoritative.Persistence;

/// <summary>
/// Persisted party state and the NPC slots celebrating during a world load.
/// </summary>
public sealed class WorldFilePartySection
{
  public const string SectionId = "world.party";

  public WorldFilePartySection(
    bool manual,
    bool genuine,
    int cooldown,
    IReadOnlyList<int> celebratingNpcIds)
  {
    CelebratingNpcIds = Copy(celebratingNpcIds);
    Manual = manual;
    Genuine = genuine;
    Cooldown = cooldown;
  }

  public bool Manual { get; }

  public bool Genuine { get; }

  public int Cooldown { get; }

  public IReadOnlyList<int> CelebratingNpcIds { get; }

  public static WorldFilePartySection Empty => new(
    manual: false,
    genuine: false,
    cooldown: 0,
    celebratingNpcIds: Array.Empty<int>());

  private static IReadOnlyList<int> Copy(IReadOnlyList<int> values)
  {
    ArgumentNullException.ThrowIfNull(values);
    return Array.AsReadOnly(new List<int>(values).ToArray());
  }
}
