namespace MinimalImplementation.Combat.TagEffects;

/// <summary>Explicit random source for crit outcomes; TagEffect rules never use a global RNG.</summary>
public interface ITagEffectRandom
{
  bool RollPercent(int percent);
}
