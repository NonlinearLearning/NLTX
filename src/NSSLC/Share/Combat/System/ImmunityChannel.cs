namespace Terraria.Share.Combat;

/// <summary>Damage delivery channel used to select one target-side immunity slot.</summary>
/// <remarks>
/// 对应参考客户端 ImmunityCooldownID，但 General 不再是 -1 哨兵：它与其余通道同列、同数组、同索引，
/// 因此 _untilTick[(int)channel] 恒成立。参考端的 LegacyUnused2 不移植，槽位总数仍为 6。
/// 通道与 DamageCause（来源种类）正交：同为弹体，毒牙是 General，岩浆弹是 Lava。
/// </remarks>
public enum ImmunityChannel : byte
{
  /// <summary>Default channel. Every attack that does not declare one lands here.</summary>
  General = 0,

  /// <summary>Contact damage from world tiles.</summary>
  TileContact = 1,

  /// <summary>Boss attacks that must not be cheesed by window stacking.</summary>
  BossNoCheese = 2,

  /// <summary>Wrong bug net hits.</summary>
  WrongBugNet = 3,

  /// <summary>Lava and other persistent environmental burn.</summary>
  Lava = 4,

  /// <summary>Damage passed to a teammate by the Paladin's Shield.</summary>
  PaladinsShield = 5,

  /// <summary>Number of real slots. Index into ImmunityWindowComponent.</summary>
  Count = 6
}
