namespace Terraria.Dome.Simulation.Player.Components;

public struct PlayerBankStateComponent
{
  public const int BankCount = 4;
  public const int BankCapacity = 40;

  public int BankId;
  public int Bank2Id;
  public int Bank3Id;
  public int Bank4Id;

  public PlayerBankStateComponent()
  {
    BankId = -2;
    Bank2Id = -3;
    Bank3Id = -4;
    Bank4Id = -5;
  }

  public bool IsKnown(int bankId)
  {
    return bankId is -2 or -3 or -4 or -5;
  }
}
