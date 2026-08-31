using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldModel.Systems;

public sealed class WorldMeteorImpactSystem
{
  public const ushort MeteoriteTileType = 37;

  private const int ImpactRadius = 35;
  private const int OuterRadius = 18;
  private const int InnerRadius = 12;
  private const int MaximumMeteoritesAtReferenceWidth = 400;
  private static readonly IReadOnlySet<ushort> ProtectedTileTypes = new HashSet<ushort>
  {
    26, 226, 470, 475, 488, 597
  }.ToFrozenSet();

  public static IReadOnlySet<ushort> RegisterProtectedTileDefaults()
  {
    return ProtectedTileTypes;
  }

  public bool TryCreateCommands(
    WorldGrid world,
    WorldMeteorImpactCommand impact,
    IReadOnlyCollection<WorldMeteorOccupant> occupants,
    long firstSequence,
    out IReadOnlyList<TileChangeCommand> commands,
    out string? rejectionReason)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(occupants);
    commands = [];
    rejectionReason = null;
    if (!impact.IsValid || firstSequence < 0)
    {
      rejectionReason = "Meteor impact command was invalid.";
      return false;
    }

    if (impact.X < ImpactRadius + 1 || impact.X >= world.Width - ImpactRadius - 1 ||
        impact.Y < ImpactRadius + 1 || impact.Y >= world.Height - ImpactRadius - 1)
    {
      rejectionReason = "Meteor impact was outside the safe world bounds.";
      return false;
    }

    foreach (WorldMeteorOccupant occupant in occupants)
    {
      if (occupant.Intersects(impact.X, impact.Y, ImpactRadius))
      {
        rejectionReason = "Meteor impact intersected an active entity safety area.";
        return false;
      }
    }

    int maximumMeteorites = Math.Max(
      1,
      (int)(MaximumMeteoritesAtReferenceWidth * (double)world.Width / 4200.0));
    if (CountMeteorites(world) > maximumMeteorites)
    {
      rejectionReason = "The world meteorite cap was reached.";
      return false;
    }

    for (int y = impact.Y - ImpactRadius; y <= impact.Y + ImpactRadius; y++)
    {
      for (int x = impact.X - ImpactRadius; x <= impact.X + ImpactRadius; x++)
      {
        if (IsProtectedTile(world.GetTile(x, y)))
        {
          rejectionReason = "Meteor impact intersected a protected tile.";
          return false;
        }
      }
    }

    List<TileChangeCommand> changes = [];
    long sequence = firstSequence;
    for (int y = impact.Y - OuterRadius; y <= impact.Y + OuterRadius; y++)
    {
      for (int x = impact.X - OuterRadius; x <= impact.X + OuterRadius; x++)
      {
        int deltaX = x - impact.X;
        int deltaY = y - impact.Y;
        int distanceSquared = deltaX * deltaX + deltaY * deltaY;
        if (distanceSquared <= InnerRadius * InnerRadius && deltaY <= 6)
        {
          changes.Add(new TileChangeCommand(
            sequence++,
            x,
            y,
            TileChangeKind.Kill,
            TileType: 0));
        }
        else if (distanceSquared <= OuterRadius * OuterRadius && deltaY >= -4)
        {
          changes.Add(new TileChangeCommand(
            sequence++,
            x,
            y,
            TileChangeKind.Place,
            MeteoriteTileType));
        }
      }
    }

    if (changes.Count == 0)
    {
      rejectionReason = "Meteor impact did not produce any tile changes.";
      return false;
    }

    commands = changes;
    return true;
  }

  private static int CountMeteorites(WorldGrid world)
  {
    int count = 0;
    for (int y = 0; y < world.Height; y++)
    {
      for (int x = 0; x < world.Width; x++)
      {
        WorldTile tile = world.GetTile(x, y);
        if (tile.IsActive && tile.Type == MeteoriteTileType)
        {
          count++;
        }
      }
    }

    return count;
  }

  private static bool IsProtectedTile(WorldTile tile)
  {
    if (!tile.IsActive)
    {
      return false;
    }

    return ProtectedTileTypes.Contains(tile.Type);
  }
}
