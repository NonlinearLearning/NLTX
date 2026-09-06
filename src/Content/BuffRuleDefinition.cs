using System.Collections.Immutable;

namespace Terraria.Content;

public sealed record BuffRuleDefinition(
  bool IsDebuff,
  bool AffectsPvp,
  bool IsPersistent,
  bool NoSave,
  bool NoTimeDisplay = false,
  ImmutableArray<int> ImmuneBuffTypeIds = default,
  int? DefaultDurationTicks = null);
