namespace Terraria.Player;

public static class PlayerNameLengthQuery
{
  public static bool IsWithinLimit(int length)
  {
    return length >= 0 && length <= PlayerNameDefinition.MaximumLength;
  }
}
