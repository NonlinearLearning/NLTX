using System;

namespace EntityEcs.Components;

// status: proposed
public sealed class SpawnAdmissionState
{
  public SpawnAdmissionState(
    SpawnAdmissionStatus status = SpawnAdmissionStatus.NotRequested,
    SpawnSourceKind sourceKind = SpawnSourceKind.Unknown,
    string? admissionKey = null,
    SpawnAuthorityKind authority = SpawnAuthorityKind.Unknown,
    uint attemptCount = 0,
    SpawnRejectionReason rejectionReason = SpawnRejectionReason.None)
  {
    Status = status;
    SourceKind = sourceKind;
    AdmissionKey = admissionKey;
    Authority = authority;
    AttemptCount = attemptCount;
    RejectionReason = rejectionReason;
    Validate();
  }

  public SpawnAdmissionStatus Status { get; }

  public SpawnSourceKind SourceKind { get; }

  public string? AdmissionKey { get; }

  public SpawnAuthorityKind Authority { get; }

  public uint AttemptCount { get; }

  public SpawnRejectionReason RejectionReason { get; }

  public void Validate()
  {
    bool isRejected = Status == SpawnAdmissionStatus.Rejected
      || Status == SpawnAdmissionStatus.Cancelled;

    if (isRejected && RejectionReason == SpawnRejectionReason.None)
    {
      throw new InvalidOperationException(
        "Rejected or cancelled admission requires a rejection reason.");
    }

    if (!isRejected && RejectionReason != SpawnRejectionReason.None)
    {
      throw new InvalidOperationException(
        "A rejection reason is only valid for rejected or cancelled admission.");
    }

    if (AdmissionKey is not null && string.IsNullOrWhiteSpace(AdmissionKey))
    {
      throw new ArgumentException(
        "An admission key must contain non-whitespace characters.",
        nameof(AdmissionKey));
    }

    if (Status == SpawnAdmissionStatus.Admitted
      && Authority == SpawnAuthorityKind.Unknown)
    {
      throw new InvalidOperationException(
        "An admitted candidate requires a known authority.");
    }
  }
}
