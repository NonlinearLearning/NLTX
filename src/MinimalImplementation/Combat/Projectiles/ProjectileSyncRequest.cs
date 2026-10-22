using System;
using System.Numerics;
using MinimalImplementation.Contracts;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Combat.Projectiles;

/// <summary>Detached packet-27 input plus the P02-authenticated sender projection.</summary>
public sealed class ProjectileSyncRequest
{
  public ProjectileSyncRequest(
    InboundPacketEnvelope envelope,
    AuthenticatedCombatSender sender,
    SyncProjectilePacket packet)
  {
    ArgumentNullException.ThrowIfNull(envelope);
    ArgumentNullException.ThrowIfNull(packet);
    Envelope = envelope;
    Sender = sender;
    Identity = packet.Identity;
    Position = new Vector2(packet.Position.X, packet.Position.Y);
    Velocity = new Vector2(packet.Velocity.X, packet.Velocity.Y);
    ClaimedOwner = packet.Owner;
    Type = packet.ProjectileType;
    Ai0 = packet.Ai0;
    Ai1 = packet.Ai1;
    Ai2 = packet.Ai2;
    BannerId = packet.BannerId;
    Damage = packet.Damage;
    Knockback = packet.Knockback;
    OriginalDamage = packet.OriginalDamage;
    ProtocolUuid = packet.ProjectileUuid;
  }

  public InboundPacketEnvelope Envelope { get; }

  public AuthenticatedCombatSender Sender { get; }

  public short Identity { get; }

  public Vector2 Position { get; }

  public Vector2 Velocity { get; }

  public byte ClaimedOwner { get; }

  public short Type { get; }

  public float Ai0 { get; }

  public float Ai1 { get; }

  public float Ai2 { get; }

  public ushort? BannerId { get; }

  public short? Damage { get; }

  public float? Knockback { get; }

  public short? OriginalDamage { get; }

  public short? ProtocolUuid { get; }
}
