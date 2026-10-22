using System.IO;
using MinimalImplementation.Combat.Identity;
using MinimalImplementation.Contracts;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace MinimalImplementation.Combat.Npcs;

/// <summary>Packet 28 for one committed hit; recipient selection belongs to G2.</summary>
public sealed class NpcDamagePacketIntent {
  internal NpcDamagePacketIntent(NpcDamageRequest request, NpcView npc) {
    Entity = npc.Entity;
    Slot = npc.Slot;
    Revision = npc.Revision;
    CommitVersion = npc.CommitVersion;
    InputDamage = checked((short)request.Damage);
    Knockback = request.Knockback;
    HitDirection = request.HitDirection;
    Critical = request.Critical;
    Lethal = npc.Life == 0;
  }

  public CombatEntityIdentity Entity { get; }
  public short Slot { get; }
  public long Revision { get; }
  public CommitVersion CommitVersion { get; }
  public short InputDamage { get; }
  public float Knockback { get; }
  public int HitDirection { get; }
  public bool Critical { get; }
  public bool Lethal { get; }

  public byte[] EncodeBody() {
    using var stream = new MemoryStream();
    var writer = new PacketWireWriter(stream);
    // The infrastructure codec calls the last byte HitDirection; source uses it for crit.
    DamageNPCPacket.Write(writer, Slot, InputDamage, Knockback,
        (byte)(HitDirection + 1), Critical ? (byte)1 : (byte)0);
    return stream.ToArray();
  }
}
