using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.Modules;

/// <summary>
/// Identifies module-owned state that must be retired with its authenticated session epoch.
/// </summary>
public interface IModuleEpochRetirable
{
  void RetireEpoch(SessionEpoch epoch);
}
