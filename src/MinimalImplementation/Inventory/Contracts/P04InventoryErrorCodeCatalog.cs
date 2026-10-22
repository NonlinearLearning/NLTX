using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.Inventory.Contracts;

/// <summary>Stable protocol errors that P04 may return to the root adapter.</summary>
public static class P04InventoryErrorCodeCatalog
{
  private static readonly IReadOnlyList<ProtocolErrorCode> _supported =
    Array.AsReadOnly(
      new[]
      {
        ProtocolErrorCode.Conflict,
        ProtocolErrorCode.Forbidden,
        ProtocolErrorCode.InvalidLength,
        ProtocolErrorCode.MalformedFrame,
        ProtocolErrorCode.NotReady,
        ProtocolErrorCode.StaleEntity,
        ProtocolErrorCode.StaleEpoch,
        ProtocolErrorCode.UnknownSubtype,
        ProtocolErrorCode.WrongStage
      });

  public static IReadOnlyList<ProtocolErrorCode> Supported => _supported;

  public static bool IsSupported(ProtocolErrorCode errorCode)
  {
    return _supported.Contains(errorCode);
  }

  public static bool IsComplete()
  {
    return _supported.Count == 9
      && _supported.Distinct().Count() == _supported.Count;
  }
}
