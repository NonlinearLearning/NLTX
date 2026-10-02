namespace Terraria.Npc;

public struct NpcDirectionComponent
{
  public NpcDirectionComponent(int vertical, int sprite)
  {
    Vertical = vertical;
    Sprite = sprite;
  }

  public int Vertical;
  public int Sprite;
}
