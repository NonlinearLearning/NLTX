namespace Terraria.LeashedEntity;

/// <summary>
/// Validated network intent for the registration owner. This command carries no authority write.
/// </summary>
public readonly record struct LeashedNetworkFrameCommand(
  LeashedNetworkFrame Frame,
  LeashedEntityHandle? ExistingHandle,
  bool RequiresRegistration);
