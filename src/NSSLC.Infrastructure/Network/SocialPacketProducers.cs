using System;
using System.Linq;

using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.Projectile;

namespace NSSLC.Infrastructure.Network;

public static class SocialPacketProducers {
  public static SmartTextMessagePacket CreateTeamChangeMessage(
      string playerName, byte team) {
    ArgumentNullException.ThrowIfNull(playerName);
    if (team > 5) {
      throw new ArgumentOutOfRangeException(nameof(team));
    }

    string localizationKey = team == 5
        ? "LegacyMultiplayer.22"
        : $"LegacyMultiplayer.{13 + team}";
    return CreateSmartTextMessage(
        TeamColor(team),
        NetworkText.Key(localizationKey, NetworkText.Literal(playerName)),
        widthLimit: -1);
  }

  public static SyncNPCPacket CreateSyncNpc(SocialNpcEffectSnapshot snapshot) {
    ArgumentNullException.ThrowIfNull(snapshot);
    if (snapshot.Slot.Value > short.MaxValue || snapshot.NetId > short.MaxValue
        || snapshot.CanBeCaught
        || snapshot.Shimmering && !snapshot.SpawnNeedsSyncing
        || !float.IsFinite(snapshot.Position.X) || !float.IsFinite(snapshot.Position.Y)
        || !float.IsFinite(snapshot.Velocity.X) || !float.IsFinite(snapshot.Velocity.Y)
        || snapshot.Ai.Any(static value => !float.IsFinite(value))) {
      throw new ArgumentOutOfRangeException(nameof(snapshot),
          "The committed NPC cannot be represented by the supported Social packet-23 projection.");
    }

    bool fullLife = snapshot.CurrentLife == snapshot.MaximumLife;
    return new SyncNPCPacket {
      NpcSlot = checked((short)snapshot.Slot.Value),
      PositionX = snapshot.Position.X,
      PositionY = snapshot.Position.Y,
      VelocityX = snapshot.Velocity.X,
      VelocityY = snapshot.Velocity.Y,
      Target = snapshot.Target < 0 ? byte.MaxValue : checked((ushort)snapshot.Target),
      DirectionPositive = snapshot.Direction > 0,
      DirectionYPositive = snapshot.DirectionY > 0,
      SpriteDirectionPositive = snapshot.SpriteDirection > 0,
      FullLife = fullLife,
      SpawnedFromStatue = snapshot.SpawnedFromStatue,
      SpawnNeedsSyncing = snapshot.SpawnNeedsSyncing || snapshot.NetworkUpdatePending,
      Shimmering = snapshot.Shimmering,
      Ai = snapshot.Ai.ToArray(),
      NetId = checked((short)snapshot.NetId),
      PlayerCount = null,
      Difficulty = null,
      Life = fullLife ? null : snapshot.CurrentLife,
      EncodedLifeWidth = fullLife ? null : GetNpcLifeWidth(snapshot.MaximumLife),
      CatchableReleaseOwner = null
    };
  }

  public static SteamProjectileSyncPacket CreateProjectileSync(
      SocialProjectileSpawnSnapshot snapshot) {
    ArgumentNullException.ThrowIfNull(snapshot);
    if (snapshot.ProjectileUuid.HasValue
        || snapshot.Identity > 1000
        || snapshot.Damage < short.MinValue || snapshot.Damage > short.MaxValue
        || snapshot.OriginalDamage < short.MinValue
        || snapshot.OriginalDamage > short.MaxValue) {
      throw new ArgumentOutOfRangeException(nameof(snapshot),
          "The committed projectile cannot be represented by the Steam packet-27 projection.");
    }

    var state = new ProjectileNetworkApplyCommand(
        snapshot.OwnerSlot,
        snapshot.Identity,
        snapshot.ProjectileType,
        snapshot.Position,
        snapshot.Velocity,
        snapshot.Damage,
        snapshot.OriginalDamage,
        snapshot.Knockback,
        snapshot.Ai0,
        snapshot.Ai1,
        snapshot.Ai2,
        projectileUuid: -1,
        bannerIdToRespondTo: snapshot.BannerIdToRespondTo);
    return new SteamProjectileSyncPacket(snapshot.SteamKey, state);
  }

  public static SmartTextMessagePacket CreateSmartTextMessage(PacketRgb color,
      NetworkText text, short widthLimit) {
    ArgumentNullException.ThrowIfNull(text);
    return new SmartTextMessagePacket {
      Color = color,
      Text = text,
      WidthLimit = widthLimit
    };
  }

  private static PacketRgb TeamColor(byte team) {
    return team switch {
      1 => new PacketRgb(255, 0, 0),
      2 => new PacketRgb(0, 255, 0),
      3 => new PacketRgb(0, 128, 255),
      4 => new PacketRgb(255, 255, 0),
      5 => new PacketRgb(255, 0, 255),
      _ => new PacketRgb(255, 255, 255)
    };
  }

  public static PlayLegacySoundPacket CreateLegacySound(PacketVector2 position,
      ushort soundIndex, int? style = null, float? volume = null, float? pitchOffset = null) {
    if (!float.IsFinite(position.X) || !float.IsFinite(position.Y)) {
      throw new ArgumentOutOfRangeException(nameof(position));
    }
    if (volume.HasValue && !float.IsFinite(volume.Value)) {
      throw new ArgumentOutOfRangeException(nameof(volume));
    }
    if (pitchOffset.HasValue && !float.IsFinite(pitchOffset.Value)) {
      throw new ArgumentOutOfRangeException(nameof(pitchOffset));
    }

    return new PlayLegacySoundPacket {
      Position = position,
      SoundIndex = soundIndex,
      Style = style,
      Volume = volume,
      PitchOffset = pitchOffset
    };
  }

  public static SyncEmoteBubblePacket CreateEmoteBubbleRemoval(int bubbleId) {
    return new SyncEmoteBubblePacket {
      BubbleId = bubbleId,
      AnchorKind = byte.MaxValue
    };
  }

  private static byte GetNpcLifeWidth(int maximumLife) {
    return maximumLife > short.MaxValue ? (byte)4
        : maximumLife > sbyte.MaxValue ? (byte)2 : (byte)1;
  }
}
