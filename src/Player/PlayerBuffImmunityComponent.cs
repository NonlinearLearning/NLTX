namespace Terraria.Player;

// status: implemented-isolated-core
// source-members: P09-718
// crossSubsystemOwner: buff catalog capacity and effect rebuild remain integration-review
public sealed class PlayerBuffImmunityComponent
{
  private readonly bool[] _immuneBuffTypes;

  public PlayerBuffImmunityComponent(int buffTypeCount)
  {
    if (buffTypeCount < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(buffTypeCount));
    }

    _immuneBuffTypes = new bool[buffTypeCount];
  }

  public IReadOnlyList<bool> ImmuneBuffTypes => _immuneBuffTypes;
}
