namespace Terraria.WorldSession.Runtime;

public static class HardmodeReadProjection
{
  public static HardmodeSnapshot Read(HardmodeSnapshot committedSnapshot)
  {
    return committedSnapshot;
  }
}
