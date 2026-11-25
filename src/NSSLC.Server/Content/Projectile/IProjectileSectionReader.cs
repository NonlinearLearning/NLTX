using Terraria.Share.Lvcs;
using Terraria.Unshared.Projectiles;

namespace NSSLC.Server.Content.Projectile;

/// <summary>把一个内容族的 JSON 片段读成 section，并负责本族字段校验。</summary>
/// <remarks>
/// 这个接口存在的唯一理由：新增一个内容族不应该改 <see cref="ProjectileCodec" />。
/// codec 只遍历读取器、按返回顺序拼 section 列表；族名、字段名、取值范围都在读取器里，
/// 因此「1 个 section + 1 个读取器 + JSON」就是新增一族的全部改动。
/// <para>
/// 校验与读取分成两步：<see cref="Validate" /> 只报诊断不构造对象，<see cref="Read" />
/// 只在通过校验后调用。合在一起会让"先收集全部诊断"这件事做不到。
/// </para>
/// </remarks>
internal interface IProjectileSectionReader
{
  /// <summary>族名，用于诊断定位；与 JSON 字段同名。</summary>
  string Family { get; }

  /// <summary>校验本族字段；返回 false 表示已写入至少一条错误诊断。</summary>
  bool Validate(
      ProjectilePrototype prototype,
      LvcsFamilyId familyId,
      string location,
      LvcsDiagnosticSink diagnostics);

  /// <summary>折出本族 section；族未参与时返回 null。调用前必须先通过 <see cref="Validate" />。</summary>
  IProjectileSection? Read(ProjectilePrototype prototype);
}
