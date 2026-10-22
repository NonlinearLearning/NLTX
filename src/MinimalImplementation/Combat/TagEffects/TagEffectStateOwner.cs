using MinimalImplementation.Combat.Identity;
using MinimalImplementation.Combat.Npcs;
using MinimalImplementation.Combat.Projectiles;
using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.TagEffects;

/// <summary>Single-writer owner for player TagEffect state and NPC-slot generations.</summary>
public sealed class TagEffectStateOwner
{
  private readonly ICombatEntityIdentityAuthority _identityAuthority;
  private readonly NpcManager _npcs;
  private readonly TagEffectDefinitionCatalog _definitions;
  private readonly int _npcSlotCount;
  private readonly Dictionary<byte, PlayerTagEffectState> _players = [];
  private long _revision;
  private CommitVersion _commitVersion = CommitVersion.Initial;
  private long _worldScope;

  public TagEffectStateOwner(
    ICombatEntityIdentityAuthority identityAuthority,
    NpcManager npcs,
    TagEffectDefinitionCatalog definitions,
    int npcSlotCount = NpcManager.TerrariaNpcSlotCapacity)
  {
    ArgumentNullException.ThrowIfNull(identityAuthority);
    ArgumentNullException.ThrowIfNull(npcs);
    ArgumentNullException.ThrowIfNull(definitions);
    if (npcSlotCount is <= 0 or > byte.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(npcSlotCount));
    }

    long worldScope = identityAuthority.CurrentWorldScope;
    if (worldScope <= 0)
    {
      throw new ArgumentException(
        "TagEffect state requires an active world scope.",
        nameof(identityAuthority));
    }

