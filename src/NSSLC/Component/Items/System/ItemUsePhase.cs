namespace Terraria.Items;

public enum ItemUsePhase : byte
{
  Idle,
  Starting,
  Using,
  Channeling,
  Cooldown,
  Interrupted,
}
