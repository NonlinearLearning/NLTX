namespace MinimalImplementation.Server.Inventory.State;

/// <summary>Server simulation counters; TimeLeft is not a universal item lifetime.</summary>
public sealed record WorldItemRuntime(
  int Age = 0,
  int NoGrabDelay = 0,
  bool Wet = false,
  bool LavaWet = false,
  bool HoneyWet = false,
  bool ShimmerWet = false,
  bool BeingGrabbed = false,
  int OwnTime = 0,
  int OwnIgnore = -1,
  int KeepTime = 0) {
  internal WorldItemRuntime AdvanceOwnershipTimers() {
    // A positive ownTime retains ownIgnore even when this update decrements it to zero.
    return this with {
      OwnTime = OwnTime > 0 ? OwnTime - 1 : 0,
      OwnIgnore = OwnTime > 0 ? OwnIgnore : -1,
      KeepTime = KeepTime > 0 ? KeepTime - 1 : 0
    };
  }
}
