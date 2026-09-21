namespace Terraria.Content.Items;

public sealed record ItemProjectileUseDefinition
{
  public ItemProjectileUseDefinition(ProjectileContentId projectileTypeId, float projectileSpeed)
  {
    if (float.IsNaN(projectileSpeed) || float.IsInfinity(projectileSpeed) || projectileSpeed < 0f)
    {
      throw new ArgumentOutOfRangeException(nameof(projectileSpeed));
    }

    ProjectileTypeId = projectileTypeId;
    ProjectileSpeed = projectileSpeed;
  }

  public static ItemProjectileUseDefinition None => new(ProjectileContentId.None, 0f);

  public bool HasProjectile => ProjectileTypeId.HasValue;

  public ProjectileContentId ProjectileTypeId { get; }

  public float ProjectileSpeed { get; }
}
