using ArchWorld = Arch.Core.World;
using Terraria.Entity;

namespace Terraria.Movement;

/// <summary>Locates the single entity that carries the movement intent request queue.</summary>
public static class MovementIntentRequestQueue
{
  public static MovementIntentRequestQueueComponent Require(ArchWorld world)
    => SingletonComponent<MovementIntentRequestQueueComponent>.Require(
        world,
        "A MovementIntentRequestQueueComponent entity must exist before movement intents are "
        + "recorded.");
}
