using MinimalImplementation.Contracts;
using LeashedEntityMessage =
  MinimalImplementation.Server.Protocol.Version4ModulePayloads.LeashedEntityMessage;

namespace MinimalImplementation.Server.Mechanisms;

/// <summary>Owns the state-changing boundary for a committed module 13 operation.</summary>
public interface ILeashedEntityCommitPort
{
  long CurrentRevision { get; }

  LeashedEntityCommitResult Commit(
    LeashedEntityMessage message,
    MechanismSectionCoordinate section,
    CommitVersion commitVersion);
}

public sealed record LeashedEntityCommitResult(
  LeashedEntityChangeResult Change,
  long RegistryRevision);

/// <summary>
/// Local P06 adapter used until the root composition supplies the full cross-owner commit port.
/// </summary>
public sealed class LeashedEntityRegistryCommitPort : ILeashedEntityCommitPort
{
  private readonly LeashedEntityRegistry _registry;

  public LeashedEntityRegistryCommitPort(LeashedEntityRegistry registry)
  {
    _registry = registry ?? throw new ArgumentNullException(nameof(registry));
  }

  public long CurrentRevision => _registry.Revision;

  public LeashedEntityCommitResult Commit(
    LeashedEntityMessage message,
    MechanismSectionCoordinate section,
    CommitVersion commitVersion)
  {
    ArgumentNullException.ThrowIfNull(message);
    if (!commitVersion.IsValid)
    {
      throw new ArgumentException(
        "A leashed entity commit requires a valid committed version.",
        nameof(commitVersion));
    }

    LeashedEntityChangeResult change = _registry.Apply(message, section);
    return new LeashedEntityCommitResult(change, _registry.Revision);
  }
}
