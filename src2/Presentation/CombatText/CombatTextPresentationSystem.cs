using System.Numerics;

namespace Terraria.Presentation.CombatText;

public sealed class CombatTextPresentationSystem
{
  private readonly CombatTextPalette _palette;

  public CombatTextPresentationSystem()
    : this(CombatTextPalette.Default)
  {
  }

  public CombatTextPresentationSystem(CombatTextPalette palette)
  {
    _palette = palette ?? throw new ArgumentNullException(nameof(palette));
  }

  public int Advance(CombatTextEntryComponent component, int ticks)
  {
    ArgumentNullException.ThrowIfNull(component);
    if (ticks < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(ticks));
    }

    int clearedCount = 0;
    for (int tick = 0; tick < ticks; tick++)
    {
      for (int slot = 0; slot < CombatTextPoolPolicy.Capacity; slot++)
      {
        CombatTextEntry entry = component.ReadEntry(slot);
        if (!entry.IsActive)
        {
          continue;
        }
        if (entry.RemainingTicks <= 1)
        {
          component.ClearSlot(slot);
          clearedCount++;
          continue;
        }

        component.WriteEntry(slot, entry with
        {
          Position = entry.Position + entry.Velocity,
          RemainingTicks = entry.RemainingTicks - 1
        });
      }
    }

    return clearedCount;
  }

  public bool ClearAll(CombatTextEntryComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    bool hadEntries = CombatTextPresentationQuery.HasActiveEntries(component);
    for (int slot = 0; slot < CombatTextPoolPolicy.Capacity; slot++)
    {
      component.ClearSlot(slot);
    }

    return hadEntries;
  }

  public bool TrySpawn(
    CombatTextEntryComponent component,
    CombatTextSpawnCommand command,
    out int slot)
  {
    ArgumentNullException.ThrowIfNull(component);
    slot = FindFreeSlot(component);
    if (slot < 0)
    {
      slot = FindOldestSlot(component);
    }

    CombatTextEntry entry = new(
      command.Position,
      command.Velocity,
      1f,
      1,
      command.Text,
      command.Scale,
      command.Rotation,
      _palette.Get(command.ColorRole),
      true,
      command.LifetimeTicks,
      command.IsCritical,
      command.IsDamageOverTime);
    component.WriteEntry(slot, entry);
    return true;
  }

  private static int FindFreeSlot(CombatTextEntryComponent component)
  {
    for (int slot = 0; slot < CombatTextPoolPolicy.Capacity; slot++)
    {
      if (!component.ReadEntry(slot).IsActive)
      {
        return slot;
      }
    }

    return -1;
  }

  private static int FindOldestSlot(CombatTextEntryComponent component)
  {
    int selectedSlot = 0;
    int lowestRemainingTicks = int.MaxValue;
    for (int slot = 0; slot < CombatTextPoolPolicy.Capacity; slot++)
    {
      CombatTextEntry entry = component.ReadEntry(slot);
      if (entry.RemainingTicks < lowestRemainingTicks)
      {
        selectedSlot = slot;
        lowestRemainingTicks = entry.RemainingTicks;
      }
    }

    return selectedSlot;
  }
}
