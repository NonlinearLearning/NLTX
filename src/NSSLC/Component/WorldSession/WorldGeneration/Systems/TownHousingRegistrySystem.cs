using System;
using System.Collections.Generic;
using System.IO;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Systems;

/// <summary>
/// Owns the mutable room and homelessness indexes for the current world scope.
/// The key remains NPC type until an integration decision authorizes another mode.
/// </summary>
public static class TownHousingRegistrySystem
{
  public static void UseNpcTypeKeys(TownHousingRegistryComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    lock (component.SyncRoot)
    {
      EnsureNpcTypeKeyMode(component);
    }
  }

  public static bool TryGetRoom(
    TownHousingRegistryComponent component,
    TownHousingResidentKey resident,
    out TilePosition room)
  {
    ArgumentNullException.ThrowIfNull(component);
    lock (component.SyncRoot)
    {
      return component.RoomsByResidentKey.TryGetValue(resident, out room);
    }
  }

  public static bool HasRoom(
    TownHousingRegistryComponent component,
    TownHousingResidentKey resident)
  {
    ArgumentNullException.ThrowIfNull(component);
    lock (component.SyncRoot)
    {
      return component.RoomsByResidentKey.ContainsKey(resident);
    }
  }

  public static bool IsHomeless(
    TownHousingRegistryComponent component,
    TownHousingResidentKey resident)
  {
    ArgumentNullException.ThrowIfNull(component);
    lock (component.SyncRoot)
    {
      return component.HomelessResidentKeys.Contains(resident);
    }
  }

  public static IReadOnlyList<KeyValuePair<TownHousingResidentKey, TilePosition>>
    GetRoomAssignmentsSnapshot(TownHousingRegistryComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    lock (component.SyncRoot)
    {
      if (component.RoomAssignmentOrder.Count != component.RoomsByResidentKey.Count)
      {
        throw new InvalidOperationException("The housing room order index is inconsistent.");
      }

      var snapshot = new KeyValuePair<TownHousingResidentKey, TilePosition>[
        component.RoomAssignmentOrder.Count];
      for (int index = 0; index < component.RoomAssignmentOrder.Count; index++)
      {
        TownHousingResidentKey resident = component.RoomAssignmentOrder[index];
        if (!component.RoomsByResidentKey.TryGetValue(resident, out TilePosition room))
        {
          throw new InvalidOperationException("The housing room order index is inconsistent.");
        }

        snapshot[index] = new KeyValuePair<TownHousingResidentKey, TilePosition>(resident, room);
      }

      return Array.AsReadOnly(snapshot);
    }
  }

  public static IReadOnlyList<TownHousingResidentKey> GetOccupants(
    TownHousingRegistryComponent component,
    TilePosition room)
  {
    ArgumentNullException.ThrowIfNull(component);
    lock (component.SyncRoot)
    {
      var occupants = new List<TownHousingResidentKey>();
      foreach (TownHousingResidentKey resident in component.RoomAssignmentOrder)
      {
        if (component.RoomsByResidentKey.TryGetValue(resident, out TilePosition assignedRoom) &&
            assignedRoom == room)
        {
          occupants.Add(resident);
        }
      }

      return occupants.AsReadOnly();
    }
  }

  public static void AssignRoom(
    TownHousingRegistryComponent component,
    TownHousingResidentKey resident,
    TilePosition room)
  {
    ArgumentNullException.ThrowIfNull(component);
    lock (component.SyncRoot)
    {
      EnsureNpcTypeKeyMode(component);

      bool hadRoom = component.RoomsByResidentKey.TryGetValue(resident, out TilePosition oldRoom);
      bool movedToEnd = hadRoom &&
        component.RoomAssignmentOrder.Count > 0 &&
        component.RoomAssignmentOrder[^1] != resident;
      bool changed = !hadRoom || oldRoom != room ||
        component.HomelessResidentKeys.Contains(resident) || movedToEnd;
      if (changed)
      {
        component.EnsureRevisionCanAdvance();
      }

      component.HomelessResidentKeys.Remove(resident);
      if (hadRoom)
      {
        component.RoomAssignmentOrder.Remove(resident);
      }

      component.RoomAssignmentOrder.Add(resident);
      component.RoomsByResidentKey[resident] = room;
      if (changed)
      {
        component.AdvanceRevision();
      }
    }
  }