    _identityAuthority = identityAuthority;
    _npcs = npcs;
    _definitions = definitions;
    _npcSlotCount = npcSlotCount;
    _worldScope = worldScope;
  }

  public long Revision => _revision;

  public CommitVersion CommitVersion => _commitVersion;

  public int NpcSlotCount => _npcSlotCount;

  /// <summary>
  /// Handles one decoded module 12 message. The packet's Player field is untrusted and is
  /// replaced with the player slot resolved by P02 before the returned broadcast is routed.
  /// </summary>
  public TagEffectDispatchResult Handle(
    InboundPacketEnvelope envelope,
    AuthenticatedCombatSender sender,
    TagEffectCommand command)
  {
    ArgumentNullException.ThrowIfNull(envelope);
    ArgumentNullException.ThrowIfNull(command);
    if (envelope.MessageId != 82)
    {
      return Rejected(ProtocolErrorCode.UnknownPacket, "Expected Packet 82.");
    }

    if (envelope.ModuleId != 12)
    {
      return Rejected(ProtocolErrorCode.UnknownModule, "Expected Packet 82 module 12.");
    }

    if (sender.Epoch != envelope.Epoch)
    {
      return Stale(
        ProtocolErrorCode.StaleEpoch,
        "The authenticated sender does not match the envelope epoch.");
    }

    if (command.Action == 0)
    {
      return Rejected(
        ProtocolErrorCode.Forbidden,
        "TagEffect FullState is server-produced and cannot mutate server state.");
    }

    OwnerCommitResult scopeResult = SynchronizeWorldScope();
    if (scopeResult.Status != OwnerCommitStatus.Applied)
    {
      return new TagEffectDispatchResult(scopeResult, null);
    }

    if (envelope.WorldScope is long envelopeScope && envelopeScope != _worldScope)
    {
      return Stale(
        ProtocolErrorCode.StaleEntity,
        "The TagEffect packet belongs to an expired world scope.");
    }

    if (_players.TryGetValue(sender.PlayerSlot, out PlayerTagEffectState? current) &&
        current.SessionEpoch != sender.Epoch)
    {
      return Stale(
        ProtocolErrorCode.StaleEpoch,
        "The player slot is still bound to a different session epoch.");
    }

    return command.Action switch
    {
      1 => HandleChangeActiveEffect(sender, current, command),
      2 or 3 or 4 => HandleNpcChange(sender, current, command),
      _ => Rejected(ProtocolErrorCode.UnknownSubtype, $"Unknown TagEffect action {command.Action}.")
    };
  }

  public OwnerCommitResult AdvanceTick()
  {
    OwnerCommitResult scopeResult = SynchronizeWorldScope();
    if (scopeResult.Status != OwnerCommitStatus.Applied)
    {
      return scopeResult;
    }

    var changedPlayers = new List<PlayerTagEffectState>();
    foreach (PlayerTagEffectState state in _players.Values)
    {
      if (state.HasActiveEffect && state.HasTickChanges(_identityAuthority))
      {
        changedPlayers.Add(state);
      }
    }

    if (changedPlayers.Count == 0)
    {
      return OwnerCommitResult.Applied(_commitVersion);
    }

    if (!TryPrepareCommit(
          changedPlayers,
          out CommitVersion nextVersion,
          out OwnerCommitResult? failure))
    {
      return failure!;
    }

    foreach (PlayerTagEffectState state in changedPlayers)
    {
      state.AdvanceTimers(_identityAuthority);
    }

    CompleteCommit(changedPlayers, nextVersion);
    return OwnerCommitResult.Applied(nextVersion);
  }

  /// <summary>
  /// Clears timers tied to a retired NPC generation without touching a reused slot.
  /// </summary>
  public OwnerCommitResult RetireNpc(short npcSlot, CombatEntityIdentity retiredIdentity)
  {
    if (npcSlot < 0 || npcSlot >= _npcSlotCount || !retiredIdentity.IsValid)
    {
      return OwnerCommitResult.Rejected(
        ProtocolErrorCode.InvalidLength,
        "NPC retirement requires an in-range slot and a valid retired identity.");
    }

    var changedPlayers = _players.Values
      .Where(state => state.MatchesNpc(npcSlot, retiredIdentity))
      .ToArray();
    if (changedPlayers.Length == 0)
    {
      return OwnerCommitResult.Applied(_commitVersion);
    }

    if (!TryPrepareCommit(
          changedPlayers,
          out CommitVersion nextVersion,
          out OwnerCommitResult? failure))
    {
      return failure!;
    }

    foreach (PlayerTagEffectState state in changedPlayers)
    {
      state.ClearNpcSlot(npcSlot);
    }

    CompleteCommit(changedPlayers, nextVersion);
    return OwnerCommitResult.Applied(nextVersion);
  }

  /// <summary>Releases all state bound to the disconnected session epoch.</summary>
  public OwnerCommitResult RetireSession(SessionEpoch sessionEpoch)
  {
    if (!sessionEpoch.IsValid)
    {
      return OwnerCommitResult.Rejected(
        ProtocolErrorCode.StaleEpoch,
        "A valid session epoch is required.");
    }

    PlayerTagEffectState[] retired = _players.Values
      .Where(state => state.SessionEpoch == sessionEpoch)
      .ToArray();
    if (retired.Length == 0)
    {
      return OwnerCommitResult.Applied(_commitVersion);
    }

    if (!TryPrepareCommit([], out CommitVersion nextVersion, out OwnerCommitResult? failure))
    {
      return failure!;
    }

    foreach (PlayerTagEffectState state in retired)
    {
      state.ClearAllNpcData();
      _players.Remove(state.Player);
    }

    CompleteCommit([], nextVersion);
    return OwnerCommitResult.Applied(nextVersion);
  }

  public IReadOnlyList<TagEffectSnapshotView> CaptureDirty()
  {
    if (!IsWorldScopeCurrent())
    {
      return Array.Empty<TagEffectSnapshotView>();
    }

    var snapshots = new List<TagEffectSnapshotView>();
    foreach (PlayerTagEffectState state in _players.Values)
    {
      if (state.IsDirty)
      {
        snapshots.Add(state.Capture(_definitions));
      }
    }

    return snapshots.AsReadOnly();
  }

  public bool TryCapture(byte player, out TagEffectSnapshotView? snapshot)
  {
    if (IsWorldScopeCurrent() && _players.TryGetValue(player, out PlayerTagEffectState? state))
    {
      snapshot = state.Capture(_definitions);
      return true;
    }

    snapshot = null;
    return false;
  }

  public bool TryCreateFullState(
    TagEffectSnapshotView snapshot,
    out TagEffectCommand? command)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    if (!snapshot.NetSync)
    {
      command = null;
      return false;
    }

    command = new TagEffectCommand(
      player: snapshot.Player,
      action: 0,
      effectType: snapshot.EffectType,
      npcTimes: snapshot.NpcTimes,
      procTimes: snapshot.SyncProcs ? snapshot.ProcTimes : null,
      npcIndex: null);
    return true;
  }

  public void AcknowledgeSnapshot(TagEffectSnapshotView published)
  {
    ArgumentNullException.ThrowIfNull(published);
    if (_players.TryGetValue(published.Player, out PlayerTagEffectState? state) &&
        state.SessionEpoch == published.SessionEpoch && state.Revision == published.Revision &&
        state.CommitVersion == published.CommitVersion)
    {
      state.Acknowledge(published.Revision);
    }
  }

  public bool IsTagged(byte player, NpcView npc)
  {
    ArgumentNullException.ThrowIfNull(npc);
    return TryGetMatchingState(player, npc, out PlayerTagEffectState? state) &&
      state!.GetNpcTime(npc.Slot) > 0;
  }

  public bool CanProcOnNpc(byte player, NpcView npc)
  {
    ArgumentNullException.ThrowIfNull(npc);
    return TryGetMatchingState(player, npc, out PlayerTagEffectState? state) &&
      state!.GetProcTime(npc.Slot) > 0;
  }

  /// <summary>
  /// Applies the source TagEffectState hit order: tagged modifier first, proc modifier
  /// second, then clears proc state only after the hit has committed. Secondary effects are
  /// returned as typed intents; projectile creation remains in the projectile owner.
  /// </summary>
  public TagEffectHitResult ApplyHit(TagEffectHitFacts facts, ITagEffectRandom? random = null)
  {
    ArgumentNullException.ThrowIfNull(facts);
    if (facts.Npc is null || !facts.Npc.Entity.IsValid || facts.Damage < 0 ||
        facts.BonusTagDamage < 0 ||
        (facts.ProjectileType is short projectileType && projectileType <= 0))
    {
      return RejectedHit(
          facts,
          ProtocolErrorCode.InvalidLength,
          "TagEffect hit facts are outside the supported numeric envelope.");
    }

    OwnerCommitResult scopeResult = SynchronizeWorldScope();
    if (scopeResult.Status != OwnerCommitStatus.Applied)
    {
      return FailedHit(facts, scopeResult);
    }

    if (!TryGetMatchingState(facts.Player, facts.Npc, out PlayerTagEffectState? state) ||
        state is null || !_definitions.TryGet(state.EffectType, out TagEffectDefinition definition))
    {
      return AppliedHit(facts, facts.Damage, facts.Critical, false, false, false,
          TagEffectTaggedAction.None, TagEffectProcAction.None, scopeResult);
    }

    if (state.GetNpcTime(facts.Npc.Slot) <= 0 || !facts.ProjectileType.HasValue ||
        !definition.CanRunHitEffects(
          facts.ProjectileOwned, facts.IsMinion, facts.IsMinionShot, facts.IsSentry))
    {
      return AppliedHit(facts, facts.Damage, facts.Critical, false, false, false,
          TagEffectTaggedAction.None, TagEffectProcAction.None, scopeResult);
    }

    int damage = definition.ModifyTaggedHit(
        facts.Damage,
        facts.ProjectileType ?? 0,
        facts.ProjectileOwned,
        facts.IsMinion,
        facts.IsMinionShot,
        facts.IsSentry,
        facts.BonusTagDamage);
    bool critical = facts.Critical;
    if (definition.CritChance > 0)
    {
      if (random is null)
      {
        return FailedHit(facts, OwnerCommitResult.Rejected(
            ProtocolErrorCode.NotReady,
            "This TagEffect hit requires an explicit crit random source."));
      }

      critical |= random.RollPercent(definition.CritChance);
    }

    bool proc = state.GetProcTime(facts.Npc.Slot) > 0;
    if (proc)
    {
      damage = definition.ModifyProcHit(damage);
    }

    if (!proc)
    {
      return AppliedHit(
          facts,
          damage,
          critical,
          true,
          false,
          false,
          definition.TaggedAction,
          TagEffectProcAction.None,
          scopeResult);
    }

    if (!TryPrepareCommit([state], out CommitVersion nextVersion,
          out OwnerCommitResult? failure))
    {
      return FailedHit(facts, failure!);
    }

    state.ClearProc(facts.Npc.Slot, facts.Npc.Entity);
    CompleteCommit([state], nextVersion);
    return AppliedHit(
        facts,
        damage,
        critical,
        true,
        true,
        true,
        definition.TaggedAction,
        definition.ProcAction,
        OwnerCommitResult.Applied(nextVersion));
  }

  private TagEffectDispatchResult HandleChangeActiveEffect(
    AuthenticatedCombatSender sender,
    PlayerTagEffectState? state,
    TagEffectCommand command)
  {
    if (!HasShape(command, effectType: true, npcTimes: false, procTimes: false, npcIndex: false))
    {
      return Rejected(
        ProtocolErrorCode.InvalidLength,
        "TagEffect action 1 must contain only an effect type.");
    }

    short effectType = command.EffectType!.Value;
    TagEffectDefinition? nextDefinition = null;
    if (effectType != 0 && !_definitions.TryGet(effectType, out nextDefinition!))
    {
      return Rejected(
        ProtocolErrorCode.NotReady,
        $"TagEffect type {effectType} has no migrated Version4 content definition.");
    }

    bool previousWasNetworked = state is not null &&
      _definitions.TryGet(state.EffectType, out TagEffectDefinition previousDefinition) &&
      previousDefinition.NetSync;
    if (!previousWasNetworked && nextDefinition?.NetSync != true)
    {
      return Rejected(
        ProtocolErrorCode.Forbidden,
        "Only transitions into or out of a Version4 network-synchronized effect are accepted.");
    }

    if (state?.EffectType == effectType)
    {
      return Broadcasted(OwnerCommitResult.Applied(_commitVersion), sender.PlayerSlot, command);
    }

    IReadOnlyCollection<PlayerTagEffectState> plannedChanges = state is null
      ? Array.Empty<PlayerTagEffectState>()
      : new[] { state };
    if (!TryPrepareCommit(plannedChanges, out CommitVersion nextVersion,
          out OwnerCommitResult? failure))
    {
      return new TagEffectDispatchResult(failure!, null);
    }

    PlayerTagEffectState updated = state ?? new PlayerTagEffectState(
      sender.PlayerSlot,
      sender.Epoch,
      _npcSlotCount);
    updated.ChangeActiveEffect(effectType);
    if (state is null)
    {
      _players.Add(sender.PlayerSlot, updated);
    }

    IReadOnlyCollection<PlayerTagEffectState> changedPlayers = state is null
      ? Array.Empty<PlayerTagEffectState>()
      : new[] { updated };
    CompleteCommit([updated], nextVersion);
    return Broadcasted(
      OwnerCommitResult.Applied(nextVersion),
      sender.PlayerSlot,
      command);
  }

  private TagEffectDispatchResult HandleNpcChange(
    AuthenticatedCombatSender sender,
    PlayerTagEffectState? state,
    TagEffectCommand command)
  {
    if (!HasShape(command, effectType: false, npcTimes: false, procTimes: false, npcIndex: true))
    {
      return Rejected(
        ProtocolErrorCode.InvalidLength,
        "TagEffect NPC actions must contain only an NPC slot.");
    }

    if (state is null || !state.HasActiveEffect ||
        !_definitions.TryGet(state.EffectType, out TagEffectDefinition definition) ||
        !definition.NetSync)
    {
      return Rejected(
        ProtocolErrorCode.Forbidden,
        "The player has no active network-synchronized TagEffect.");
    }

    byte npcSlot = command.NpcIndex!.Value;
    if (npcSlot >= _npcSlotCount || !_npcs.TryGet(npcSlot, out NpcView? npc) || npc is null ||
        npc.Entity.WorldScope != _worldScope || !_identityAuthority.IsActive(npc.Entity))
    {
      return Stale(
        ProtocolErrorCode.StaleEntity,
        "The NPC slot is inactive or belongs to an expired generation.");
    }

    if (command.Action == 2 && !definition.CanApplyTagToNpc(npc.Type))
    {
      return Rejected(
        ProtocolErrorCode.Forbidden,
        "The active TagEffect cannot be applied to this NPC type.");
    }

    if (!TryPrepareCommit([state], out CommitVersion nextVersion, out OwnerCommitResult? failure))
    {
      return new TagEffectDispatchResult(failure!, null);
    }

    switch (command.Action)
    {
      case 2:
        state.ApplyTag(npcSlot, npc.Entity, definition.TagDuration);
        break;
      case 3:
        state.EnableProc(npcSlot, npc.Entity, definition.TagDuration);
        break;
      case 4:
        state.ClearProc(npcSlot, npc.Entity);
        break;
    }

    CompleteCommit([state], nextVersion);
    return Broadcasted(OwnerCommitResult.Applied(nextVersion), sender.PlayerSlot, command);
  }

  private OwnerCommitResult SynchronizeWorldScope()
  {
    long currentScope = _identityAuthority.CurrentWorldScope;
    if (currentScope <= 0)
    {
      return OwnerCommitResult.Rejected(
        ProtocolErrorCode.NotReady,
        "The identity authority has no active world scope.");
    }

    if (currentScope == _worldScope)
    {
      return OwnerCommitResult.Applied(_commitVersion);
    }

    PlayerTagEffectState[] affectedPlayers = _players.Values.ToArray();
    if (affectedPlayers.Length == 0)
    {
      _worldScope = currentScope;
      return OwnerCommitResult.Applied(_commitVersion);
    }

    if (!TryPrepareCommit(affectedPlayers, out CommitVersion nextVersion,
          out OwnerCommitResult? failure))
    {
      return failure!;
    }

    foreach (PlayerTagEffectState state in affectedPlayers)
    {
      state.ClearAllNpcData();
    }

    _worldScope = currentScope;
    CompleteCommit(affectedPlayers, nextVersion);
    return OwnerCommitResult.Applied(nextVersion);
  }

  private bool TryGetMatchingState(
    byte player,
    NpcView npc,
    out PlayerTagEffectState? state)
  {
    if (_players.TryGetValue(player, out state) && state.HasActiveEffect &&
        npc.Entity.WorldScope == _worldScope && _identityAuthority.IsActive(npc.Entity) &&
        state.MatchesNpc(npc.Slot, npc.Entity))
    {
      return true;
    }

    state = null;
    return false;
  }

  private bool IsWorldScopeCurrent()
  {
    return _worldScope > 0 && _identityAuthority.CurrentWorldScope == _worldScope;
  }

  private bool TryPrepareCommit(
    IReadOnlyCollection<PlayerTagEffectState> changedPlayers,
    out CommitVersion nextVersion,
    out OwnerCommitResult? failure)
  {
    if (_revision == long.MaxValue ||
        changedPlayers.Any(static state => state.Revision == long.MaxValue))
    {
      nextVersion = default;
      failure = OwnerCommitResult.Rejected(
        ProtocolErrorCode.NotReady,
        "TagEffect owner revision range is exhausted.");
      return false;
    }

    try
    {
      nextVersion = _commitVersion.Next();
      failure = null;
      return true;
    }
    catch (InvalidOperationException)
    {
      nextVersion = default;
      failure = OwnerCommitResult.Rejected(
        ProtocolErrorCode.NotReady,
        "TagEffect commit version range is exhausted.");
      return false;
    }
  }

  private void CompleteCommit(
    IReadOnlyCollection<PlayerTagEffectState> changedPlayers,
    CommitVersion nextVersion)
  {
    _revision++;
    _commitVersion = nextVersion;
    foreach (PlayerTagEffectState state in changedPlayers)
    {
      state.Touch(nextVersion);
    }
  }

  private static bool HasShape(
    TagEffectCommand command,
    bool effectType,
    bool npcTimes,
    bool procTimes,
    bool npcIndex)
  {
    return (command.EffectType.HasValue == effectType) &&
      ((command.NpcTimes is not null) == npcTimes) &&
      ((command.ProcTimes is not null) == procTimes) &&
      (command.NpcIndex.HasValue == npcIndex);
  }

  private static TagEffectDispatchResult Broadcasted(
    OwnerCommitResult result,
    byte authenticatedPlayer,
    TagEffectCommand command)
  {
    var forwarded = new TagEffectCommand(
      authenticatedPlayer,
      command.Action,
      command.EffectType,
      command.NpcTimes,
      command.ProcTimes,
      command.NpcIndex);
    return new TagEffectDispatchResult(result, forwarded);
  }

  private static TagEffectDispatchResult Rejected(ProtocolErrorCode code, string detail)
  {
    return new TagEffectDispatchResult(OwnerCommitResult.Rejected(code, detail), null);
  }

  private static TagEffectDispatchResult Stale(ProtocolErrorCode code, string detail)
  {
    return new TagEffectDispatchResult(
      new OwnerCommitResult(OwnerCommitStatus.Stale, null, code, detail),
      null);
  }

  private static TagEffectHitResult AppliedHit(
      TagEffectHitFacts facts,
      int damage,
      bool critical,
      bool tagged,
      bool proc,
      bool procCleared,
      TagEffectTaggedAction taggedAction,
      TagEffectProcAction procAction,
      OwnerCommitResult result)
  {
    return new TagEffectHitResult(
        result,
        facts.Entity,
        damage,
        critical,
        tagged,
        proc,
        procCleared,
        taggedAction,
        procAction);
  }

  private static TagEffectHitResult FailedHit(
      TagEffectHitFacts facts,
      OwnerCommitResult result)
  {
    return new TagEffectHitResult(
        result,
        facts.Entity,
        facts.Damage,
        facts.Critical,
        false,
        false,
        false,
        TagEffectTaggedAction.None,
        TagEffectProcAction.None);
  }

  private static TagEffectHitResult RejectedHit(
      TagEffectHitFacts facts,
      ProtocolErrorCode code,
      string detail)
  {
    return FailedHit(facts, OwnerCommitResult.Rejected(code, detail));
  }

  private sealed class PlayerTagEffectState
  {
    private readonly int[] _npcTimes;
    private readonly int[] _procTimes;
    private readonly CombatEntityIdentity?[] _npcIdentities;

    public PlayerTagEffectState(byte player, SessionEpoch sessionEpoch, int npcSlotCount)
    {
      Player = player;
      SessionEpoch = sessionEpoch;
      _npcTimes = new int[npcSlotCount];
      _procTimes = new int[npcSlotCount];
      _npcIdentities = new CombatEntityIdentity?[npcSlotCount];
    }

    public byte Player { get; }

    public SessionEpoch SessionEpoch { get; }

    public short EffectType { get; private set; }

    public long Revision { get; private set; }

    public CommitVersion CommitVersion { get; private set; } =
      MinimalImplementation.Contracts.CommitVersion.Initial;

    public bool IsDirty { get; private set; }

    public bool HasActiveEffect => EffectType != 0;

    public bool MatchesNpc(short slot, CombatEntityIdentity identity)
    {
      return slot >= 0 && slot < _npcIdentities.Length && _npcIdentities[slot] == identity;
    }

    public int GetNpcTime(short slot) => _npcTimes[slot];

    public int GetProcTime(short slot) => _procTimes[slot];

    public bool HasTickChanges(ICombatEntityIdentityAuthority identityAuthority)
    {
      for (int slot = 0; slot < _npcTimes.Length; slot++)
      {
        CombatEntityIdentity? identity = _npcIdentities[slot];
        if (identity is CombatEntityIdentity npcIdentity)
        {
          if (!identityAuthority.IsActive(npcIdentity) ||
              _npcTimes[slot] > 0 || _procTimes[slot] > 0)
          {
            return true;
          }
        }
        else if (_npcTimes[slot] > 0 || _procTimes[slot] > 0)
        {
          return true;
        }
      }

      return false;
    }

    public void AdvanceTimers(ICombatEntityIdentityAuthority identityAuthority)
    {
      for (int slot = 0; slot < _npcTimes.Length; slot++)
      {
        CombatEntityIdentity? identity = _npcIdentities[slot];
        if ((identity is CombatEntityIdentity npcIdentity &&
            !identityAuthority.IsActive(npcIdentity)) || identity is null)
        {
          _npcTimes[slot] = 0;
          _procTimes[slot] = 0;
          _npcIdentities[slot] = null;
          continue;
        }

        if (_npcTimes[slot] > 0)
        {
          _npcTimes[slot]--;
        }

        if (_procTimes[slot] > 0)
        {
          _procTimes[slot]--;
        }
      }
    }

    public void ChangeActiveEffect(short effectType)
    {
      EffectType = effectType;
      ClearAllNpcData();
    }

    public void ApplyTag(short slot, CombatEntityIdentity identity, int duration)
    {
      BindNpc(slot, identity);
      _npcTimes[slot] = duration;
    }

    public void EnableProc(short slot, CombatEntityIdentity identity, int duration)
    {
      BindNpc(slot, identity);
      _procTimes[slot] = duration;
    }

    public void ClearProc(short slot, CombatEntityIdentity identity)
    {
      BindNpc(slot, identity);
      _procTimes[slot] = 0;
    }

    public void ClearNpcSlot(short slot)
    {
      _npcTimes[slot] = 0;
      _procTimes[slot] = 0;
      _npcIdentities[slot] = null;
    }

    public void ClearAllNpcData()
    {
      Array.Clear(_npcTimes);
      Array.Clear(_procTimes);
      Array.Clear(_npcIdentities);
    }

    public void Touch(CommitVersion version)
    {
      Revision++;
      CommitVersion = version;
      IsDirty = true;
    }

    public void Acknowledge(long revision)
    {
      if (Revision == revision)
      {
        IsDirty = false;
      }
    }

    public TagEffectSnapshotView Capture(TagEffectDefinitionCatalog definitions)
    {
      bool hasDefinition = definitions.TryGet(EffectType, out TagEffectDefinition definition);
      return new TagEffectSnapshotView(
        Player,
        SessionEpoch,
        EffectType,
        hasDefinition && definition.NetSync,
        hasDefinition && definition.SyncProcs,
        _npcTimes,
        _procTimes,
        _npcIdentities,
        Revision,
        CommitVersion,
        IsDirty);
    }

    private void BindNpc(short slot, CombatEntityIdentity identity)
    {
      if (_npcIdentities[slot] != identity)
      {
        _npcTimes[slot] = 0;
        _procTimes[slot] = 0;
        _npcIdentities[slot] = identity;
      }
    }
  }
}
