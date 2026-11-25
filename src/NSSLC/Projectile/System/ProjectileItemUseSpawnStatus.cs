namespace Terraria.Unshared.Projectiles;

/// <summary>Why one item-use fact did or did not become a spawn request.</summary>
public enum ProjectileItemUseSpawnStatus : byte
{
  /// <summary>请求已入队，本 tick 由生成接缝落地。</summary>
  Enqueued = 0,

  /// <summary>该物品没有使用内容定义（内容侧漏项）。</summary>
  UseUnknown = 1,

  /// <summary>物品可以使用但不射击（没有 shoot 配置）。</summary>
  NoShootProfile = 2,

  /// <summary>射击配置引用的投射物定义不存在。</summary>
  DefinitionUnknown = 3,

  /// <summary>持有者已不在世界或缺少位置。</summary>
  OwnerUnavailable = 4
}
