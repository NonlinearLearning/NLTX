using MinimalImplementation.Combat.Identity;
using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Npcs;

public sealed partial class NpcManager {
  private enum CoreLootMode { Core, Common, Global }

  private sealed record CoreLootRoll(
      CoreLootMode Mode, IReadOnlyList<NpcDropIntent> Drops);

  private const int MaximumPendingDeaths = 1024;
  private readonly Dictionary<CombatEntityIdentity, NpcDeathRecord> _pendingDeaths = [];
  private readonly Dictionary<CombatEntityIdentity, CoreLootRoll> _coreLoot = [];
  private bool _isRollingCoreLoot;

  /// <summary>
  /// Rolls migrated core loot once per dead generation. Repreparing after owner-version
  /// conflicts or identity retirement failure reuses the roll. Global loot is not included.
  /// </summary>
  public NpcPreparation<NpcDeathPlan> PrepareCoreDeath(
      CombatEntityIdentity identity, long expectedRevision,
      NpcLootConditions conditions, INpcLootRandom random) {
    return PrepareCoreLootDeath(identity, expectedRevision, conditions, null, null, random);
  }

  /// <summary>
  /// Includes common money/life/mana drops. Choose one loot entry on
  /// the first preparation for a generation; retries retain that completed roll.
  /// This entry omits global item rules and death events.
  /// </summary>
  public NpcPreparation<NpcDeathPlan> PrepareCoreDeathWithCommonLoot(
      CombatEntityIdentity identity, long expectedRevision, NpcLootConditions conditions,
      NpcCommonLootFacts commonLoot, INpcLootRandom random) {
    ArgumentNullException.ThrowIfNull(commonLoot);
    return PrepareCoreLootDeath(identity, expectedRevision, conditions, commonLoot, null, random);
  }

  /// <summary>
  /// Includes the global item rules and common money/life/mana for type 1/2/3.
  /// The first successful roll freezes this mode. Death events remain separate.
  /// </summary>
  public NpcPreparation<NpcDeathPlan> PrepareCoreDeathWithGlobalLoot(
      CombatEntityIdentity identity, long expectedRevision, NpcLootConditions conditions,
      NpcCommonLootFacts commonLoot, NpcGlobalLootFacts globalLoot, INpcLootRandom random) {
    ArgumentNullException.ThrowIfNull(commonLoot);
    ArgumentNullException.ThrowIfNull(globalLoot);
    return PrepareCoreLootDeath(
        identity, expectedRevision, conditions, commonLoot, globalLoot, random);
  }

