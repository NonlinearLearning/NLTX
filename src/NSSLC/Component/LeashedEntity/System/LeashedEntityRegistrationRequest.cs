using Terraria.WorldStorage;

namespace Terraria.LeashedEntity;

public readonly record struct LeashedEntityRegistrationRequest(
  int DefinitionId,
  SectionCoordinate Section,
  bool SectionActive);
