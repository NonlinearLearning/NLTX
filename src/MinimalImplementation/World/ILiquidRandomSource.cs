namespace MinimalImplementation.Server.World;

/// <summary>
/// Host-owned world-generation random stream used by Liquid.Update's three-cell 254 branch.
/// Next must return a value in [0, maximumExclusive) and must not reenter or mutate World.
/// </summary>
public interface ILiquidRandomSource {
  int Next(int maximumExclusive);
}
