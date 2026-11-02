using ArchWorld = Arch.Core.World;
using Terraria.Entity;

namespace Terraria.Combat;

/// <summary>Locates the single entity that carries the life-restore request queue.</summary>
internal static class LifeRestoreRequestQueue
{
  public static LifeRestoreRequestQueueComponent Require(ArchWorld world)
    => SingletonComponent<LifeRestoreRequestQueueComponent>.Require(
        world,
        "A LifeRestoreRequestQueueComponent entity must exist before life restores are requested.");
}
