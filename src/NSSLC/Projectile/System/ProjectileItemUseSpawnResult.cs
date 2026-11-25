using Terraria.Share.Items;
using Terraria.Share.Relationships;

namespace Terraria.Unshared.Projectiles;

/// <summary>One item-use fact's routing outcome.</summary>
/// <remarks>
/// 失败不写日志、不抛异常，而是作为可观测结果交回宿主：tick 管线里抛异常会打断整帧，
/// 而"某个物品的射击配置引用了不存在的定义"属于内容问题，应当在内容加载或验证期暴露。
/// 宿主或验证项目读 <see cref="ProjectileItemUseSpawnSystem.Results" /> 即可确认没有静默丢弃。
/// </remarks>
public readonly record struct ProjectileItemUseSpawnResult(
    EntityRuntimeHandle Owner,
    ItemId ItemId,
    ProjectileItemUseSpawnStatus Status);
