namespace Terraria.Tiles.Interaction;

public sealed class TileHitTrackingSystem
{
  private readonly TileCrackPresentationAdapter _crackPresentation;

  public TileHitTrackingSystem(TileCrackPresentationAdapter crackPresentation)
  {
    _crackPresentation = crackPresentation ??
      throw new ArgumentNullException(nameof(crackPresentation));
  }

  public int Advance(TileHitTrackingComponent component, int ticks)
  {
    ArgumentNullException.ThrowIfNull(component);
    if (ticks < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(ticks));
    }

    int clearedCount = 0;
    for (int tick = 0; tick < ticks; tick++)
    {
      for (int slot = 0; slot < TileHitTrackingPolicy.Capacity; slot++)
      {
        TileHitEntry entry = component.ReadEntry(slot);
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
          RemainingTicks = entry.RemainingTicks - 1,
          AnimationTicks = Math.Max(0, entry.AnimationTicks - 1)
        });
      }
    }

    return clearedCount;
  }

  public bool Apply(TileHitTrackingComponent component, TileHitCommand command)
  {
    ArgumentNullException.ThrowIfNull(component);

    return command.Action switch
    {
      TileHitCommand.ActionKind.RegisterHit => Register(component, command),
      TileHitCommand.ActionKind.AddDamage => AddDamage(component, command),
      TileHitCommand.ActionKind.UpdatePosition => UpdatePosition(component, command),
      TileHitCommand.ActionKind.Clear => ClearAll(component),
      TileHitCommand.ActionKind.ClearAtLocation => ClearAtLocation(component, command),
      _ => false
    };
  }

  private static bool AddDamage(TileHitTrackingComponent component, TileHitCommand command)
  {
    if (!TileHitLookupQuery.TryFind(
      component,
      command.X,
      command.Y,
      command.HitKind,
      out int slot,
      out TileHitEntry entry))
    {
      return false;
    }

    component.WriteEntry(slot, entry with { Damage = entry.Damage + command.Damage });
    component.Touch(slot);
    return true;
  }

  private static bool ClearAll(TileHitTrackingComponent component)
  {
    bool hadActiveEntries = false;
    for (int slot = 0; slot < TileHitTrackingPolicy.Capacity; slot++)
    {
      if (component.ReadEntry(slot).IsActive)
      {
        hadActiveEntries = true;
      }
    }

    component.Reset();
    return hadActiveEntries;
  }

  private static bool ClearAtLocation(
    TileHitTrackingComponent component,
    TileHitCommand command)
  {
    bool cleared = false;
    for (int slot = 0; slot < TileHitTrackingPolicy.Capacity; slot++)
    {
      TileHitEntry entry = component.ReadEntry(slot);
      if (entry.IsActive && entry.X == command.X && entry.Y == command.Y &&
        (command.HitKind == TileHitKind.Unused || entry.Kind == command.HitKind))
      {
        component.ClearSlot(slot);
        cleared = true;
      }
    }

    return cleared;
  }

  private bool Register(TileHitTrackingComponent component, TileHitCommand command)
  {
    if (command.HitKind == TileHitKind.Unused)
    {
      return false;
    }

    if (TileHitLookupQuery.TryFind(
      component,
      command.X,
      command.Y,
      command.HitKind,
      out int existingSlot,
      out TileHitEntry existing))
    {
      component.WriteEntry(existingSlot, existing with
      {
        Damage = existing.Damage + command.Damage,
        RemainingTicks = TileHitTrackingPolicy.DefaultLifetimeTicks
      });
      component.Touch(existingSlot);
      return true;
    }

    int slot = FindFreeSlot(component);
    if (slot < 0)
    {
      slot = component.ReadOldestSlot();
    }

    component.WriteEntry(slot, new TileHitEntry(
      command.X,
      command.Y,
      command.Damage,
      command.HitKind,
      TileHitTrackingPolicy.DefaultLifetimeTicks,
      _crackPresentation.SelectCrackStyle(),
      0,
      System.Numerics.Vector2.Zero));
    component.Touch(slot);
    return true;
  }

  private static int FindFreeSlot(TileHitTrackingComponent component)
  {
    for (int slot = 0; slot < TileHitTrackingPolicy.Capacity; slot++)
    {
      if (!component.ReadEntry(slot).IsActive)
      {
        return slot;
      }
    }

    return -1;
  }

  private static bool UpdatePosition(TileHitTrackingComponent component, TileHitCommand command)
  {
    if (!TileHitLookupQuery.TryFind(
      component,
      command.X,
      command.Y,
      command.HitKind,
      out int slot,
      out TileHitEntry entry))
    {
      return false;
    }

    component.WriteEntry(slot, entry with { X = command.NewX, Y = command.NewY });
    component.Touch(slot);
    return true;
  }
}
