namespace Terraria.Player;

public sealed class PlayerAccessoryStringEffectComponent
{
  public int ExtraAccessorySlots { get; internal set; } = 2;

  public bool ExtraAccessory { get; internal set; }

  public int TankPet { get; internal set; } = -1;

  public bool TankPetReset { get; internal set; }

  public int StringColor { get; internal set; }

  public int CounterWeight { get; internal set; }

  public int VanityCounterWeight { get; internal set; }

  public bool MagicString { get; internal set; }

  public bool YoyoString { get; internal set; }

  public bool YoyoGlove { get; internal set; }

  public float RapidAttackBonus { get; internal set; }

  public bool StressBall { get; internal set; }

  public bool StressBallPrevious { get; internal set; }

  public bool StaffOfRegrowthBonus { get; internal set; }
}
