namespace MinimalImplementation.Server.Mechanisms;

/// <summary>
/// Authoritative world reads in the server commit window. SolidTile2 includes platforms and
/// rejects inactive/actuated tiles; liquidType null means any liquid, zero means water.
/// </summary>
public interface ILeashedEntityEnvironment
{
  long WorldScope { get; }
  int Width { get; }
  int Height { get; }
  bool IsSolidTile2(int x, int y);
  bool HasLiquid(int x, int y, int? liquidType = null);
}
