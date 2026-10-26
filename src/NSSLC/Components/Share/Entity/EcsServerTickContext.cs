using Arch.Core;
using ArchWorld = Arch.Core.World;

namespace Terraria.Entity;

/// <summary>
/// Immutable input shared by all systems in one NSSLC ECS update.
/// </summary>
public readonly record struct EcsServerTickContext(
    ArchWorld World,
    long Tick,
    float DeltaTime)
{
  public bool IsValid => World is not null && Tick >= 0 &&
      float.IsFinite(DeltaTime) && DeltaTime > 0f;
}
