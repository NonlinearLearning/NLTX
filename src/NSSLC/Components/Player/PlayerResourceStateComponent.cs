using System;

namespace Terraria.Player;

/// <summary>
/// Stores player-owned resources that are not item stacks, currently Mana and its regeneration.
/// </summary>
public sealed class PlayerResourceStateComponent
{
  public PlayerResourceStateComponent(
      int maximumMana,
      int currentMana,
      float manaRegenerationPerSecond = 0f)
  {
    if (maximumMana < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maximumMana));
    }

    if (currentMana < 0 || currentMana > maximumMana)
    {
      throw new ArgumentOutOfRangeException(nameof(currentMana));
    }

    if (!float.IsFinite(manaRegenerationPerSecond) || manaRegenerationPerSecond < 0f)
    {
      throw new ArgumentOutOfRangeException(nameof(manaRegenerationPerSecond));
    }

    MaximumMana = maximumMana;
    CurrentMana = currentMana;
    ManaRegenerationPerSecond = manaRegenerationPerSecond;
  }

  public int CurrentMana { get; private set; }

  public int MaximumMana { get; private set; }

  public float ManaRegenerationPerSecond { get; private set; }

  public float ManaRegenerationRemainder { get; private set; }

  public bool TryConsumeMana(int amount)
  {
    if (amount < 0 || amount > CurrentMana)
    {
      return false;
    }

    CurrentMana -= amount;
    return true;
  }

  public void RestoreMana(int amount)
  {
    if (amount < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(amount));
    }

    CurrentMana = Math.Min(MaximumMana, CurrentMana + amount);
  }

  public void ConfigureMana(int maximumMana, float manaRegenerationPerSecond)
  {
    if (maximumMana < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maximumMana));
    }

    if (!float.IsFinite(manaRegenerationPerSecond) || manaRegenerationPerSecond < 0f)
    {
      throw new ArgumentOutOfRangeException(nameof(manaRegenerationPerSecond));
    }

    MaximumMana = maximumMana;
    CurrentMana = Math.Min(CurrentMana, maximumMana);
    ManaRegenerationPerSecond = manaRegenerationPerSecond;
    ManaRegenerationRemainder = Math.Clamp(ManaRegenerationRemainder, 0f, 1f);
  }

  public void Advance(float deltaTime)
  {
    if (!float.IsFinite(deltaTime) || deltaTime <= 0f)
    {
      throw new ArgumentOutOfRangeException(nameof(deltaTime));
    }

    if (CurrentMana >= MaximumMana || ManaRegenerationPerSecond <= 0f)
    {
      return;
    }

    float total = ManaRegenerationRemainder + ManaRegenerationPerSecond * deltaTime;
    int restored = Math.Min(MaximumMana - CurrentMana, (int)MathF.Floor(total));
    if (restored > 0)
    {
      CurrentMana += restored;
    }

    ManaRegenerationRemainder = total - restored;
    if (CurrentMana == MaximumMana)
    {
      ManaRegenerationRemainder = 0f;
    }
  }
}
