using System;
using System.Collections.Generic;

namespace Terraria.WorldSession.Calendar;

public sealed class BirthdayPartyStateComponent
{
  private readonly IReadOnlyList<NpcEntityId> _celebratingNpcIds;

  public BirthdayPartyStateComponent(
    bool manualParty = false,
    bool genuineParty = false,
    int partyDaysOnCooldown = 0,
    IReadOnlyList<NpcEntityId>? celebratingNpcIds = null,
    bool wasCelebrating = false)
  {
    ManualParty = manualParty;
    GenuineParty = genuineParty;
    PartyDaysOnCooldown = partyDaysOnCooldown;
    _celebratingNpcIds = new List<NpcEntityId>(
      celebratingNpcIds ?? Array.Empty<NpcEntityId>()).AsReadOnly();
    WasCelebrating = wasCelebrating;
    Validate();
  }

  public bool ManualParty;
  public bool GenuineParty;
  public int PartyDaysOnCooldown;
  public IReadOnlyList<NpcEntityId> CelebratingNpcIds => _celebratingNpcIds;
  public bool WasCelebrating;

  public bool IsUp => ManualParty || GenuineParty;

  public void Validate()
  {
    ArgumentOutOfRangeException.ThrowIfNegative(PartyDaysOnCooldown);

    HashSet<NpcEntityId> ids = new();
    for (int index = 0; index < CelebratingNpcIds.Count; index++)
    {
      if (!ids.Add(CelebratingNpcIds[index]))
      {
        throw new ArgumentException(
          "A birthday party cannot contain a duplicate NPC reference.",
          nameof(CelebratingNpcIds));
      }
    }
  }
}
