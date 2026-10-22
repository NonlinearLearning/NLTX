using System;
using System.Collections.Generic;
using System.Linq;
using MinimalImplementation.Server.Session;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Player;

/// <summary>Pure packet projections built from a captured player snapshot.</summary>
public sealed class PlayerPacketProjection
{
  public PlayerPacketProjection(byte messageId, object packet)
  {
    ArgumentNullException.ThrowIfNull(packet);
    MessageId = messageId;
    Packet = packet;
  }

  public byte MessageId { get; }

  public object Packet { get; }

  public static IReadOnlyList<PlayerPacketProjection> From(
    PlayerJoinSnapshot snapshot,
    SyncProjectileTrackersPacket? projectileTrackers = null)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    if (projectileTrackers is not null && projectileTrackers.Player != snapshot.PlayerSlot)
    {
      throw new ArgumentException(
        "Projectile trackers must belong to the captured player.", nameof(projectileTrackers));
    }

    var projections = new List<PlayerPacketProjection>
    {
      new(
        SessionAdmissionCoordinator.PlayerActiveMessageId,
        new PlayerActivePacket
        {
          Player = snapshot.PlayerSlot,
          ActiveState = 1
        }),
      new(
        SessionAdmissionCoordinator.SyncPlayerMessageId,
        CreateAppearancePacket(snapshot)),
      new(
        SessionAdmissionCoordinator.PlayerControlsMessageId,
        CreateControlsPacket(snapshot))
    };

    if (snapshot.Vitals.IsDead)
    {
      projections.Add(new PlayerPacketProjection(
        SessionAdmissionCoordinator.DeadPlayerMessageId,
        new DeadPlayerPacket { Player = snapshot.PlayerSlot }));
    }

    projections.AddRange([
      new(
        SessionAdmissionCoordinator.PlayerLifeManaMessageId,
        new PlayerLifeManaPacket
        {
          Player = snapshot.PlayerSlot,
          Life = snapshot.Vitals.Life,
          MaximumLife = snapshot.Vitals.MaximumLife
        }),
      new(
        SessionAdmissionCoordinator.TogglePvpMessageId,
        new TogglePVPPacket
        {
          Player = snapshot.PlayerSlot,
          Hostile = snapshot.PvpStatus.Hostile
        }),
      new(
        SessionAdmissionCoordinator.TeamChangeMessageId,
        new TeamChangePacket
        {
          Player = snapshot.PlayerSlot,
          Team = snapshot.Team.Current
        }),
      new(
        SessionAdmissionCoordinator.Unknown42MessageId,
        new Unknown42Packet
        {
          Player = snapshot.PlayerSlot,
          Mana = snapshot.Vitals.Mana,
          MaximumMana = snapshot.Vitals.MaximumMana
        }),
      new(
        SessionAdmissionCoordinator.PlayerBuffsMessageId,
        new PlayerBuffsPacket
        {
          Player = snapshot.PlayerSlot,
          BuffTypes = snapshot.BuffTypes.Where(type => type != 0).ToArray()
        })
    ]);

    if (projectileTrackers is not null)
    {
      projections.Add(new PlayerPacketProjection(
        SessionAdmissionCoordinator.SyncProjectileTrackersMessageId, projectileTrackers));
    }

    projections.AddRange([
      new(
        SessionAdmissionCoordinator.SyncLoadoutMessageId,
        new SyncLoadoutPacket
        {
          Player = snapshot.PlayerSlot,
          LoadoutIndex = snapshot.LoadoutIndex,
          AccessoryVisibilityMask = snapshot.AccessoryVisibilityMask
        }),
      new(
        SessionAdmissionCoordinator.SyncPlayerZoneMessageId,
        new SyncPlayerZonePacket
        {
          Player = snapshot.PlayerSlot,
          Zone1 = snapshot.Zones.Zone1,
          Zone2 = snapshot.Zones.Zone2,
          Zone3 = snapshot.Zones.Zone3,
          Zone4 = snapshot.Zones.Zone4,
          Zone5 = snapshot.Zones.Zone5,
          TownNpcCount = snapshot.Zones.TownNpcCount
        }),
      new(
        SessionAdmissionCoordinator.PlayerStealthMessageId,
        new PlayerStealthPacket
        {
          Player = snapshot.PlayerSlot,
          Stealth = snapshot.Stealth
        }),
      new(
        SessionAdmissionCoordinator.UpdatePlayerLuckFactorsMessageId,
        CreateLuckPacket(snapshot.PlayerSlot, snapshot.Luck))
    ]);

    return projections.AsReadOnly();
  }

  private static SyncPlayerPacket CreateAppearancePacket(PlayerJoinSnapshot snapshot)
  {
    PlayerAppearance appearance = snapshot.Appearance;
    return new SyncPlayerPacket
    {
      Player = snapshot.PlayerSlot,
      SkinVariant = appearance.SkinVariant,
      VoiceVariant = appearance.VoiceVariant,
      VoicePitchOffset = appearance.VoicePitchOffset,
      Hair = appearance.Hair,
      Name = appearance.Name,
      HairDye = appearance.HairDye,
      HiddenAccessories = appearance.HiddenAccessories,
      HideMisc = appearance.HideMisc,
      HairColor = appearance.HairColor,
      SkinColor = appearance.SkinColor,
      EyeColor = appearance.EyeColor,
      ShirtColor = appearance.ShirtColor,
      UnderShirtColor = appearance.UnderShirtColor,
      PantsColor = appearance.PantsColor,
      ShoeColor = appearance.ShoeColor,
      DifficultyAndAccessoryFlags = appearance.DifficultyAndAccessoryFlags,
      BiomeAndCartFlags = appearance.BiomeAndCartFlags,
      PermanentUpgradeFlags = appearance.PermanentUpgradeFlags
    };
  }

  private static PlayerControlsPacket CreateControlsPacket(PlayerJoinSnapshot snapshot)
  {
    PlayerMovementInput movement = snapshot.Movement;
    return new PlayerControlsPacket
    {
      Player = snapshot.PlayerSlot,
      ControlFlags = movement.ControlFlags,
      MovementFlags = movement.MovementFlags,
      PlayerFeatureFlags = movement.PlayerFeatureFlags,
      ActionFlags = movement.ActionFlags,
      SelectedItem = movement.SelectedItem,
      Position = movement.Position,
      Velocity = movement.VelocityIncluded ? movement.Velocity : null,
      MountType = movement.MountType,
      PotionOfReturnUsePosition = movement.PotionOfReturnUsePosition,
      PotionOfReturnHomePosition = movement.PotionOfReturnHomePosition,
      CameraTarget = movement.CameraTarget
    };
  }

  public static UpdatePlayerLuckFactorsPacket CreateLuckPacket(byte playerSlot, PlayerLuck luck)
  {
    return new UpdatePlayerLuckFactorsPacket
    {
      Player = playerSlot,
      LadyBugLuckTime = luck.LadyBugLuckTime,
      TorchLuck = luck.TorchLuck,
      LuckPotion = luck.LuckPotion,
      HasGardenGnome = luck.HasGardenGnome,
      BrokenMirrorBadLuck = luck.BrokenMirrorBadLuck,
      EquipmentLuckBonus = luck.EquipmentLuckBonus,
      CoinLuck = luck.CoinLuck,
      KiteLuckLevel = luck.KiteLuckLevel
    };
  }
}
