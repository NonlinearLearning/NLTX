namespace Terraria.Projectile;

public struct ProjectileDefinitionComponent
{
  public ProjectileDefinitionComponent(
    int type,
    bool friendly,
    bool hostile,
    int extraUpdates)
  {
    Type = type;
    Friendly = friendly;
    Hostile = hostile;
    ExtraUpdates = extraUpdates;
  }

  public int Type;
  public bool Friendly;
  public bool Hostile;
  public int ExtraUpdates;
}
