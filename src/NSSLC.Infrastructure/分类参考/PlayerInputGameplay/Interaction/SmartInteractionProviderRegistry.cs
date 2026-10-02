namespace NLTX.PlayerInputGameplay.Interaction;

public interface ISmartInteractionCandidateProvider
{
  SmartInteractionCandidateKind Kind { get; }

  void Provide(SmartInteractionScanSettings settings, SmartInteractionCandidateBuffer candidates);
}

public sealed class SmartInteractionProviderRegistry
{
  private readonly List<ISmartInteractionCandidateProvider> _providers = new();

  public IReadOnlyList<ISmartInteractionCandidateProvider> Providers => _providers;

  public void Register(ISmartInteractionCandidateProvider provider)
  {
    ArgumentNullException.ThrowIfNull(provider);
    _providers.Add(provider);
  }
}
