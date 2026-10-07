using Terraria.Relationships;

namespace EntityEcs.Components;

/// <summary>
/// 保存 ECS 实体的全局 UUID 身份。
/// </summary>
/// <remarks>
/// <para>拆分来源：由 Entity.whoAmI 的旧槽位身份模型重组。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Entity.cs。</para>
/// <para>重组说明：Uuid 是 ECS 全局身份模型新增的数据；不等同于旧 whoAmI 数组索引。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/non-authoritative/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-non-authoritative-P06-entity-lifecycle-attribution-component-design.md。
/// </para>
/// <para>依据位置：第 35 行。</para>
/// </remarks>
public readonly record struct EntityIdentityComponent
{
  internal EntityIdentityComponent(EntityUuid uuid)
  {
    if (!uuid.IsAssigned)
    {
      throw new ArgumentException("An entity identity component requires an assigned UUID.", nameof(uuid));
    }

    Uuid = uuid;
  }

  public EntityUuid Uuid { get; }
}
