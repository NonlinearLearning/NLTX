using System;

namespace Terraria.Npc;

/// <summary>
/// 保存 NPC 类型、网络变体、初始类型和定义版本。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.NPC。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/NPC.cs。</para>
/// <para>主要源成员：type（第 6307 行）； netID（第 6391 行）。</para>
/// <para>重组说明：InitialTypeId 与 CatalogRevision 是定义初始化和版本管理新增的状态。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>拆分依据文件：2026-09-05-version4-npc-and-town-simulation-component-design.md。</para>
/// <para>依据位置：第 189 行。</para>
/// </remarks>
public sealed class NpcDefinitionReferenceComponent
{
  public NpcDefinitionReferenceComponent(NpcTypeId typeId, NpcNetId netId, int catalogRevision = 0)
  {
    if (!typeId.IsValid)
    {
      throw new ArgumentException(
        "NpcTypeId must be valid.",
        nameof(typeId));
    }

    TypeId = typeId;
    NetId = netId;
    InitialTypeId = typeId;
    CatalogRevision = catalogRevision;
  }

  public NpcTypeId TypeId { get; }

  public NpcNetId NetId { get; }

  public NpcTypeId InitialTypeId { get; }

  public int CatalogRevision { get; }

  public bool UsesNetIdVariant => NetId.IsVariant;

  public bool IsInitialized => TypeId.IsValid && InitialTypeId.IsValid;
}
