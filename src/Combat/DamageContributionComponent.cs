using Terraria.Relationships;

namespace Terraria.Combat;

public sealed class DamageContributionComponent
{
  private readonly Dictionary<EntityReference, int> _damageBySource = new();

  public int GetDamage(EntityReference source)
  {
    return _damageBySource.GetValueOrDefault(source);
  }

  public void AddDamage(EntityReference source, int amount)
  {
    if (source.IsEmpty || amount <= 0)
    {
      return;
    }

    _damageBySource[source] = GetDamage(source) + amount;
  }
}
