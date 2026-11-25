namespace Terraria.Unshared.Projectiles;

/// <summary>Declares the collision decisions that are unique to one projectile instance.</summary>
/// <remarks>
/// 这里只保留两个**投射物特有**的判定：
/// <list type="bullet">
/// <item><see cref="CollideWithTiles"/>：投射物是否参与地形碰撞。</item>
/// <item><see cref="IgnoreOwner"/>：命中判定时是否排除发射者。</item>
/// </list>
///
/// 原先还有 <c>CollideWithEntities</c> 与 <c>IgnoreSameFaction</c> 两个字段，已删除，
/// 它们是把别处已有的模型在投射物实例上又声明了一遍：
/// <list type="bullet">
/// <item><c>CollideWithEntities</c> 与 <see cref="Terraria.Share.Physics.EntityCollisionCapabilityComponent"/>
/// 重复——<c>Projectile</c> 本来就单独携带了后者，同一份"是否参与实体碰撞"
/// 有了两个真相来源。</item>
/// <item><c>IgnoreSameFaction</c> 属于阵营关系判定，归
/// <c>Terraria.Share.Combat.DamageRelationSystem</c> 负责，不应由每个投射物实例单独声明。</item>
/// </list>
/// 两者在全仓都没有任何读取方（只有声明与构造处），删除不改变行为。
/// </remarks>
public readonly record struct ProjectileCollisionPolicyComponent(
    bool CollideWithTiles,
    bool IgnoreOwner)
{
}
