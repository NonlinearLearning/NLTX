using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Systems;

/// <summary>
/// Owns the mutable room and homelessness indexes for the current world scope.
/// The key remains NPC type until an integration decision authorizes another schema.
/// </summary>
public static class TownHousingRegistrySystem
{
  public static void UseNpcTypeKeys(TownHousingRegistryComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    if (component.ResidentKeyMode is not (
        TownHousingKeyMode.Unresolved or TownHousingKeyMode.Type))
    {
      throw new InvalidOperationException(
        "The housing registry cannot change key mode after a different mode is selected.");
    }

    component.SetResidentKeyMode(TownHousingKeyMode.Type);
  }

  public static bool TryGetRoom(
    TownHousingRegistryComponent component,
    TownHousingResidentKey resident,
    out TilePosition room)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.RoomsByResidentKey.TryGetValue(resident, out room);
  }

  public static bool HasRoom(
    TownHousingRegistryComponent component,
    TownHousingResidentKey resident)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.RoomsByResidentKey.ContainsKey(resident);
  }

  public static bool IsHomeless(
    TownHousingRegistryComponent component,
    TownHousingResidentKey resident)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.HomelessResidentKeys.Contains(resident);
  }

  public static void AssignRoom(
    TownHousingRegistryComponent component,
    TownHousingResidentKey resident,
    TilePosition room)
  {
    ArgumentNullException.ThrowIfNull(component);
    EnsureNpcTypeKeyMode(component);

    bool changed = !component.RoomsByResidentKey.TryGetValue(resident, out TilePosition oldRoom) ||
      oldRoom != room ||
      component.HomelessResidentKeys.Remove(resident);
    component.RoomsByResidentKey[resident] = room;
    if (changed)
    {
      component.AdvanceRevision();
    }
  }

  public static void MarkHomeless(
    TownHousingRegistryComponent component,
    TownHousingResidentKey resident)
  {
    ArgumentNullException.ThrowIfNull(component);
    EnsureNpcTypeKeyMode(component);

    bool changed = component.RoomsByResidentKey.Remove(resident);
    changed |= component.HomelessResidentKeys.Add(resident);
    if (changed)
    {
      component.AdvanceRevision();
    }
  }

  public static bool RemoveResident(
    TownHousingRegistryComponent component,
    TownHousingResidentKey resident)
  {
    ArgumentNullException.ThrowIfNull(component);
    bool changed = component.RoomsByResidentKey.Remove(resident);
    changed |= component.HomelessResidentKeys.Remove(resident);
    if (changed)
    {
      component.AdvanceRevision();
    }

    return changed;
  }

  public static void Clear(TownHousingRegistryComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    if (component.IsEmpty)
    {
      return;
    }

    component.RoomsByResidentKey.Clear();
    component.HomelessResidentKeys.Clear();
    component.AdvanceRevision();
  }

  private static void EnsureNpcTypeKeyMode(TownHousingRegistryComponent component)
  {
    if (component.ResidentKeyMode == TownHousingKeyMode.Unresolved)
    {
      UseNpcTypeKeys(component);
      return;
    }

    if (component.ResidentKeyMode != TownHousingKeyMode.Type)
    {
      throw new InvalidOperationException(
        "The housing registry key mode is incompatible with an NPC-type resident key.");
    }
  }
}
