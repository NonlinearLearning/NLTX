namespace Terraria.Combat;

/// <summary>
/// 保存主体生命恢复或持续伤害的速率、累积量和恢复进度。
/// </summary>

public struct HealthRegenerationComponent
{
  public int Rate;
  public int InitialRate;
  public int OneTickChangeRate;
  public int FinalRate;
  // /// <summary>
  // /// NPC 持续伤害消费规则使用的预期每秒损失，可为空。
  // /// </summary>
  // public int? ExpectedLossPerSecond;
}
