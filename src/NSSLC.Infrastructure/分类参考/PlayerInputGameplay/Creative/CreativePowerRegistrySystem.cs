namespace NLTX.PlayerInputGameplay.Creative;

public sealed class CreativePowerRegistrySystem
{
  public void Initialize(CreativePowerRegistryComponent registry, IEnumerable<CreativePowerContract> contracts)
  {
    ArgumentNullException.ThrowIfNull(registry);
    ArgumentNullException.ThrowIfNull(contracts);
    foreach (var contract in contracts)
    {
      if (!registry.Register(contract))
      {
        throw new InvalidOperationException($"Creative power ID or name is already registered: {contract.PowerId}/{contract.ServerConfigName}.");
      }
    }

    registry.MarkInitialized();
  }
}
