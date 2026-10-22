using MinimalImplementation.Combat.Identity;
using MinimalImplementation.Combat.TagEffects;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Mechanisms;
using MinimalImplementation.Server.Modules.Protocol;
using MinimalImplementation.Server.Protocol;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Host;

public enum TeleportPylonProjectionAction : byte
{
  Added = 0,
  Removed = 1
}

/// <summary>Projects committed P06 pylon facts as server-authored module 7 intents.</summary>
public sealed class TeleportPylonModuleProducer
{
  private readonly Packet82ModuleIntentProducer _producer;
  private readonly IMechanismIdentityAuthority _identities;

  public TeleportPylonModuleProducer(
    Packet82ModuleIntentProducer producer,
    IMechanismIdentityAuthority identities)
  {
    ArgumentNullException.ThrowIfNull(producer);
    ArgumentNullException.ThrowIfNull(identities);
    _producer = producer;
    _identities = identities;
  }

  public OutboundPacketIntent? CreateChangeIntent(
    TeleportPylonSnapshot pylon,
    TeleportPylonProjectionAction action,
    IReadOnlyCollection<SessionEpoch> recipients,
    CommitVersion commitVersion)
  {
    ArgumentNullException.ThrowIfNull(pylon);
    ArgumentNullException.ThrowIfNull(recipients);
    if (action is not TeleportPylonProjectionAction.Added and
        not TeleportPylonProjectionAction.Removed)
    {
      throw new ArgumentOutOfRangeException(nameof(action));
    }
    ValidatePylon(pylon, requireActiveIdentity: action == TeleportPylonProjectionAction.Added);

    if (recipients.Count == 0)
    {
      return null;
    }

    DelegatedModuleIntentValidation.ValidateTargets(recipients, commitVersion);
    var payload = new Packet82TeleportPylonData(
      (byte)action,
      checked((short)pylon.Anchor.X),
      checked((short)pylon.Anchor.Y),
      pylon.Type);
    return _producer.CreateIntent(
      (ushort)Packet82ModuleId.TeleportPylon,
      payload,
      recipients,
      commitVersion);
  }

  public IReadOnlyList<OutboundPacketIntent> CreateJoinIntents(
    IReadOnlyCollection<TeleportPylonSnapshot> pylons,
    long worldScope,
    SessionEpoch joiningEpoch,
    CommitVersion commitVersion)
  {
    ArgumentNullException.ThrowIfNull(pylons);
    if (pylons.Count == 0)
    {
      return Array.Empty<OutboundPacketIntent>();
    }

    SessionEpoch[] target = [joiningEpoch];
    DelegatedModuleIntentValidation.ValidateTargets(target, commitVersion);
    var intents = new List<OutboundPacketIntent>(pylons.Count);
    foreach (TeleportPylonSnapshot pylon in pylons.OrderBy(value => value.Ordinal))
    {
      ValidatePylon(pylon, requireActiveIdentity: true, expectedWorldScope: worldScope);
      OutboundPacketIntent? intent = CreateChangeIntent(
        pylon,
        TeleportPylonProjectionAction.Added,
        target,
        commitVersion);
      if (intent is not null)
      {
        intents.Add(intent);
      }
    }

    return Array.AsReadOnly(intents.ToArray());
  }

  private void ValidatePylon(
    TeleportPylonSnapshot pylon,
    bool requireActiveIdentity,
    long? expectedWorldScope = null)
  {
    if (!pylon.Identity.IsValid || pylon.Identity.WorldScope <= 0 ||
        pylon.Ordinal <= 0 || pylon.Anchor.X < 0 || pylon.Anchor.Y < 0 ||
        pylon.Anchor.X > short.MaxValue || pylon.Anchor.Y > short.MaxValue ||
        expectedWorldScope is long scope && pylon.Identity.WorldScope != scope)
    {
      throw new InvalidOperationException(
        "A pylon projection must use a valid committed identity, scope, ordinal, and wire position.");
    }

    if (requireActiveIdentity && !_identities.IsActive(pylon.Identity))
    {
      throw new InvalidOperationException("A pylon projection cannot use a retired P06 identity.");
    }
  }
}

/// <summary>
/// Projects committed P06 leashed-entity facts without owning entity state or transport.
/// </summary>
public sealed class LeashedEntityModuleProducer
{
  private readonly Packet82ModuleIntentProducer _producer;
  private readonly IMechanismIdentityAuthority _identities;

  public LeashedEntityModuleProducer(
    Packet82ModuleIntentProducer producer,
    IMechanismIdentityAuthority identities)
  {
    ArgumentNullException.ThrowIfNull(producer);
    ArgumentNullException.ThrowIfNull(identities);
    _producer = producer;
    _identities = identities;
  }