  public static void MarkHomeless(
    TownHousingRegistryComponent component,
    TownHousingResidentKey resident)
  {
    ArgumentNullException.ThrowIfNull(component);
    lock (component.SyncRoot)
    {
      EnsureNpcTypeKeyMode(component);

      bool hadRoom = component.RoomsByResidentKey.ContainsKey(resident);
      bool wasHomeless = component.HomelessResidentKeys.Contains(resident);
      bool changed = hadRoom || !wasHomeless;
      if (changed)
      {
        component.EnsureRevisionCanAdvance();
      }

      if (component.RoomsByResidentKey.Remove(resident))
      {
        component.RoomAssignmentOrder.Remove(resident);
      }

      component.HomelessResidentKeys.Add(resident);
      if (changed)
      {
        component.AdvanceRevision();
      }
    }
  }

  public static bool RemoveResident(
    TownHousingRegistryComponent component,
    TownHousingResidentKey resident)
  {
    ArgumentNullException.ThrowIfNull(component);
    lock (component.SyncRoot)
    {
      bool hadRoom = component.RoomsByResidentKey.ContainsKey(resident);
      bool wasHomeless = component.HomelessResidentKeys.Contains(resident);
      bool changed = hadRoom || wasHomeless;
      if (changed)
      {
        component.EnsureRevisionCanAdvance();
      }

      if (component.RoomsByResidentKey.Remove(resident))
      {
        component.RoomAssignmentOrder.Remove(resident);
      }

      component.HomelessResidentKeys.Remove(resident);
      if (changed)
      {
        component.AdvanceRevision();
      }

      return changed;
    }
  }

  public static void ReplaceRoomAssignments(
    TownHousingRegistryComponent component,
    IReadOnlyList<KeyValuePair<TownHousingResidentKey, TilePosition>> assignments)
  {
    ArgumentNullException.ThrowIfNull(component);
    ArgumentNullException.ThrowIfNull(assignments);

    var stagedRooms = new Dictionary<TownHousingResidentKey, TilePosition>(assignments.Count);
    var stagedOrder = new List<TownHousingResidentKey>(assignments.Count);
    for (int index = 0; index < assignments.Count; index++)
    {
      KeyValuePair<TownHousingResidentKey, TilePosition> assignment = assignments[index];
      if (!stagedRooms.TryAdd(assignment.Key, assignment.Value))
      {
        throw new InvalidDataException("Duplicate town housing resident keys.");
      }

      stagedOrder.Add(assignment.Key);
    }

    lock (component.SyncRoot)
    {
      EnsureNpcTypeKeyMode(component);
      bool changed = component.HomelessResidentKeys.Count != 0 ||
        component.RoomAssignmentOrder.Count != stagedOrder.Count ||
        component.RoomsByResidentKey.Count != stagedRooms.Count;

      for (int index = 0; !changed && index < stagedOrder.Count; index++)
      {
        TownHousingResidentKey resident = stagedOrder[index];
        changed = component.RoomAssignmentOrder[index] != resident ||
          !component.RoomsByResidentKey.TryGetValue(resident, out TilePosition currentRoom) ||
          currentRoom != stagedRooms[resident];
      }

      if (!changed)
      {
        return;
      }

      component.EnsureRevisionCanAdvance();
      component.RoomsByResidentKey.Clear();
      foreach (KeyValuePair<TownHousingResidentKey, TilePosition> assignment in stagedRooms)
      {
        component.RoomsByResidentKey.Add(assignment.Key, assignment.Value);
      }

      component.RoomAssignmentOrder.Clear();
      component.RoomAssignmentOrder.AddRange(stagedOrder);
      component.HomelessResidentKeys.Clear();
      component.AdvanceRevision();
    }
  }

  public static void Clear(TownHousingRegistryComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    lock (component.SyncRoot)
    {
      if (component.RoomsByResidentKey.Count == 0 &&
          component.HomelessResidentKeys.Count == 0 &&
          component.RoomAssignmentOrder.Count == 0)
      {
        return;
      }

      component.EnsureRevisionCanAdvance();
      component.RoomsByResidentKey.Clear();
      component.HomelessResidentKeys.Clear();
      component.RoomAssignmentOrder.Clear();
      component.AdvanceRevision();
    }
  }

  private static void EnsureNpcTypeKeyMode(TownHousingRegistryComponent component)
  {
    if (component.ResidentKeyMode == TownHousingKeyMode.Unresolved)
    {
      component.SetResidentKeyMode(TownHousingKeyMode.Type);
      return;
    }

    if (component.ResidentKeyMode != TownHousingKeyMode.Type)
    {
      throw new InvalidOperationException(
        "The housing registry key mode is incompatible with an NPC-type resident key.");
    }
  }
}
