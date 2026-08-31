using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.Npc.Systems;

public readonly record struct NpcSlotAccount(bool IsActive, float NpcSlotCost);

public sealed class NpcSlotAccountingSystem
{
  public float CalculateActiveSlots(IReadOnlyList<NpcSlotAccount> accounts)
  {
    ArgumentNullException.ThrowIfNull(accounts);
    double total = 0.0;
    for (int index = 0; index < accounts.Count; index++)
    {
      NpcSlotAccount account = accounts[index];
      if (!float.IsFinite(account.NpcSlotCost) || account.NpcSlotCost < 0.0f)
      {
        throw new ArgumentOutOfRangeException(nameof(accounts));
      }

      if (account.IsActive)
      {
        total += account.NpcSlotCost;
      }
    }

    if (!double.IsFinite(total) || total > float.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(accounts));
    }

    return (float)total;
  }
}
