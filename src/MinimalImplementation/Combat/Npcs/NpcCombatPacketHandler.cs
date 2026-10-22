using MinimalImplementation.Combat.Identity;
using MinimalImplementation.Combat.Projectiles;
using MinimalImplementation.Contracts;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Combat.Npcs;

/// <summary>Trusted player-side facts needed to resolve packet 24's melee request.</summary>
public sealed record NpcMeleeStrikeFacts(
    int Damage,
    float Knockback,
    int HitDirection,
    bool Critical,
    bool ExpertMode,
    bool NoGravity,
    bool ShadowFlame);

/// <summary>Result of one authenticated NPC damage command.</summary>
public sealed record NpcCombatDispatchResult(
    OwnerCommitResult Result,
    NpcView? Npc,
    NpcDamagePacketIntent? DamagePacket,
    bool Lethal,
    UnusedMeleeStrikePacket? Echo)
{
  public bool IsApplied => Result.Status == OwnerCommitStatus.Applied;
}

/// <summary>
/// Thin packet owner adapter for NPC damage. It resolves slot projections and sender identity;
/// all arithmetic and lifecycle writes remain in NpcManager.
/// </summary>
public sealed class NpcCombatPacketHandler
{
  private readonly NpcManager _npcs;

  public NpcCombatPacketHandler(NpcManager npcs)
  {
    ArgumentNullException.ThrowIfNull(npcs);
    _npcs = npcs;
  }

  public NpcCombatDispatchResult HandleMeleeStrike(
      InboundPacketEnvelope envelope,
      AuthenticatedCombatSender sender,
      UnusedMeleeStrikePacket packet,
      NpcMeleeStrikeFacts facts)
  {
    ArgumentNullException.ThrowIfNull(envelope);
    ArgumentNullException.ThrowIfNull(packet);
    ArgumentNullException.ThrowIfNull(facts);
    if (envelope.MessageId != 24 || envelope.Epoch != sender.Epoch ||
        packet.Player != sender.PlayerSlot)
    {
      return Rejected(ProtocolErrorCode.Forbidden,
          "Packet 24 must use the authenticated player slot and session epoch.");
    }

    if (!_npcs.TryGet(packet.NpcIndex, out NpcView? npc) || npc is null)
    {
      return Rejected(ProtocolErrorCode.StaleEntity,
          "Packet 24 targets an inactive NPC slot.");
    }

    NpcDamageRequest request = new(
        npc.Entity,
        npc.Revision,
        facts.Damage,
        facts.Knockback,
        facts.HitDirection,
        facts.Critical,
        facts.ExpertMode,
        facts.NoGravity,
        facts.ShadowFlame,
        sender.Epoch);
    return CommitDamage(request, echo: packet);
  }

  public NpcCombatDispatchResult HandleDamage(
      InboundPacketEnvelope envelope,
      AuthenticatedCombatSender sender,
      DamageNPCPacket packet)
  {
    ArgumentNullException.ThrowIfNull(envelope);
    ArgumentNullException.ThrowIfNull(packet);
    if (envelope.MessageId != 28 || envelope.Epoch != sender.Epoch)
    {
      return Rejected(ProtocolErrorCode.Forbidden,
          "Packet 28 must use the authenticated session epoch.");
    }

    if (packet.EncodedDirection > 2 ||
        packet.HitDirection > 1 || !float.IsFinite(packet.Knockback))
    {
      return Rejected(ProtocolErrorCode.InvalidLength,
          "Packet 28 direction, critical flag, or knockback is invalid.");
    }

    if (!_npcs.TryGet(packet.NpcIndex, out NpcView? npc) || npc is null)
    {
      return Rejected(ProtocolErrorCode.StaleEntity,
          "Packet 28 targets an inactive NPC slot.");
    }

    NpcDamageRequest request = new(
        npc.Entity,
        npc.Revision,
        packet.Damage,
        packet.Knockback,
        packet.EncodedDirection - 1,
        packet.HitDirection == 1,
        ExpertMode: false,
        NoGravity: false,
        ShadowFlame: false,
        sender.Epoch);
    return CommitDamage(request, echo: null);
  }

  private NpcCombatDispatchResult CommitDamage(
      NpcDamageRequest request,
      UnusedMeleeStrikePacket? echo)
  {
    NpcPreparation<NpcDamagePlan> preparation = _npcs.PrepareDamage(request);
    if (!preparation.IsPrepared || preparation.Plan is null)
    {
      return new NpcCombatDispatchResult(preparation.Result, null, null, false, echo);
    }

    OwnerCommitResult validation = _npcs.Validate(preparation.Plan);
    if (validation.Status != OwnerCommitStatus.Applied)
    {
      return new NpcCombatDispatchResult(validation, null, null, false, echo);
    }

    (OwnerCommitResult result, NpcView? committed) = _npcs.Commit(preparation.Plan);
    if (result.Status != OwnerCommitStatus.Applied || committed is null)
    {
      return new NpcCombatDispatchResult(result, null, null, false, echo);
    }

    NpcDamagePacketIntent? damagePacket = preparation.Plan.CreateDamagePacket(committed);
    return new NpcCombatDispatchResult(
        result,
        committed,
        damagePacket,
        preparation.Plan.IsLethal,
        echo);
  }

  private static NpcCombatDispatchResult Rejected(
      ProtocolErrorCode code,
      string detail)
  {
    return new NpcCombatDispatchResult(
        OwnerCommitResult.Rejected(code, detail), null, null, false, null);
  }
}
