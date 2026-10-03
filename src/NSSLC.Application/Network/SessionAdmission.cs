namespace Terraria.Network;

public sealed record SessionAdmission(SenderBinding? Binding, string? RejectionCode = null);
