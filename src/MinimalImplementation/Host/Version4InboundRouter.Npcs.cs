using MinimalImplementation.Combat.Npcs;
using MinimalImplementation.Combat.Projectiles;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Session;
using MinimalImplementation.Server.World;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Host;

public sealed partial class Version4InboundRouter {
  private string? HandleNpcDamage(
      InboundPacketEnvelope envelope, DamageNPCPacket packet,
      List<OutboundPacketIntent> intents, List<IOutboundDeliverySettlement> settlements) {
    OwnerCommitResult result = _composition.SessionAdmission.ExecuteAuthenticatedPlayerPacket(
        envelope, 28, [SessionStage.Active], binding => {
          WorldHeaderCapture world = _composition.WorldOwner.CaptureWorldHeader();
          // New clients keep a FIFO of predicted strikes, including stale-generation strikes.
          // Acknowledge each authenticated request before sending any corrected life snapshot.
          intents.Add(new OutboundPacketIntent([envelope.Epoch], null, 162,
              ReadOnlyMemory<byte>.Empty, world.Version));
          ushort packed = unchecked((ushort)packet.NpcIndex);
          short slot = (byte)packed;
          byte generation = (byte)(packed >> 8);
          if (!_composition.NpcManager.TryGet(slot, out NpcView? npc) || npc is null) {
            return OwnerCommitResult.Applied(world.Version);
          }
          if (!_composition.NpcNetwork.Matches(npc, generation) || npc.Life == 0 ||
              !_composition.Players.TryGetForBinding(binding, out var player) ||
              player is null || player.IsDead) {
            intents.Add(_composition.NpcNetwork.CreateSync(npc, [envelope.Epoch]));
            return OwnerCommitResult.Applied(world.Version);
          }
          var section = new WorldSectionCoordinate(
              (int)(npc.Position.X / 16) / WorldGeometry.SectionWidth,
              (int)(npc.Position.Y / 16) / WorldGeometry.SectionHeight);
          if (_worldSectionRequests?.HasQueuedSection(envelope.Epoch, world.WorldEpoch,
              section) != true) {
            return OwnerCommitResult.Rejected(ProtocolErrorCode.Forbidden,
                "NPC damage targets a section not delivered to the authenticated player.");
          }
          // Packet 28 reports client strike input, not weapon/projectile provenance. Preserve
          // vanilla's report model; section authentication is not full weapon validation.
          if (packet.Damage <= 0) {
            intents.Add(_composition.NpcNetwork.CreateSync(npc, [envelope.Epoch]));
            return OwnerCommitResult.Applied(world.Version);
          }
          var normalized = new DamageNPCPacket {
            NpcIndex = slot,
            Damage = packet.Damage,
            Knockback = packet.Knockback,
            EncodedDirection = packet.EncodedDirection,
            HitDirection = packet.HitDirection
          };
          NpcCombatDispatchResult hit = _composition.NpcCombat.HandleDamage(envelope,
              new AuthenticatedCombatSender(envelope.Epoch, binding.PlayerSlot), normalized);
          if (!hit.IsApplied || hit.Npc is null) {
            intents.Add(_composition.NpcNetwork.CreateSync(npc, [envelope.Epoch]));
            return hit.Result;
          }
          IReadOnlyList<SessionEpoch> targets =
              _composition.SessionAdmission.CaptureActiveSessionEpochs();
          SessionEpoch[] observers = targets.Where(target => target != envelope.Epoch).ToArray();
          if (observers.Length > 0) {
            short packedSlot = unchecked((short)((generation << 8) | (byte)slot));
            var observedHit = new DamageNPCPacket {
              NpcIndex = packedSlot,
              Damage = packet.Damage,
              Knockback = packet.Knockback,
              EncodedDirection = packet.EncodedDirection,
              HitDirection = packet.HitDirection
            };
            intents.Add(_composition.Version4WireProfile.CreateOutboundIntent(
                observedHit, observers, null, hit.Npc.CommitVersion));
          }
          OutboundPacketIntent sync = _composition.NpcNetwork.CreateSync(hit.Npc, targets);
          intents.Add(sync);
          settlements.Add(new NpcSnapshotDeliverySettlement(
              _composition.NpcManager, [hit.Npc], [sync], () => { }));
          ServerLog.Info("server.npc.damage",
              $"epoch={envelope.Epoch.Value} sequence={envelope.Sequence.Value} " +
              $"slot={slot} generation={generation} input={packet.Damage} " +
              $"critical={packet.HitDirection == 1} before={npc.Life} " +
              $"applied={npc.Life - hit.Npc.Life} life={hit.Npc.Life}/{hit.Npc.MaximumLife} " +
              $"lethal={hit.Lethal}");
          return hit.Result;
        });
    return result.Status == OwnerCommitStatus.Applied ? null : result.Detail;
  }
}
