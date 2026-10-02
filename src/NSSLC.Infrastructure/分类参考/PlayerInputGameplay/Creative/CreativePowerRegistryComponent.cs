namespace NLTX.PlayerInputGameplay.Creative;

public sealed class CreativePowerRegistryComponent
{
  private readonly Dictionary<ushort, CreativePowerContract> _powersById = new();
  private readonly Dictionary<string, CreativePowerContract> _powersByName = new(StringComparer.Ordinal);

  public bool IsInitialized { get; private set; }

  public int Count => _powersById.Count;

  public IReadOnlyCollection<CreativePowerContract> Powers => _powersById.Values;

  public bool Register(CreativePowerContract contract)
  {
    ArgumentNullException.ThrowIfNull(contract);
    if (_powersById.ContainsKey(contract.PowerId) || _powersByName.ContainsKey(contract.ServerConfigName))
    {
      return false;
    }

    _powersById.Add(contract.PowerId, contract);
    _powersByName.Add(contract.ServerConfigName, contract);
    return true;
  }

  public bool TryGet(ushort powerId, out CreativePowerContract? contract)
  {
    return _powersById.TryGetValue(powerId, out contract);
  }

  public bool TryGet(string serverConfigName, out CreativePowerContract? contract)
  {
    ArgumentNullException.ThrowIfNull(serverConfigName);
    return _powersByName.TryGetValue(serverConfigName, out contract);
  }

  internal void MarkInitialized()
  {
    IsInitialized = true;
  }
}
