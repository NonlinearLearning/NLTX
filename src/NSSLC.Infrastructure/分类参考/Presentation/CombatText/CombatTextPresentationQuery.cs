namespace Terraria.Presentation.CombatText;

public static class CombatTextPresentationQuery
{
  public static bool ContainsText(CombatTextEntryComponent component, string text)
  {
    ArgumentNullException.ThrowIfNull(component);
    ArgumentNullException.ThrowIfNull(text);
    for (int slot = 0; slot < CombatTextPoolPolicy.Capacity; slot++)
    {
      CombatTextEntry entry = component.Entries[slot];
      if (entry.IsActive && entry.Text == text)
      {
        return true;
      }
    }

    return false;
  }

  public static bool HasActiveEntries(CombatTextEntryComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    for (int slot = 0; slot < CombatTextPoolPolicy.Capacity; slot++)
    {
      if (component.Entries[slot].IsActive)
      {
        return true;
      }
    }

    return false;
  }

  public static bool TryGet(
    CombatTextEntryComponent component,
    int slot,
    out CombatTextEntry entry)
  {
    ArgumentNullException.ThrowIfNull(component);
    if (slot < 0 || slot >= CombatTextPoolPolicy.Capacity)
    {
      entry = CombatTextEntry.Empty;
      return false;
    }

    entry = component.Entries[slot];
    return entry.IsActive;
  }
}
