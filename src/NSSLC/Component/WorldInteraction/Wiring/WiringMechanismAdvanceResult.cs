using System;
using System.Collections.Generic;

namespace Terraria.WorldInteraction.Wiring;

public sealed class WiringMechanismAdvanceResult
{
  public WiringMechanismAdvanceResult(
    IReadOnlyList<MechanismCooldownEntry> readyEntries,
    int removedInvalidEntries,
    int cannonCooldownTicks,
    int bunnyCannonCooldownTicks,
    int snowballCannonCooldownTicks)
  {
    ArgumentNullException.ThrowIfNull(readyEntries);
    ReadyEntries = new List<MechanismCooldownEntry>(readyEntries).AsReadOnly();
    RemovedInvalidEntries = removedInvalidEntries;
    CannonCooldownTicks = cannonCooldownTicks;
    BunnyCannonCooldownTicks = bunnyCannonCooldownTicks;
    SnowballCannonCooldownTicks = snowballCannonCooldownTicks;
  }

  public IReadOnlyList<MechanismCooldownEntry> ReadyEntries { get; }

  public int RemovedInvalidEntries { get; }

  public int CannonCooldownTicks { get; }

  public int BunnyCannonCooldownTicks { get; }

  public int SnowballCannonCooldownTicks { get; }
}
