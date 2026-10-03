using System.Collections.Generic;
using Terraria.WorldStorage;

namespace Terraria.NonAuthoritative.Persistence;

public interface IWorldLoadApiCatalog
{
  IReadOnlyList<WorldLoadApiDescriptor> Descriptors { get; }

  WorldLoadApiExecutionResult Execute(WorldLoadApiBindings bindings);
}
