namespace Terraria.WorldInteraction.Interaction;

public sealed class InteractionActorContextComponent
{
  // null 表示没有可确认的 Version4 玩家索引。
  // Version4 的兼容无调用者值为 255，但不把 255 暴露为有效玩家。
  public byte? InitiatingPlayerIndex { get; internal set; }
}
