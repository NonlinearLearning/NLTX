namespace Terraria.NonAuthoritative.Platform;

public sealed class RuntimeCapabilityQuery
{
  private readonly Func<bool> _reflectionContextProbe;

  public RuntimeCapabilityQuery(Func<bool> reflectionContextProbe)
  {
    _reflectionContextProbe = reflectionContextProbe ??
      throw new ArgumentNullException(nameof(reflectionContextProbe));
  }

  public bool IsReflectionContextAvailable()
  {
    return _reflectionContextProbe();
  }
}
