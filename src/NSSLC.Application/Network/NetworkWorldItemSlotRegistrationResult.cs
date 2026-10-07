namespace Terraria.Network;

public readonly record struct NetworkWorldItemSlotRegistrationResult(
  bool Succeeded,
  short ItemIndex,
  uint Generation,
  string? RejectionCode);
