namespace Terraria.Player;

// status: implemented-isolated-core
// source-members: P09-1326..P09-1342, P09-1346..P09-1348
// crossSubsystemOwner: color calculation, item metadata, and renderer consumption remain integration-review
public sealed class PlayerEquipmentColorProjectionComponent
{
  public int CHead { get; internal set; }

  public int CBody { get; internal set; }

  public int CLegs { get; internal set; }

  public int CHandOn { get; internal set; }

  public int CHandOff { get; internal set; }

  public int CBack { get; internal set; }

  public int CFront { get; internal set; }

  public int CShoe { get; internal set; }

  public int CWaist { get; internal set; }

  public int CShield { get; internal set; }

  public int CNeck { get; internal set; }

  public int CFace { get; internal set; }

  public int CFaceHead { get; internal set; }

  public int CFaceFlower { get; internal set; }

  public int CFaceMask { get; internal set; }

  public int CBalloon { get; internal set; }

  public int CBalloonFront { get; internal set; }

  public int CBackpack { get; internal set; }

  public int CTail { get; internal set; }

  public int CShieldFallback { get; internal set; } = -1;
}
