namespace Terraria.Combat;

/// <summary>Outcome of one request processed by the shared Combat boundary.</summary>
public readonly record struct DamageProcessResult(
    DamageProcessStatus Status,
    DamageRejectionReason RejectionReason,
    DamageAppliedReceipt? Receipt)
{
  public bool Applied => Status == DamageProcessStatus.Applied && Receipt.HasValue;

  public static DamageProcessResult Accepted(in DamageAppliedReceipt receipt)
  {
    return new DamageProcessResult(DamageProcessStatus.Applied, DamageRejectionReason.None, receipt);
  }

  public static DamageProcessResult Rejected(DamageRejectionReason reason)
  {
    return new DamageProcessResult(DamageProcessStatus.Rejected, reason, null);
  }
}

public enum DamageProcessStatus : byte
{
  Applied = 0,
  Rejected = 1
}
