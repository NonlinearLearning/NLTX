namespace Terraria.Communication;

/// <summary>
/// Signals that one session ended. Carries only what the ECS side acts on.
/// </summary>
/// <remarks>
/// 领域侧只关心"哪个会话结束了、为什么"。最后收到的序列号与最后一次发送的确定性属于传输细节，
/// 留在网络侧记录，不进领域事件。
/// </remarks>
public sealed record ConnectionClosed
{
  public SessionEpoch Epoch { get; }

  public string Reason { get; }

  public ConnectionClosed(SessionEpoch epoch, string reason)
  {
    if (!epoch.IsValid)
    {
      throw new ArgumentException("A close event must identify a valid session epoch.", nameof(epoch));
    }

    ArgumentException.ThrowIfNullOrWhiteSpace(reason);
    Epoch = epoch;
    Reason = reason;
  }
}
