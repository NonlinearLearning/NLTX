using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Npcs;

public sealed partial class Npc {
  private readonly HashSet<SessionEpoch> _interactions = [];
  private NpcCombatStats _combatStats;

  public bool JustHit { get; private set; }
  internal SessionEpoch? LastInteraction { get; private set; }
  internal NpcMotionState Motion { get; private set; } = NpcMotionState.Initial;

  internal void ApplyMotion(NpcMotionState motion) {
    Motion = motion;
  }

  internal void ApplyCombatStats(NpcCombatStats stats) {
    _combatStats = stats;
  }

  internal void RecordHit(SessionEpoch? epoch) {
    JustHit = true;
    if (epoch.HasValue) {
      _interactions.Add(epoch.Value);
      LastInteraction = epoch;
    }
  }

  internal IReadOnlyList<SessionEpoch> CaptureInteractions() {
    return Array.AsReadOnly(_interactions.OrderBy(epoch => epoch.Value).ToArray());
  }

  internal void ClearJustHit() {
    JustHit = false;
  }
}
