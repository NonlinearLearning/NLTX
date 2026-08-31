namespace Terraria.Dome.Simulation.Npc.Components;

public struct NpcHomePublicationComponent
{
  public NpcHomePublicationComponent()
  {
    LastPublishedHomeTileX = -1;
    LastPublishedHomeTileY = -1;
    LastPublishedHomeless = false;
  }

  public NpcHomePublicationComponent(
    short lastPublishedHomeTileX,
    short lastPublishedHomeTileY,
    bool lastPublishedHomeless)
  {
    LastPublishedHomeTileX = lastPublishedHomeTileX;
    LastPublishedHomeTileY = lastPublishedHomeTileY;
    LastPublishedHomeless = lastPublishedHomeless;
  }

  public short LastPublishedHomeTileX;
  public short LastPublishedHomeTileY;
  public bool LastPublishedHomeless;

  public bool HasPendingPublication(NpcHomeComponent home)
  {
    return home.HomeTileX != LastPublishedHomeTileX ||
      home.HomeTileY != LastPublishedHomeTileY ||
      home.IsHomeless != LastPublishedHomeless;
  }

  public void CapturePublishedState(NpcHomeComponent home)
  {
    LastPublishedHomeTileX = home.HomeTileX;
    LastPublishedHomeTileY = home.HomeTileY;
    LastPublishedHomeless = home.IsHomeless;
  }
}
