namespace Terraria.Player;

public enum PlayerStatusEffectRejectionReason : byte
{
  None,
  InvalidEffectType,
  NonPositiveDuration,
  UnknownDefinition,
  Immune,
  NoAvailableSlot,
}
