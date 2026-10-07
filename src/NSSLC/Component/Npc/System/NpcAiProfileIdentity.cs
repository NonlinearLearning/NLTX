namespace Terraria.Npc;

public readonly record struct NpcAiProfileIdentity(int TypeId, int NetId, int AiStyle)
{
  public override string ToString()
  {
    return $"type={TypeId}, netId={NetId}, aiStyle={AiStyle}";
  }
}
