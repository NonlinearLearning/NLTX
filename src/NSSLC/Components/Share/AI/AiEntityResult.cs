using System;

namespace Terraria.Ai;

/// <summary>
/// Associates an AI result with the host's entity key without exposing an ECS type.
/// </summary>
public readonly record struct AiEntityResult<TEntity, TIntent>(
    TEntity Entity, AiResult<TIntent> Result)
    where TEntity : notnull
    where TIntent : unmanaged {
  public AiStatus Status => Result.Status;
}
