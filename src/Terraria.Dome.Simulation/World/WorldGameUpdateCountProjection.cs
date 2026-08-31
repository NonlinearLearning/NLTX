namespace Terraria.Dome.Simulation.WorldModel;

public sealed class WorldGameUpdateCountProjection
{
  private uint _value;

  public WorldGameUpdateCountProjection(uint initialValue = 0)
  {
    _value = initialValue;
  }

  public uint Value => _value;

  public void Advance()
  {
    _value = unchecked(_value + 1u);
  }
}
