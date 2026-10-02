namespace Terraria.Player;

// Stores compatibility identity facts for the two external container projectiles.
// The component never holds a Projectile instance or performs registry recovery.
public sealed class PlayerTrackedContainerLinkComponent
{
  public bool IsTrackingPiggyBankProjectile { get; set; }

  public LegacyProjectileSlot? PiggyBankProjectileLocalSlot { get; set; }

  public LegacyPlayerSlot? PiggyBankProjectileOwnerSlot { get; set; }

  public int? PiggyBankProjectileIdentity { get; set; }

  public int? PiggyBankProjectileType { get; set; }

  public bool IsTrackingVoidLensProjectile { get; set; }

  public LegacyProjectileSlot? VoidLensProjectileLocalSlot { get; set; }

  public LegacyPlayerSlot? VoidLensProjectileOwnerSlot { get; set; }

  public int? VoidLensProjectileIdentity { get; set; }

  public int? VoidLensProjectileType { get; set; }
}
