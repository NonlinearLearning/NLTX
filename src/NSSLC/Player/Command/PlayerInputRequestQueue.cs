using ArchWorld = Arch.Core.World;
using Terraria.Share.Entities;

namespace Terraria.Unshared.Player;

/// <summary>Locates the single entity that carries the player input request queue.</summary>
public static class PlayerInputRequestQueue
{
  public static PlayerInputRequestQueueComponent Require(ArchWorld world)
    => SingletonComponent<PlayerInputRequestQueueComponent>.Require(
        world,
        "A PlayerInputRequestQueueComponent entity must exist before player input is recorded.");
}
