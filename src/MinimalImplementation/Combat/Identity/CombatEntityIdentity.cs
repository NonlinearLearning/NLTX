using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Identity;

/// <summary>A value projection of the server's identity handle, never an identity authority.</summary>
public readonly record struct CombatEntityIdentity(
  EntityUuid Id,
  long WorldScope,
  long Generation)//WorldScope 用来拒绝旧世界留下的操作。Generation 通常用于解决编号复用。这两个都是重复的
{
  public bool IsValid => Id.IsValid && WorldScope > 0 && Generation > 0;
}
