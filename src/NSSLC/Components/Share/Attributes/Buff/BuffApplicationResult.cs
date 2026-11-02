namespace Terraria.Attributes;

/// <summary>Result returned after the shared Buff system attempts an application.</summary>
public readonly record struct BuffApplicationResult(
    BuffAdmissionResult Status,
    bool Added,
    bool Refreshed,
    int Stacks,
    long SourceId)
{
  public bool Applied => Status == BuffAdmissionResult.Accepted;

  public static BuffApplicationResult Rejected(
      BuffAdmissionResult status,
      long sourceId = 0)
  {
    return new BuffApplicationResult(status, false, false, 0, sourceId);
  }
}
