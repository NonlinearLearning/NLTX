namespace Terraria.EntityLifecycleAttribution;

public sealed class PresentationEffectPoolSystem
{
  private readonly PresentationEffectPoolStore _store;

  public PresentationEffectPoolSystem(PresentationEffectPoolStore store)
  {
    _store = store ?? throw new ArgumentNullException(nameof(store));
  }

  public void AdvanceTick()
  {
    _store.AdvanceTick();
  }
}
