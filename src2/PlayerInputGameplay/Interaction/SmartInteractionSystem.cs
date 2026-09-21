namespace NLTX.PlayerInputGameplay.Interaction;

public sealed class SmartInteractionSystem
{
  public void Scan(
    SmartInteractionProviderRegistry registry,
    SmartInteractionScanSettings settings,
    SmartInteractionCandidateBuffer candidates)
  {
    ArgumentNullException.ThrowIfNull(registry);
    ArgumentNullException.ThrowIfNull(candidates);
    candidates.Clear();
    foreach (var provider in registry.Providers)
    {
      provider.Provide(settings, candidates);
    }
  }
}