  public OutboundPacketIntent? CreateChangeIntent(
    LeashedEntityChangeResult change,
    long worldScope,
    IReadOnlyCollection<SessionEpoch> recipients,
    CommitVersion commitVersion)
  {
    ArgumentNullException.ThrowIfNull(change);
    ArgumentNullException.ThrowIfNull(recipients);
    if (change.Kind == LeashedEntityChangeKind.NoOp || recipients.Count == 0)
    {
      return null;
    }

    DelegatedModuleIntentValidation.ValidateTargets(recipients, commitVersion);
    Version4ModulePayloads.LeashedEntityMessage payload = change.Kind switch
    {
      LeashedEntityChangeKind.Created when change.Current is not null && change.Previous is null =>
        CreateFullMessage(change.Current, worldScope),
      LeashedEntityChangeKind.Updated when
        change.Current is not null && change.Previous is not null =>
        CreatePartialMessage(change.Current, change.Previous, worldScope),
      LeashedEntityChangeKind.Removed when change.Current is null && change.Previous is not null =>
        CreateRemoveMessage(change.Previous, worldScope),
      _ => throw new InvalidOperationException(
        "The P06 leashed change result does not match its committed lifecycle kind.")
    };

    return _producer.CreateIntent(
      (ushort)Packet82ModuleId.LeashedEntity,
      payload,
      recipients,
      commitVersion);
  }

  public IReadOnlyList<OutboundPacketIntent> CreateJoinIntents(
    IReadOnlyCollection<LeashedEntityRuntimeSnapshot> entities,
    long worldScope,
    SessionEpoch joiningEpoch,
    CommitVersion commitVersion)
  {
    ArgumentNullException.ThrowIfNull(entities);
    if (entities.Count == 0)
    {
      return Array.Empty<OutboundPacketIntent>();
    }

    SessionEpoch[] target = [joiningEpoch];
    DelegatedModuleIntentValidation.ValidateTargets(target, commitVersion);
    var intents = new List<OutboundPacketIntent>(entities.Count);
    foreach (LeashedEntityRuntimeSnapshot entity in entities.OrderBy(value => value.Slot))
    {
      Version4ModulePayloads.LeashedEntityMessage payload = CreateFullMessage(entity, worldScope);
      intents.Add(_producer.CreateIntent(
        (ushort)Packet82ModuleId.LeashedEntity,
        payload,
        target,
        commitVersion));
    }

    return Array.AsReadOnly(intents.ToArray());
  }

  private Version4ModulePayloads.LeashedEntityMessage CreateFullMessage(
    LeashedEntityRuntimeSnapshot entity,
    long worldScope)
  {
    ValidateEntity(entity, worldScope, requireActiveIdentity: true);
    Version4ModulePayloads.LeashedEntityState state = entity.State switch
    {
      Version4ModulePayloads.KiteLeashedEntityState kite when kite.ProjectileType is int => kite,
      Version4ModulePayloads.CritterLeashedEntityState critter
        when critter.NpcType is int && critter.SizeX is float && critter.SizeY is float => critter,
      _ => throw new InvalidOperationException(
        "A full leashed projection requires complete subtype state.")
    };
    return new Version4ModulePayloads.LeashedEntityMessage(
      1,
      entity.Slot,
      entity.Type,
      checked((short)entity.Anchor.X),
      checked((short)entity.Anchor.Y),
      state);
  }

  private Version4ModulePayloads.LeashedEntityMessage CreatePartialMessage(
    LeashedEntityRuntimeSnapshot current,
    LeashedEntityRuntimeSnapshot previous,
    long worldScope)
  {
    ValidateEntity(current, worldScope, requireActiveIdentity: true);
    ValidateEntity(previous, worldScope, requireActiveIdentity: false);
    if (current.Handle != previous.Handle || current.Slot != previous.Slot ||
        current.Type != previous.Type || current.Anchor != previous.Anchor ||
        current.Section != previous.Section)
    {
      throw new InvalidOperationException(
        "A partial leashed projection cannot change entity identity, slot, anchor, or section.");
    }

    Version4ModulePayloads.LeashedEntityState state = current.State switch
    {
      Version4ModulePayloads.KiteLeashedEntityState kite => kite with { ProjectileType = null },
      Version4ModulePayloads.CritterLeashedEntityState critter => critter with
      {
        NpcType = null,
        SizeX = null,
        SizeY = null,
        ButterflyVariant = null,
        OldPositionsLength = null
      },
      _ => throw new InvalidOperationException("The leashed entity has an unknown subtype state.")
    };
    return new Version4ModulePayloads.LeashedEntityMessage(
      2,
      current.Slot,
      current.Type,
      null,
      null,
      state);
  }

  private Version4ModulePayloads.LeashedEntityMessage CreateRemoveMessage(
    LeashedEntityRuntimeSnapshot previous,
    long worldScope)
  {
    ValidateEntity(previous, worldScope, requireActiveIdentity: false);
    return new Version4ModulePayloads.LeashedEntityMessage(
      0,
      previous.Slot,
      null,
      null,
      null,
      null);
  }

