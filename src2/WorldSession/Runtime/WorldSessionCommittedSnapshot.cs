using Terraria.WorldSession.Components;
using Terraria.WorldSession.Session;

namespace Terraria.WorldSession.Runtime;

public sealed record WorldSessionCommittedSnapshot
{
  public required ActiveWorldMetadataProjection Metadata { get; init; }

  public required SecretSeedFlags SecretSeedFlags { get; init; }

  public required HardmodeSnapshot Hardmode { get; init; }
}
