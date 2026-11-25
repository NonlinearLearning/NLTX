namespace Terraria.Unshared.Projectiles;

/// <summary>Read-only context handed to every projectile section.</summary>
/// <remarks>
/// 只携带内容 id：section 不该知道世界、编目或别的 section。需要诊断信息时用它拼消息，
/// 而不是反过来让 section 持有编目引用。
/// </remarks>
public readonly record struct ProjectileSectionContext(ProjectileId Id);