  private void ValidateEntity(
    LeashedEntityRuntimeSnapshot entity,
    long worldScope,
    bool requireActiveIdentity)
  {
    ArgumentNullException.ThrowIfNull(entity);
    if (worldScope <= 0 || !entity.Handle.IsValid || entity.Handle.WorldScope != worldScope ||
        entity.Revision <= 0 || entity.Slot < 0 ||
        entity.Slot >= LeashedEntityRegistry.DefaultMaximumSlots ||
        entity.Type is < 1 or > 19 || entity.Anchor.X < 0 || entity.Anchor.Y < 0 ||
        entity.Anchor.X > short.MaxValue || entity.Anchor.Y > short.MaxValue ||
        entity.Section.X < 0 || entity.Section.Y < 0)
    {
      throw new InvalidOperationException(
        "A leashed projection must use a valid committed P06 identity and wire-compatible bounds.");
    }

    if (requireActiveIdentity && !_identities.IsActive(entity.Handle))
    {
      throw new InvalidOperationException(
        "A leashed projection cannot use a retired P06 identity.");
    }
  }
}

/// <summary>
/// Projects committed P05 TagEffect full-state snapshots to module 12 recipients.
/// </summary>
public sealed class TagEffectModuleProducer
{
  private readonly Packet82ModuleIntentProducer _producer;
  private readonly ICombatEntityIdentityAuthority _identities;

  public TagEffectModuleProducer(
    Packet82ModuleIntentProducer producer,
    ICombatEntityIdentityAuthority identities)
  {
    ArgumentNullException.ThrowIfNull(producer);
    ArgumentNullException.ThrowIfNull(identities);
    _producer = producer;
    _identities = identities;
  }

  public OutboundPacketIntent? CreateFullStateIntent(
    TagEffectSnapshotView snapshot,
    IReadOnlyCollection<SessionEpoch> recipients)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(recipients);
    if (!snapshot.NetSync || recipients.Count == 0)
    {
      return null;
    }

    NetModulesPacket? packet = CreateFullStatePacket(snapshot);
    if (packet is null)
    {
      return null;
    }

    DelegatedModuleIntentValidation.ValidateTargets(recipients, snapshot.CommitVersion);
    return _producer.CreateIntent(
      packet.ModuleId,
      packet.Data!,
      recipients,
      snapshot.CommitVersion);
  }

  public NetModulesPacket? CreateFullStatePacket(TagEffectSnapshotView snapshot)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    if (!snapshot.NetSync)
    {
      return null;
    }

    ValidateSnapshot(snapshot);
    var payload = new Version4ModulePayloads.TagEffectMessage(
      snapshot.Player,
      0,
      snapshot.EffectType,
      snapshot.NpcTimes,
      snapshot.SyncProcs ? snapshot.ProcTimes : null,
      null);
    return new NetModulesPacket
    {
      ModuleId = (ushort)Packet82ModuleId.TagEffectState,
      Data = payload
    };
  }

  private void ValidateSnapshot(TagEffectSnapshotView snapshot)
  {
    if (!snapshot.SessionEpoch.IsValid || !snapshot.CommitVersion.IsValid ||
        snapshot.EffectType <= 0 || snapshot.NpcTimes.Count != snapshot.ProcTimes.Count ||
        snapshot.NpcTimes.Count != snapshot.NpcIdentities.Count ||
        snapshot.SyncProcs && snapshot.ProcTimes.Count > byte.MaxValue ||
        snapshot.NpcTimes.Count > byte.MaxValue)
    {
      throw new InvalidOperationException("The TagEffect snapshot is incomplete or not committed.");
    }

    long worldScope = _identities.CurrentWorldScope;
    if (worldScope <= 0)
    {
      throw new InvalidOperationException("TagEffect output requires an active P05 world scope.");
    }

    for (int index = 0; index < snapshot.NpcTimes.Count; index++)
    {
      int npcTime = snapshot.NpcTimes[index];
      int procTime = snapshot.ProcTimes[index];
      if (npcTime < 0 || procTime < 0)
      {
        throw new InvalidOperationException("TagEffect timer values cannot be negative.");
      }

      if (npcTime == 0 && (!snapshot.SyncProcs || procTime == 0))
      {
        continue;
      }

      if (snapshot.NpcIdentities[index] is not CombatEntityIdentity identity ||
          !identity.IsValid || identity.WorldScope != worldScope || !_identities.IsActive(identity))
      {
        throw new InvalidOperationException(
          "TagEffect output cannot include timers for a stale or unresolved NPC generation.");
      }
    }
  }
}

internal static class DelegatedModuleIntentValidation
{
  internal static void ValidateTargets(
    IReadOnlyCollection<SessionEpoch> recipients,
    CommitVersion commitVersion)
  {
    ArgumentNullException.ThrowIfNull(recipients);
    if (recipients.Count == 0)
    {
      throw new ArgumentException(
        "A delegated module intent must have a target.",
        nameof(recipients));
    }

    if (!commitVersion.IsValid)
    {
      throw new ArgumentOutOfRangeException(nameof(commitVersion));
    }

    foreach (SessionEpoch recipient in recipients)
    {
      if (!recipient.IsValid)
      {
        throw new ArgumentException(
          "Every delegated target must have a valid session epoch.",
          nameof(recipients));
      }
    }
  }
}
