namespace Terraria.Dome.Simulation.Items;

public readonly record struct ItemCommandRejection(string Code, string Reason)
{
  public static ItemCommandRejection Invalid(string reason)
  {
    return new ItemCommandRejection("invalid", reason);
  }
}
