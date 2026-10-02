using EntityEcs.Components;
using Terraria.WorldStorage;

namespace Terraria.LeashedEntity;

public readonly record struct LeashedEntityRegistrationRequest(
  EntityId RuntimeEntityId,
  int DefinitionId,
  SectionCoordinate Section,
  bool SectionActive);

