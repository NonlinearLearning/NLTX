using System.Numerics;

namespace Terraria.Player;

/// <summary>
/// Authoritative player state that is only represented by network-facing player packets.
/// The entity identity, lifecycle, inventory, vitality, buffs, environment and progression
/// components remain the owners of their respective state; this component holds the remaining
/// player-owned network facts so packet DTOs never become a second store.
/// </summary>
/// <remarks>
/// <para>职责：保存玩家网络同步使用的外观、控制、战斗和交互投影。</para>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：skinVariant（第 904 行）； voiceVariant（第 906 行）； voicePitchOffset（第 908 行）； itemRotation（第
/// 1023 行）； stealth（第 1077 行）； PotionOfReturnHomePosition（第 1901 行）； hostile（第 1968 行）；
/// MinionRestTargetPoint（第 2386 行）； MinionAttackTargetNPC（第 2388 行）； itemAnimation（第 2393 行）；
/// ShouldNotDraw（第 3094 行）； talkNPC（第 3110 行）。
/// </para>
/// </remarks>
public sealed class PlayerNetworkStateComponent
{
  // Packet 4 facts that do not have a separate domain component yet. These are
  // authoritative entity state, not a copy of the packet DTO.
  public byte SkinVariant { get; internal set; }

  public byte VoiceVariant { get; internal set; }

  public float VoicePitchOffset { get; internal set; }

  public ushort HiddenAccessories { get; internal set; }

  public byte HideMiscBits { get; internal set; }

  public byte DifficultyAndAccessoryFlags { get; internal set; }

  public byte BiomeAndCartFlags { get; internal set; }

  public byte PermanentUpgradeFlags { get; internal set; }

  // Packet 13 input facts that do not yet have dedicated simulation systems. These
  // remain formal player-owned state so the packet DTO is never the authority.
  public byte ControlFlags { get; internal set; }

  public byte MovementFlags { get; internal set; }

  public byte PlayerFeatureFlags { get; internal set; }

  public byte ActionFlags { get; internal set; }

  public bool Hostile { get; internal set; }

  public float Stealth { get; internal set; } = 1.0f;

  public bool ShouldNotDraw { get; internal set; }

  public int TalkNpc { get; internal set; } = -1;

  public float ItemRotation { get; internal set; }

  public int ItemAnimation { get; internal set; }

  public byte ItemAnimationChannel { get; internal set; }

  public Vector2 MinionRestTargetPoint { get; internal set; }

  public int MinionAttackTargetNpc { get; internal set; } = -1;

  public byte TownNpcCount { get; internal set; }

  public bool IsPotionOfReturnInUse { get; internal set; }

  public Vector2? PotionOfReturnPosition { get; internal set; }

  public Vector2? PotionOfReturnHomePosition { get; internal set; }

  public bool HasPendingTeleport { get; internal set; }

  public Vector2? PendingTeleportPosition { get; internal set; }

  public IReadOnlySet<ushort> PvPBuffs => _pvpBuffs;

  public bool HasPvPBuff(ushort buffType)
  {
    return _pvpBuffs.Contains(buffType);
  }

  internal void SetHostile(bool value)
  {
    Hostile = value;
  }

  internal void SetIdentityNetworkState(
    byte skinVariant,
    byte voiceVariant,
    float voicePitchOffset,
    ushort hiddenAccessories,
    byte hideMiscBits,
    byte difficultyAndAccessoryFlags,
    byte biomeAndCartFlags,
    byte permanentUpgradeFlags)
  {
    SkinVariant = skinVariant;
    VoiceVariant = voiceVariant;
    VoicePitchOffset = voicePitchOffset;
    HiddenAccessories = hiddenAccessories;
    HideMiscBits = hideMiscBits;
    DifficultyAndAccessoryFlags = difficultyAndAccessoryFlags;
    BiomeAndCartFlags = biomeAndCartFlags;
    PermanentUpgradeFlags = permanentUpgradeFlags;
  }

  internal void SetControlNetworkState(
    byte controlFlags,
    byte movementFlags,
    byte playerFeatureFlags,
    byte actionFlags)
  {
    ControlFlags = controlFlags;
    MovementFlags = movementFlags;
    PlayerFeatureFlags = playerFeatureFlags;
    ActionFlags = actionFlags;
  }

  public void ApplyControlNetworkState(
    byte controlFlags,
    byte movementFlags,
    byte playerFeatureFlags,
    byte actionFlags)
  {
    SetControlNetworkState(
      controlFlags,
      movementFlags,
      playerFeatureFlags,
      actionFlags);
  }

  internal void SetPotionOfReturnState(
    Vector2? usePosition,
    Vector2? homePosition)
  {
    IsPotionOfReturnInUse = usePosition.HasValue || homePosition.HasValue;
    PotionOfReturnPosition = usePosition;
    PotionOfReturnHomePosition = homePosition;
  }

  public void ApplyPotionOfReturnState(
    Vector2? usePosition,
    Vector2? homePosition)
  {
    SetPotionOfReturnState(usePosition, homePosition);
  }

  internal void SetStealth(float value)
  {
    Stealth = value;
  }

  internal void SetItemAnimation(float rotation, int animation, byte channel)
  {
    ItemRotation = rotation;
    ItemAnimation = animation;
    ItemAnimationChannel = channel;
  }

  internal void SetMinionRestTarget(Vector2 value)
  {
    MinionRestTargetPoint = value;
  }

  internal void SetMinionAttackTarget(int value)
  {
    MinionAttackTargetNpc = value;
  }

  internal void SetTalkNpc(int value)
  {
    TalkNpc = value;
  }

  internal void SetTownNpcCount(byte value)
  {
    TownNpcCount = value;
  }

  public void ApplyTownNpcCount(byte value)
  {
    SetTownNpcCount(value);
  }

  /// <summary>Applies the server's draw suppression projection.</summary>
  public void ApplyShouldNotDraw(bool value)
  {
    ShouldNotDraw = value;
  }

  internal void SetPvPBuff(ushort buffType, bool enabled)
  {
    if (enabled)
    {
      _pvpBuffs.Add(buffType);
    }
    else
    {
      _pvpBuffs.Remove(buffType);
    }
  }

  internal void ClearPvPBuffs()
  {
    _pvpBuffs.Clear();
  }

  internal void MarkTeleportRequested(Vector2 position)
  {
    HasPendingTeleport = true;
    PendingTeleportPosition = position;
  }

  internal void AcknowledgeTeleport()
  {
    HasPendingTeleport = false;
    PendingTeleportPosition = null;
  }

  private readonly HashSet<ushort> _pvpBuffs = new();
}
