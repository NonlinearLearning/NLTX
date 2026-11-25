using System;
using System.Numerics;
using Terraria.Share.Combat;
using Terraria.Share.Relationships;

namespace Terraria.Unshared.Projectiles;

/// <summary>Validated input for creating one projectile instance.</summary>
/// <remarks>
/// 请求只携带<b>实例级</b>输入：谁发的、用哪个定义、在哪、朝哪、属于哪个阵营。
/// 碰撞体与尺寸不在这里——它们是实体定义的一部分（<see cref="ProjectileBasicsSection" />
/// 的 <c>Contribute</c> 直接装配），物理系统只读不写，生成方不该提供几何。
/// </remarks>
public readonly record struct ProjectileSpawnRequest
{
  public ProjectileSpawnRequest(
      ProjectileId definitionId,
      EntityRuntimeHandle owner,
      ProjectileSourceMetadataComponent source,
      Vector2 position,
      Faction faction)
  {
    if (!definitionId.IsValid)
    {
      throw new ArgumentException("A projectile definition id is required.", nameof(definitionId));
    }

    if (!owner.IsValid)
    {
      throw new ArgumentException("A projectile owner handle is required.", nameof(owner));
    }

    if (string.IsNullOrWhiteSpace(source.SourceKind) ||
        string.IsNullOrWhiteSpace(source.SourceId) || source.SourceRevision < 0)
    {
      throw new ArgumentException("Projectile source metadata is invalid.", nameof(source));
    }

    ValidateFinite(position, nameof(position));
    if (!Enum.IsDefined(faction))
    {
      throw new ArgumentOutOfRangeException(nameof(faction));
    }

    DefinitionId = definitionId;
    Owner = owner;
    Source = source;
    Position = position;
    Faction = faction;
  }

  public ProjectileId DefinitionId { get; }

  public EntityRuntimeHandle Owner { get; }

  public ProjectileSourceMetadataComponent Source { get; }

  public Vector2 Position { get; }

  /// <summary>
  /// 阵营。出生速度不在这里：D3 把速度交给 AI 行为，生成请求只回答"谁、用什么、在哪"。
  /// </summary>
  public Faction Faction { get; }

  private static void ValidateFinite(Vector2 value, string parameterName)
  {
    if (!float.IsFinite(value.X) || !float.IsFinite(value.Y))
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}
