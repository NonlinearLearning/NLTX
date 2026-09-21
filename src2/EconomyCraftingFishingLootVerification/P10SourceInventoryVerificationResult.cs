namespace NLTX.EconomyCraftingFishingLootVerification;

public sealed class P10SourceInventoryVerificationResult
{
  public P10SourceInventoryVerificationResult(
    bool isValid,
    string failureMessage)
  {
    IsValid = isValid;
    FailureMessage = failureMessage;
  }

  public bool IsValid { get; }

  public string FailureMessage { get; }
}
