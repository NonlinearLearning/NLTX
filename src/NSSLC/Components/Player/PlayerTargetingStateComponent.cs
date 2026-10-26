using System;
using Terraria.Relationships;

namespace Terraria.Player;

/// <summary>Stores a server-selected target relation without any client selection state.</summary>
public sealed class PlayerTargetingStateComponent
{
  public EntityUuid TargetId { get; private set; }

  public long Revision { get; private set; }

  public bool HasTarget => TargetId.IsValid;

  public void SetTarget(EntityUuid targetId, long revision)
  {
    if (!targetId.IsValid)
    {
      throw new ArgumentException(
          "A target relation requires a valid entity identity.", nameof(targetId));
    }

    if (revision < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(revision));
    }

    TargetId = targetId;
    Revision = revision;
  }

  public bool Clear()
  {
    if (!HasTarget)
    {
      return false;
    }

    TargetId = default;
    Revision++;
    return true;
  }
}
