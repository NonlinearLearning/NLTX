namespace Terraria.WorldGeneration.Housing;

public sealed class HousingRoomDiagnosticFlags
{
  public bool HasStinkbug { get; private set; }

  public bool HasEchoStinkbug { get; private set; }

  public void MarkStinkbug()
  {
    HasStinkbug = true;
  }

  public void MarkEchoStinkbug()
  {
    HasEchoStinkbug = true;
  }

  public void Replace(bool hasStinkbug, bool hasEchoStinkbug)
  {
    HasStinkbug = hasStinkbug;
    HasEchoStinkbug = hasEchoStinkbug;
  }

  public void Clear()
  {
    HasStinkbug = false;
    HasEchoStinkbug = false;
  }
}
