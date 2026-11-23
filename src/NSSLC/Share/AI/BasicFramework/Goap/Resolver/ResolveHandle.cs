// Adapted from CrashKonijn/GOAP ResolveHandle; see LICENSE.txt and SOURCES.md.
using System.Collections.Generic;
using System.Linq;
using Terraria.Ai.Goap.Core;

namespace Terraria.Ai.Goap.Resolver;

public sealed class ResolveHandle : IResolveHandle {
  private readonly JobResult _result;

  internal ResolveHandle(GraphResolver resolver, ILookup<int, int> nodeConditions,
                         ILookup<int, int> conditionConnections, RunData data) {
    GraphResolverJob job = new() {
      NodeConditions = nodeConditions,
      ConditionConnections = conditionConnections,
      RunData = data,
      Result = new List<NodeData>(),
      PickedGoal = new List<NodeData>()
    };
    job.Execute();
    _result = new JobResult {
      Actions = job.Result.Select(node => (IConnectable)resolver.GetAction(node.Index)).ToArray(),
      Goal = job.PickedGoal.Count == 0 ? null : resolver.GetGoal(job.PickedGoal[0].Index)
    };
  }

  public JobResult Complete() {
    return _result;
  }
}