  private NpcPreparation<NpcDeathPlan> PrepareCoreLootDeath(
      CombatEntityIdentity identity, long expectedRevision, NpcLootConditions conditions,
      NpcCommonLootFacts? commonLoot, NpcGlobalLootFacts? globalLoot, INpcLootRandom random) {
    ArgumentNullException.ThrowIfNull(conditions);
    ArgumentNullException.ThrowIfNull(random);
    if (_isRollingCoreLoot) {
      return NpcPreparation<NpcDeathPlan>.Rejected(
          ProtocolErrorCode.Conflict, "Loot callbacks must not reenter death preparation.");
    }
    if (commonLoot is not null) {
      if (commonLoot.Entity != identity || commonLoot.SourceNpcRevision != expectedRevision) {
        return NpcPreparation<NpcDeathPlan>.Rejected(
            ProtocolErrorCode.StaleEntity, "Common loot facts target a different NPC revision.");
      }
      if (!commonLoot.HasValidNumbers) {
        return NpcPreparation<NpcDeathPlan>.Rejected(
            ProtocolErrorCode.InvalidLength, "Common loot facts exceed the numeric envelope.");
      }
    }
    if (globalLoot is not null) {
      if (globalLoot.Entity != identity || globalLoot.SourceNpcRevision != expectedRevision) {
        return NpcPreparation<NpcDeathPlan>.Rejected(
            ProtocolErrorCode.StaleEntity, "Global loot facts target a different NPC revision.");
      }
      if (!globalLoot.HasValidNumbers) {
        return NpcPreparation<NpcDeathPlan>.Rejected(
            ProtocolErrorCode.InvalidLength, "Global loot facts exceed arithmetic bounds.");
      }
    }
    NpcPreparation<NpcDeathPlan> check = PrepareDeath(
        identity, expectedRevision, Array.Empty<NpcDropIntent>());
    if (!check.IsPrepared || check.Plan is null) {
      return check;
    }
    Npc npc = check.Plan.Retirement.Npc;
    CoreLootMode mode = globalLoot is not null ? CoreLootMode.Global :
        commonLoot is not null ? CoreLootMode.Common : CoreLootMode.Core;
    if (_coreLoot.TryGetValue(identity, out CoreLootRoll? cached)) {
      if (cached.Mode != mode) {
        return NpcPreparation<NpcDeathPlan>.Rejected(
            ProtocolErrorCode.Conflict, "The loot mode for this NPC generation is already frozen.");
      }
      return PrepareDeath(identity, expectedRevision, cached.Drops);
    }

    IReadOnlyList<NpcDropIntent> drops;
    _isRollingCoreLoot = true;
    try {
      NpcView snapshot = npc.Capture();
      bool anyInteractions = npc.CaptureInteractions().Count > 0;
      if (commonLoot is not null && globalLoot is not null) {
        drops = NpcLootService.RollCoreWithGlobalLoot(
            snapshot, conditions, anyInteractions, commonLoot, globalLoot, random);
      } else if (commonLoot is not null) {
        drops = NpcLootService.RollCoreWithCommonLoot(
            snapshot, conditions, anyInteractions, commonLoot, random);
      } else {
        drops = NpcLootService.RollCore(snapshot, conditions, anyInteractions, random);
      }
    } finally {
      _isRollingCoreLoot = false;
    }

    // A random adapter is external code. Do not cache a roll against an NPC/world
    // that it mutated or retired while the roll was in progress.
    OwnerCommitResult validation = Validate(check.Plan);
    if (validation.Status != OwnerCommitStatus.Applied) {
      return NpcPreparation<NpcDeathPlan>.Rejected(
          validation.ErrorCode ?? ProtocolErrorCode.Conflict,
          validation.Detail ?? "NPC state changed while rolling loot.");
    }
    _coreLoot.Add(identity, new CoreLootRoll(mode, drops));
    return PrepareDeath(identity, expectedRevision, drops);
  }

  /// <summary>Loot is prepared once by the caller, including global/coin/heal rules.</summary>
  public NpcPreparation<NpcDeathPlan> PrepareDeath(
      CombatEntityIdentity identity, long expectedRevision, IReadOnlyList<NpcDropIntent> drops) {
    ArgumentNullException.ThrowIfNull(drops);
    NpcPreparation<NpcRetirementPlan> retirement = PrepareRetire(identity);
    if (!retirement.IsPrepared || retirement.Plan is null) {
      return NpcPreparation<NpcDeathPlan>.Rejected(
          retirement.Result.ErrorCode ?? ProtocolErrorCode.Conflict,
          retirement.Result.Detail ?? "NPC retirement could not be prepared.");
    }
    Npc npc = retirement.Plan.Npc;
    if (npc.Revision != expectedRevision) {
      return NpcPreparation<NpcDeathPlan>.Rejected(
          ProtocolErrorCode.StaleEntity, "The NPC changed after loot preparation.");
    }
    if (npc.Type is not (1 or 2 or 3)) {
      return NpcPreparation<NpcDeathPlan>.Rejected(
          ProtocolErrorCode.NotReady, "Type-specific death transformations are not migrated.");
    }
    if (_pendingDeaths.Count >= MaximumPendingDeaths) {
      return NpcPreparation<NpcDeathPlan>.Rejected(
          ProtocolErrorCode.NotReady, "Settle pending NPC deaths before retiring more NPCs.");
    }
    if (drops.Any(drop => drop is null || drop.ItemType <= 0 || drop.Stack <= 0)) {
      return NpcPreparation<NpcDeathPlan>.Rejected(
          ProtocolErrorCode.InvalidLength, "NPC drops require positive item types and stacks.");
    }
    return NpcPreparation<NpcDeathPlan>.Prepared(new NpcDeathPlan(retirement.Plan, drops));
  }

  public OwnerCommitResult Validate(NpcDeathPlan plan) {
    ArgumentNullException.ThrowIfNull(plan);
    if (_pendingDeaths.Count >= MaximumPendingDeaths) {
      return OwnerCommitResult.Rejected(
          ProtocolErrorCode.NotReady, "The NPC death outbox is full.");
    }
    return Validate(plan.Retirement);
  }

