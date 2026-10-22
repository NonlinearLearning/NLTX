using System.Numerics;

namespace MinimalImplementation.Combat.Npcs;

internal static class NpcTargetingRules {
  internal static (ushort Target, bool Right, bool Down) Select(
      NpcView npc, IReadOnlyList<NpcTargetPlayer> players, bool confused,
      ushort target, bool right, bool down, Vector2? npcPosition = null) {
    Vector2 origin = npcPosition ?? npc.Position;
    float distance = 0f;
    bool found = false;
    NpcTankTarget? tank = null;
    foreach (NpcTargetPlayer player in players.OrderBy(player => player.Slot)) {
      if (player.Slot == 255 || !player.Active || player.Dead || player.Ghost) {
        continue;
      }
      float candidate = Distance(npc, origin, player.Position, player.Width, player.Height) -
          player.Aggro;
      if (player.NoAggro) {
        candidate += 1000f;
      }
      if (!found || candidate < distance) {
        found = true;
        target = player.Slot;
        tank = null;
        distance = candidate;
      }
      if (player.TankPet is { } pet && !player.NoAggro) {
        float petDistance = Distance(npc, origin, pet.Position, pet.Width, pet.Height) - 200f;
        if (petDistance < distance && petDistance < 200f && pet.CanHit) {
          tank = pet;
        }
      }
    }

    if (tank is not null) {
      (right, down) = Face(npc, origin, tank.Position, tank.Width, tank.Height);
    } else {
      if (target >= 255) {
        target = 0;
      }
      NpcTargetPlayer? player = players.FirstOrDefault(player => player.Slot == target);
      if (player is not null && !player.Dead && !player.NoAggro &&
          !(!player.IsAnimatingItem && player.Aggro < 0 && npc.Target < 255 && !npc.IsBoss)) {
        (right, down) = Face(npc, origin, player.Position, player.Width, player.Height);
      }
    }
    if (confused) {
      right = !right;
    }
    return (target, right, down);
  }

  private static float Distance(
      NpcView npc, Vector2 origin, Vector2 position, int width, int height) {
    // Preserve TryTrackingTarget's literal plus-half-NPC expression.
    return Math.Abs(position.X + width / 2 - origin.X + npc.Width / 2) +
        Math.Abs(position.Y + height / 2 - origin.Y + npc.Height / 2);
  }

  private static (bool Right, bool Down) Face(
      NpcView npc, Vector2 origin, Vector2 position, int width, int height) {
    return ((int)position.X + width / 2 >= origin.X + npc.Width / 2,
        (int)position.Y + height / 2 >= origin.Y + npc.Height / 2);
  }
}
