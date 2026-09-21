namespace Terraria.Content.Items;

public sealed record ItemResourceRecoveryDefinition
{
  public ItemResourceRecoveryDefinition(
    int lifeRecovery,
    int manaRecovery,
    int lifeRegen,
    int maxManaIncrease,
    int manaCost)
  {
    if (lifeRecovery < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(lifeRecovery));
    }

    if (manaRecovery < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(manaRecovery));
    }

    if (maxManaIncrease < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maxManaIncrease));
    }

    if (manaCost < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(manaCost));
    }

    LifeRecovery = lifeRecovery;
    ManaRecovery = manaRecovery;
    LifeRegen = lifeRegen;
    MaxManaIncrease = maxManaIncrease;
    ManaCost = manaCost;
  }

  public int LifeRegen { get; }

  public int LifeRecovery { get; }

  public int ManaCost { get; }

  public int ManaRecovery { get; }

  public int MaxManaIncrease { get; }
}
