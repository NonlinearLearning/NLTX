using MinimalImplementation.Contracts;

namespace MinimalImplementation.Modules.Creative;

public interface ICreativeSacrificeFacts
{
  bool IsValidSacrificeReport(ushort itemId, ushort amount);
}

public sealed record CreativeUnlockReport(
  byte AuthenticatedPlayerId,
  ushort ItemId,
  ushort Amount,
  CommitVersion Version)
{
  public CreativeUnlockReportWireMessage ToWireMessage() =>
    new(AuthenticatedPlayerId, ItemId, Amount);
}

/// <summary>Sequences relay reports; it does not own inventory or research progress.</summary>
public sealed class CreativeUnlockReportOwner {
  private CommitVersion _version = CommitVersion.Initial;

  public OwnerCommitResult Record(
      byte authenticatedPlayerId, CreativeUnlockReportWireMessage report,
      out CreativeUnlockReport? committedReport) {
    ArgumentNullException.ThrowIfNull(report);
    committedReport = null;
    if (authenticatedPlayerId == byte.MaxValue) {
      return OwnerCommitResult.Rejected(
          ProtocolErrorCode.Forbidden, "An authenticated player slot is required.");
    }

    CommitVersion nextVersion = _version.Next();
    committedReport = new CreativeUnlockReport(
        authenticatedPlayerId, report.ItemId, report.Amount, nextVersion);
    _version = nextVersion;
    return OwnerCommitResult.Applied(_version);
  }

  // Kept for existing composition callers. Version4 relays both ushort fields without
  // applying the inventory stack policy or server-side sacrifice bookkeeping.
  public OwnerCommitResult Record(
    byte authenticatedPlayerId,
    CreativeUnlockReportWireMessage report,
    ICreativeSacrificeFacts sacrificeFacts,
    out CreativeUnlockReport? committedReport)
  {
    ArgumentNullException.ThrowIfNull(report);
    ArgumentNullException.ThrowIfNull(sacrificeFacts);
    if (!sacrificeFacts.IsValidSacrificeReport(report.ItemId, report.Amount))
    {
      committedReport = null;
      return OwnerCommitResult.Rejected(
        ProtocolErrorCode.InvalidLength,
        "The creative sacrifice report does not describe a known item and positive amount.");
    }

    return Record(authenticatedPlayerId, report, out committedReport);
  }
}
