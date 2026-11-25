namespace Terraria.Unshared.Projectiles;

/// <summary>为什么一条发射意图最终没有变成生成请求。</summary>
public enum EmissionDrainStatus : byte
{
  /// <summary>已解析并入了生成请求队列。</summary>
  Enqueued = 0,

  /// <summary>内容 id 无法解析成投射物定义（内容侧漏项或拼写错误）。</summary>
  DefinitionUnknown = 1,

  /// <summary>发射者句柄在本世界解析不到（已销毁、跨世界或从未注册）。</summary>
  EmitterUnavailable = 2,

  /// <summary>解析到的实体缺少位置或阵营之外的必需状态，无法确定生成归属。</summary>
  EmitterNotProjectileOwner = 3
}

/// <summary>一条发射意图的排空去向，逐条保留供宿主与验证项目读取。</summary>
public readonly record struct EmissionDrainResult(
    string ContentId,
    EmissionDrainStatus Status,
    ProjectileLifecycleStatus LifecycleStatus = ProjectileLifecycleStatus.Applied);
