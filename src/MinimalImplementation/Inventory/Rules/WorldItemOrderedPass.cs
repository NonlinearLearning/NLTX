using System.Numerics;
using MinimalImplementation.Server.Inventory.Content;
using MinimalImplementation.Server.Inventory.State;

namespace MinimalImplementation.Server.Inventory.Rules;

/// <summary>Detached, ascending-slot working state for one authoritative item update pass.</summary>
internal sealed class WorldItemOrderedPass
{
  private const int ItemSize = 16;
  private readonly WorldItemSnapshot?[] _items;
  private readonly HashSet<WorldItemHandle> _reserved;

  public WorldItemOrderedPass(
    IReadOnlyList<WorldItemSnapshot> items,
    IReadOnlyList<WorldItemReservation> reservations)
  {
    _items = items.Cast<WorldItemSnapshot?>().ToArray();
    _reserved = reservations.Select(value => value.Handle).ToHashSet();
  }

  public WorldItemSnapshot? Get(int index) => _items[index];

  public bool IsReserved(WorldItemHandle handle) => _reserved.Contains(handle);

  public void Set(int index, WorldItemMutation? mutation)
  {
    WorldItemSnapshot? item = _items[index];
    if (item is null)
    {
      throw new InvalidOperationException("A retired working item cannot be updated.");
    }

    _items[index] = mutation is null ? null : item with {
      Item = mutation.Item,
      Position = mutation.Position,
      Velocity = mutation.Velocity,
      TimeLeft = mutation.TimeLeft,
      IsShimmering = mutation.IsShimmering,
      EnemyPickupDelay = mutation.EnemyPickupDelay,
      ShimmerTime = mutation.ShimmerTime,
      Runtime = mutation.Runtime
    };
  }

  public bool TryCombine(
    int index, WorldItemSnapshot item, out WorldItemSnapshot combined)
  {
    combined = item;
    // Dedicated server Main.myPlayer is 255. A stored player claim excludes server merging
    // until the Reservation owner explicitly releases it, including expired stored claims.
    if (_reserved.Contains(item.Handle)
      || !ItemContentFacts.TryGetMaximumStack(item.Item.Type, out int maximumStack)
      || item.Item.Stack >= maximumStack)
    {
      return true;
    }

    bool known = ItemContentFacts.TryGetPassiveStackEligibility(item.Item.Type, out bool eligible);
    if (known && !eligible)
    {
      return true;
    }

    for (int otherIndex = index + 1; otherIndex < _items.Length; otherIndex++)
    {
      WorldItemSnapshot? other = _items[otherIndex];
      if (other is null || other.Handle.SlotId >= 400
        || _reserved.Contains(other.Handle)
        || other.Item.Type != combined.Item.Type || other.Item.Prefix != combined.Item.Prefix
        || other.IsShimmering != combined.IsShimmering
        || Math.Abs(combined.Position.X - other.Position.X)
          + Math.Abs(combined.Position.Y - other.Position.Y) > 30f)
      {
        continue;
      }

      if (!known)
      {
        return false;
      }

      int moved = Math.Min(other.Item.Stack, maximumStack - combined.Item.Stack);
      int stack = combined.Item.Stack + moved;
      float amount = (float)moved / stack;
      // Source keeps scanning after filling: even a zero transfer uses this Lerp order.
      combined = combined with {
        Item = combined.Item.WithStack(stack),
        Position = Vector2.Lerp(other.Position, combined.Position, amount),
        Velocity = Vector2.Lerp(other.Velocity, combined.Velocity, amount)
      };
      _items[otherIndex] = other.Item.Stack == moved
        ? null : other with { Item = other.Item.WithStack(other.Item.Stack - moved) };
    }

    return true;
  }

  public bool ApplyShimmerRepulsion(int index, Vector2 position, ref Vector2 velocity)
  {
    WorldItemSnapshot current = _items[index]!;
    bool repelled = false;
    int remaining = 50;
    for (int otherIndex = 0; otherIndex < _items.Length; otherIndex++)
    {
      WorldItemSnapshot? other = _items[otherIndex];
      if (otherIndex == index || other is null || !other.IsShimmering
        || other.Handle.SlotId >= 400)
      {
        continue;
      }

      if (remaining-- <= 0)
      {
        break;
      }

      // All world-item hitboxes are 16x16, so center deltas equal position deltas.
      if (Math.Abs(position.X - other.Position.X) > ItemSize
        || Math.Abs(position.Y - other.Position.Y) > ItemSize)
      {
        continue;
      }

      repelled = true;
      float scale = Math.Min(ItemSize / Vector2.Distance(position, other.Position), 10f);
      Vector2 otherVelocity = other.Velocity;
      if (position.X < other.Position.X)
      {
        if (velocity.X > -3f * scale)
        {
          velocity.X -= 0.1f * scale;
        }
        if (otherVelocity.X < 3f)
        {
          otherVelocity.X += 0.1f * scale;
        }
      }
      else if (position.X > other.Position.X)
      {
        if (velocity.X < 3f * scale)
        {
          velocity.X += 0.1f * scale;
        }
        if (otherVelocity.X > -3f)
        {
          otherVelocity.X -= 0.1f * scale;
        }
      }
      else if (current.Handle.SlotId < other.Handle.SlotId)
      {
        if (velocity.X > -3f * scale)
        {
          velocity.X -= 0.1f * scale;
        }
        if (otherVelocity.X < 3f * scale)
        {
          otherVelocity.X += 0.1f * scale;
        }
      }

      _items[otherIndex] = other with { Velocity = otherVelocity };
    }

    return repelled;
  }
}
