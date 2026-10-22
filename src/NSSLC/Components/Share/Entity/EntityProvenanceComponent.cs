using Terraria.Relationships;

namespace EntityEcs.Components;

/// <summary>
/// 保存实体创建原因、来源实体和因果关联。
/// </summary>
/// <remarks>
/// <para>拆分来源：由 IEntitySource 及 EntitySource_Parent 的实体创建来源模型重组。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.DataStructures/EntitySource_Parent.cs。</para>
/// <para>重组说明：来源种类、实体引用和因果关系按 ECS 重组，不是单个旧实体的字段搬迁。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/non-authoritative/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-non-authoritative-P06-entity-lifecycle-attribution-component-design.md。
/// </para>
/// <para>依据位置：第 41 行。</para>
/// <para>
/// 用途：例如玩家使用物品生成射弹时，记录来源玩家和物品定义；射弹再生成子实体时，可继续记录父实体和被命中的实体。
/// </para>
/// <para>
/// 当前实现：只保存来源事实。IsRootCause 只检查 SourceEntity、ParentEntity 和两种定义编号是否为空，不检查 StruckEntity 
/// 或 WorldEvent。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C157，L2（通用可选玩法）。分级理由：记录生成因果有助于掉落、追踪和归属，但当前未发现该类型的生产消费者。
/// </para>
/// </remarks>
public struct EntityProvenanceComponent
{
  /// <summary>
  /// 实体创建来源的类别。
  /// </summary>
  public EntityProvenanceKind Kind;
  /// <summary>
  /// 直接触发创建的来源主体，可为空。
  /// </summary>
  public EntityReference? SourceEntity;
  /// <summary>
  /// 生成此实体的父实体，可为空。
  /// </summary>
  public EntityReference? ParentEntity;
  /// <summary>
  /// 与此次创建有关的被命中实体，可为空。
  /// </summary>
  public EntityReference? StruckEntity;
  /// <summary>
  /// 触发创建的物品定义编号，可为空。
  /// </summary>
  public int? SourceItemDefinitionId;
  /// <summary>
  /// 触发创建的射弹定义编号，可为空。
  /// </summary>
  public int? SourceProjectileDefinitionId;
  /// <summary>
  /// 触发创建的世界事件来源类别，可为空。
  /// </summary>
  public WorldEventSourceKind? WorldEvent;

  /// <summary>
  /// 直接来源、父实体及物品和射弹定义编号都为空时返回 true。
  /// </summary>
  public bool IsRootCause =>
    SourceEntity is null &&
    ParentEntity is null &&
    SourceItemDefinitionId is null &&
    SourceProjectileDefinitionId is null;
}
