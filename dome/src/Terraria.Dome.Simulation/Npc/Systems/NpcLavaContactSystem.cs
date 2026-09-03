using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Combat.Commands;
using Terraria.Dome.Simulation.Combat.Components;
using Terraria.Dome.Simulation.Components;
using Terraria.Dome.Simulation.Liquid.Components;
using Terraria.Dome.Simulation.Npc.Components;
using Terraria.Dome.Simulation.WorldModel;

using EntityEcs.Components;
namespace Terraria.Dome.Simulation.Npc.Systems;

public readonly record struct NpcLavaContactCandidate(
  NpcHandle Npc,
  SimulationVector Position,
  ColliderComponent Collider,
  NpcCombatStateComponent Combat,
  bool IsActive,
  bool DoesNotTakeDamage,
  int Health,
  ImmunityComponent Immunity);

public sealed class NpcLavaContactSystem
{
  private const float TileBoundaryEpsilon = 0.0001f;

  public IReadOnlyList<DamageNpcCommand> ProduceDamageCommands(
    WorldGrid world,
    IReadOnlyList<NpcLavaContactCandidate> candidates,
    int damage)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(candidates);
    if (damage <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(damage));
    }

    List<DamageNpcCommand> commands = new();
    for (int index = 0; index < candidates.Count; index++)
    {
      NpcLavaContactCandidate candidate = candidates[index];
      if (!candidate.IsActive || !candidate.Npc.IsValid || candidate.Health <= 0 ||
          candidate.Combat.LavaImmune || candidate.Combat.DoesNotTakeDamage ||
          candidate.Immunity.IsImmune ||
          !HasLavaContact(world, candidate.Position, candidate.Collider))
      {
        continue;
      }

      commands.Add(new DamageNpcCommand(
        candidate.Npc,
        damage,
        SourceKind: NpcDamageSourceKind.Lava));
    }

    return commands;
  }

  private static bool HasLavaContact(
    WorldGrid world,
    SimulationVector position,
    ColliderComponent collider)
  {
    if (!IsValidGeometry(position, collider))
    {
      return false;
    }

    if (position.X >= world.Width || position.Y >= world.Height ||
        position.X + collider.Width <= 0.0f || position.Y + collider.Height <= 0.0f)
    {
      return false;
    }

    int firstX = Math.Max(0, (int)MathF.Floor(position.X));
    int firstY = Math.Max(0, (int)MathF.Floor(position.Y));
    int lastX = Math.Min(
      world.Width - 1,
      (int)MathF.Floor(position.X + collider.Width - TileBoundaryEpsilon));
    int lastY = Math.Min(
      world.Height - 1,
      (int)MathF.Floor(position.Y + collider.Height - TileBoundaryEpsilon));
    if (firstX > lastX || firstY > lastY)
    {
      return false;
    }

    for (int y = firstY; y <= lastY; y++)
    {
      for (int x = firstX; x <= lastX; x++)
      {
        WorldTile tile = world.GetTile(x, y);
        if (tile.LiquidAmount > 0 && tile.LiquidType == (byte)LiquidType.Lava)
        {
          return true;
        }
      }
    }

    return false;
  }

  private static bool IsValidGeometry(
    SimulationVector position,
    ColliderComponent collider)
  {
    return float.IsFinite(position.X) && float.IsFinite(position.Y) &&
      float.IsFinite(collider.Width) && collider.Width > 0.0f &&
      float.IsFinite(collider.Height) && collider.Height > 0.0f &&
      float.IsFinite(position.X + collider.Width) &&
      float.IsFinite(position.Y + collider.Height);
  }
}
