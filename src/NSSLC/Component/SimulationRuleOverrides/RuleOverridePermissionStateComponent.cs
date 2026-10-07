using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Terraria.SimulationRuleOverrides;

/// <summary>
/// 保存创造模式规则的默认和当前权限。
/// </summary>
/// <remarks>
/// <para>拆分来源：由 CreativePowers 的开关、滑块和权限状态重组。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.GameContent.Creative/CreativePowers.cs。</para>
/// <para>重组说明：原 Power 内部状态按世界、玩家和权限主体分别重组；版本字段是新增状态。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-07-version4-second-round-review-merged.md。</para>
/// <para>依据位置：第 1721 行。</para>
/// </remarks>
public sealed class RuleOverridePermissionStateComponent
{
  public RuleOverridePermissionStateComponent(
    IReadOnlyDictionary<RuleKey, PowerPermissionLevel> defaultPermissionByRule,
    IReadOnlyDictionary<RuleKey, PowerPermissionLevel> currentPermissionByRule,
    long? permissionRevision = null)
  {
    var defaultPermissions = CopyAndValidate(
      defaultPermissionByRule,
      nameof(defaultPermissionByRule));
    var currentPermissions = CopyAndValidate(
      currentPermissionByRule,
      nameof(currentPermissionByRule));

    ValidateSameRuleSet(defaultPermissions, currentPermissions);
    ValidateRevision(permissionRevision, nameof(permissionRevision));

    DefaultPermissionByRule = new ReadOnlyDictionary<RuleKey, PowerPermissionLevel>(
      defaultPermissions);
    CurrentPermissionByRule = new ReadOnlyDictionary<RuleKey, PowerPermissionLevel>(
      currentPermissions);
    PermissionRevision = permissionRevision;
  }

  public IReadOnlyDictionary<RuleKey, PowerPermissionLevel> DefaultPermissionByRule { get; }

  public IReadOnlyDictionary<RuleKey, PowerPermissionLevel> CurrentPermissionByRule { get; }

  public long? PermissionRevision { get; }

  private static Dictionary<RuleKey, PowerPermissionLevel> CopyAndValidate(
    IReadOnlyDictionary<RuleKey, PowerPermissionLevel> source,
    string parameterName)
  {
    if (source is null)
    {
      throw new ArgumentNullException(parameterName);
    }

    if (source.Count == 0)
    {
      throw new ArgumentException(
        "At least one registered rule is required.",
        parameterName);
    }

    var copy = new Dictionary<RuleKey, PowerPermissionLevel>(source.Count);
    foreach (var entry in source)
    {
      if (!entry.Key.IsValid)
      {
        throw new ArgumentException(
          "The permission map contains an invalid rule key.",
          parameterName);
      }

      if (!IsDefinedPermission(entry.Value))
      {
        throw new ArgumentException(
          "The permission value is not part of the supported permission domain.",
          parameterName);
      }

      copy.Add(entry.Key, entry.Value);
    }

    return copy;
  }

  private static void ValidateSameRuleSet(
    IReadOnlyDictionary<RuleKey, PowerPermissionLevel> defaults,
    IReadOnlyDictionary<RuleKey, PowerPermissionLevel> current)
  {
    if (defaults.Count != current.Count)
    {
      throw new ArgumentException(
        "Default and current permission maps must contain the same rule set.");
    }

    foreach (var ruleKey in defaults.Keys)
    {
      if (!current.ContainsKey(ruleKey))
      {
        throw new ArgumentException(
          "Default and current permission maps must contain the same rule set.");
      }
    }
  }

  private static bool IsDefinedPermission(PowerPermissionLevel permission)
  {
    return permission is PowerPermissionLevel.LockedForEveryone
      or PowerPermissionLevel.CanBeChangedByHostAlone
      or PowerPermissionLevel.CanBeChangedByEveryone;
  }

  private static void ValidateRevision(long? value, string parameterName)
  {
    if (value is < 0)
    {
      throw new ArgumentOutOfRangeException(
        parameterName,
        value,
        "The revision must be non-negative when present.");
    }
  }
}
