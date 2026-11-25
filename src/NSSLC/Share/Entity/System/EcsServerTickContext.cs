using Arch.Core;
using ArchWorld = Arch.Core.World;

namespace Terraria.Share.Entities;

/// <summary>
/// Immutable input shared by all systems in one NSSLC ECS update.
/// </summary>
public readonly record struct EcsServerTickContext(
    ArchWorld World,
    long Tick,
    float DeltaTime,
    EcsServerCommandBuffer? Commands = null)
{
  public bool IsValid => World is not null && Tick >= 0 &&
      float.IsFinite(DeltaTime) && DeltaTime > 0f;

  public bool HasCommands => Commands is not null;

  public EcsServerCommandBuffer CommandBuffer => Commands ??
      throw new InvalidOperationException(
          "This tick context is not connected to an ECS command buffer.");
}
