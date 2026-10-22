namespace MinimalImplementation.Combat.TagEffects;

/// <summary>Server-visible secondary effect produced by a committed tagged hit.</summary>
public enum TagEffectTaggedAction
{
  None,
  DarkHarvestLightning
}

/// <summary>Server-visible secondary effect produced after a proc mark is consumed.</summary>
public enum TagEffectProcAction
{
  None,
  StarcrashMeteor,
  FlowerPetals,
  PossessionMoonLordWhip,
  FirecrackerExplosion
}
