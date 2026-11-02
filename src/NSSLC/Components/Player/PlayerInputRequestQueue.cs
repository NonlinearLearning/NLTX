using ArchWorld = Arch.Core.World;
using Terraria.Entity;

namespace Terraria.Player;

/// <summary>Locates the single entity that carries the player input request queue.</summary>
internal static class PlayerInputRequestQueue
{
  public static PlayerInputRequestQueueComponent Require(ArchWorld world)
    => SingletonComponent<PlayerInputRequestQueueComponent>.Require(
        world,
        "A PlayerInputRequestQueueComponent entity must exist before player input is recorded.");
}
