namespace Terraria.Physics;

/// <summary>Declares broad-phase participation and filtering for entity contacts.</summary>
public readonly record struct EntityCollisionCapabilityComponent(
    bool Enabled,
    uint Layer,
    uint Mask)
{
  public bool CanCollideWith(uint otherLayer)
  {
    return Enabled && (Mask & otherLayer) != 0;
  }
}
