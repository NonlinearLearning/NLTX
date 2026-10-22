using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.Mechanisms;

/// <summary>Allocates committed versions for mechanism mutations that have no shared owner clock.</summary>
public sealed class MechanismCommitVersionAllocator
{
  private CommitVersion _version = CommitVersion.Initial;
  private bool _hasAllocated;

  public CommitVersion Allocate()
  {
    if (!_hasAllocated)
    {
      _hasAllocated = true;
      return _version;
    }

    _version = _version.Next();
    return _version;
  }
}