  public (OwnerCommitResult Result, NpcDeathRecord? Death) Commit(NpcDeathPlan plan) {
    ArgumentNullException.ThrowIfNull(plan);
    OwnerCommitResult validation = Validate(plan);
    if (validation.Status != OwnerCommitStatus.Applied) {
      return (validation, null);
    }
    Npc npc = plan.Retirement.Npc;
    IReadOnlyList<SessionEpoch> interactions = npc.CaptureInteractions();
    var retired = Commit(plan.Retirement);
    if (retired.Result.Status != OwnerCommitStatus.Applied || retired.RetiredNpc is null) {
      return (retired.Result, null);
    }
    var death = new NpcDeathRecord(retired.RetiredNpc, _commitVersion,
        interactions, npc.LastInteraction, plan.Drops);
    _pendingDeaths.Add(npc.Entity, death);
    _coreLoot.Remove(npc.Entity);
    return (retired.Result, death);
  }

  /// <summary>Deliver in this order: Banner uses one monotonic P05 source cursor.</summary>
  public IReadOnlyList<NpcDeathRecord> CapturePendingDeaths() {
    if (!IsWorldScopeCurrent()) {
      return Array.Empty<NpcDeathRecord>();
    }
    return Array.AsReadOnly(_pendingDeaths.Values
        .Where(death => death.Npc.Entity.WorldScope == _worldScope)
        .OrderBy(death => death.SourceCommitVersion.Value).ToArray());
  }

  /// <summary>
  /// Call only after both item delivery and contribution owners confirm settlement.
  /// </summary>
  public bool AcknowledgeDeath(NpcDeathRecord death) {
    ArgumentNullException.ThrowIfNull(death);
    if (!IsWorldScopeCurrent() ||
        !_pendingDeaths.TryGetValue(death.Npc.Entity, out NpcDeathRecord? pending) ||
        !ReferenceEquals(pending, death)) {
      return false;
    }
    return _pendingDeaths.Remove(death.Npc.Entity);
  }

  /// <summary>
  /// Binds the slime-rain counter calculation to the already committed death outbox.
  /// P07 owns the counter and spawn application; this owner never writes event state.
  /// </summary>
  public NpcPreparation<NpcSlimeRainDeathPlan> PrepareSlimeRainDeath(
      NpcSlimeRainDeathFacts facts)
  {
    ArgumentNullException.ThrowIfNull(facts);
    if (!IsWorldScopeCurrent() ||
        !_pendingDeaths.TryGetValue(facts.Entity, out NpcDeathRecord? death) ||
        death.Npc.Entity != facts.Entity ||
        death.Npc.Revision != facts.SourceNpcRevision ||
        death.Npc.Type != facts.NpcType)
    {
      return NpcPreparation<NpcSlimeRainDeathPlan>.Rejected(
          ProtocolErrorCode.StaleEntity,
          "Slime-rain facts do not target a pending NPC death generation.");
    }

    NpcSlimeRainDeathResult result = NpcSlimeRainDeathRules.Evaluate(facts);
    if (!result.IsAccepted)
    {
      return NpcPreparation<NpcSlimeRainDeathPlan>.Rejected(
          result.ErrorCode ?? ProtocolErrorCode.NotReady,
          result.Detail ?? "Slime-rain death facts were rejected.");
    }

    return NpcPreparation<NpcSlimeRainDeathPlan>.Prepared(
        new NpcSlimeRainDeathPlan(_revision, death, facts, result));
  }

  public OwnerCommitResult Validate(NpcSlimeRainDeathPlan plan)
  {
    ArgumentNullException.ThrowIfNull(plan);
    if (!IsWorldScopeCurrent() || plan.ManagerRevision != _revision ||
        !_pendingDeaths.TryGetValue(plan.Death.Npc.Entity, out NpcDeathRecord? current) ||
        !ReferenceEquals(current, plan.Death) ||
        current.Npc.Revision != plan.Facts.SourceNpcRevision)
    {
      return OwnerCommitResult.Rejected(
          ProtocolErrorCode.StaleEntity,
          "Slime-rain death preparation changed or was settled.");
    }

    return OwnerCommitResult.Applied(_commitVersion);
  }

  /// <summary>Returns the event result; applying its counter/spawn is an external owner effect.</summary>
  public (OwnerCommitResult Result, NpcSlimeRainDeathResult? Event) Commit(
      NpcSlimeRainDeathPlan plan)
  {
    ArgumentNullException.ThrowIfNull(plan);
    OwnerCommitResult validation = Validate(plan);
    return validation.Status == OwnerCommitStatus.Applied
        ? (validation, plan.Result)
        : (validation, null);
  }
}
