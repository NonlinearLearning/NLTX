using Terraria.Dome.Simulation.Npc.Components;

namespace Terraria.Dome.Simulation.Npc.Systems;

public sealed class NpcHomePublicationSystem
{
  public bool ApplyHomeTileState(
    ref NpcHomeComponent home,
    ref NpcHomePublicationComponent publication,
    bool isHomeless,
    short homeTileX,
    short homeTileY)
  {
    bool changed = home.IsHomeless != isHomeless ||
      home.HomeTileX != homeTileX ||
      home.HomeTileY != homeTileY;
    home.IsHomeless = isHomeless;
    home.HomeTileX = homeTileX;
    home.HomeTileY = homeTileY;
    publication.CapturePublishedState(home);
    return changed;
  }

  public void CapturePublishedState(
    ref NpcHomePublicationComponent publication,
    NpcHomeComponent home)
  {
    publication.CapturePublishedState(home);
  }
}
