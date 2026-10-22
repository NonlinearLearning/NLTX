using System;
using System.IO;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace MinimalImplementation.Combat.Projectiles;

/// <summary>Builds packet bodies with the Infrastructure Pocket codecs; framing belongs to P01.</summary>
public static class ProjectilePacketPayloadCodec
{
  public static byte[] EncodeSyncBody(ProjectileView projectile, ProjectileDefinitionCatalog definitions)
  {
    ArgumentNullException.ThrowIfNull(projectile);
    ArgumentNullException.ThrowIfNull(definitions);
    if (!definitions.TryGet(projectile.Type, out ProjectileDefinition definition))
    {
      throw new InvalidOperationException(
        $"Projectile type {projectile.Type} has no migrated Version4 content definition.");
    }

    if (definition.NeedsProtocolUuid != projectile.NeedsProtocolUuid ||
        definition.NeedsProtocolUuid != projectile.ProtocolUuid.HasValue)
    {
      throw new InvalidOperationException(
        "The projectile snapshot UUID state does not match its migrated content definition.");
    }

    using var stream = new MemoryStream();
    var writer = new PacketWireWriter(stream);
    SyncProjectilePacket.Write(
      writer,
      projectile.ProtocolIdentity,
      new PacketVector2(projectile.Position.X, projectile.Position.Y),
      new PacketVector2(projectile.Velocity.X, projectile.Velocity.Y),
      projectile.Owner,
      projectile.Type,
      projectile.Ai0,
      projectile.Ai1,
      projectile.BannerId == 0 ? null : projectile.BannerId,
      projectile.Damage == 0 ? null : checked((short)projectile.Damage),
      projectile.Knockback == 0f ? null : projectile.Knockback,
      projectile.OriginalDamage == 0 ? null : checked((short)projectile.OriginalDamage),
      projectile.Ai2);
    return stream.ToArray();
  }

  public static byte[] EncodeKillBody(short identity, byte owner)
  {
    return EncodeKillBody(identity, owner, new PacketVector2(0f, 0f));
  }

  public static byte[] EncodeKillBody(ProjectileView projectile)
  {
    ArgumentNullException.ThrowIfNull(projectile);
    return EncodeKillBody(
      projectile.ProtocolIdentity,
      projectile.Owner,
      new PacketVector2(projectile.Position.X, projectile.Position.Y));
  }

  public static byte[] EncodeKillBody(short identity, byte owner, PacketVector2 position)
  {
    using var stream = new MemoryStream();
    var writer = new PacketWireWriter(stream);
    KillProjectilePacket.Write(writer, identity, owner, position);
    return stream.ToArray();
  }
}
