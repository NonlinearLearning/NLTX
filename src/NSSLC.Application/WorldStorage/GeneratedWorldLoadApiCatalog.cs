using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.WorldStorage;
using Terraria.WorldStorage.Generated.NSSLC_Application;

namespace Terraria.NonAuthoritative.Persistence;

public sealed class GeneratedWorldLoadApiCatalog : IWorldLoadApiCatalog
{
  private readonly IReadOnlyList<WorldLoadApiDescriptor> _descriptors;
  private readonly Func<WorldLoadApiBindings, WorldLoadApiExecutionResult> _execute;

  public GeneratedWorldLoadApiCatalog()
    : this(WorldLoadApiCatalog.Descriptors, WorldLoadApiCatalog.Execute)
  {
  }

  public GeneratedWorldLoadApiCatalog(
    IReadOnlyList<WorldLoadApiDescriptor> descriptors,
    Func<WorldLoadApiBindings, WorldLoadApiExecutionResult> execute)
  {
    ArgumentNullException.ThrowIfNull(descriptors);
    ArgumentNullException.ThrowIfNull(execute);

    _descriptors = Array.AsReadOnly(descriptors.ToArray());
    _execute = execute;
  }

  public IReadOnlyList<WorldLoadApiDescriptor> Descriptors => _descriptors;

  public WorldLoadApiExecutionResult Execute(WorldLoadApiBindings bindings)
  {
    return _execute.Invoke(bindings);
  }
}
