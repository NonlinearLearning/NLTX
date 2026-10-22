namespace MinimalImplementation.Server.Mechanisms;

public interface IMechanismIdentityAuthority
{
  /// <summary>Checks the root identity registry at the current world scope.</summary>
  bool IsActive(MechanismEntityHandle handle);

  /// <summary>
  /// Retires every supplied handle or leaves the registry unchanged when any cannot be retired.
  /// </summary>
  bool TryRetireMany(IReadOnlyList<MechanismEntityHandle> handles);
}
