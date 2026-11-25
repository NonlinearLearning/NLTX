// Adapted from CrashKonijn/GOAP GraphResolver; see LICENSE.txt and SOURCES.md.
// Unity containers and pooled job handles replaced by immutable lookups and synchronous handles.
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Share.Ai.Goap.Core;

namespace Terraria.Share.Ai.Goap.Resolver;

public sealed class GraphResolver : IGraphResolver, IDisposable {
  private readonly Graph _graph;
  private readonly List<IConnectable> _actionIndexList;
  private readonly List<ICondition> _conditionIndexList;
  private readonly ILookup<int, int> _nodeConditions;
  private readonly ILookup<int, int> _conditionConnections;
  private bool _disposed;

  public GraphResolver(IConnectable[] actions, IKeyResolver keyResolver) {
    ArgumentNullException.ThrowIfNull(actions);
    ArgumentNullException.ThrowIfNull(keyResolver);
    _graph = new GraphBuilder(keyResolver).Build(actions);
    List<INode> indexList = _graph.AllNodes.ToList();
    _actionIndexList = indexList.Select(node => node.Action).ToList();
    List<INodeCondition> conditions = indexList.SelectMany(node => node.Conditions).ToList();
    _conditionIndexList = conditions.Select(condition => condition.Condition).ToList();

    _nodeConditions = indexList.SelectMany((node, index) => node.Conditions.Select(condition =>
        (Node: index, Condition: _conditionIndexList.IndexOf(condition.Condition))))
        .ToLookup(pair => pair.Node, pair => pair.Condition);
    _conditionConnections = conditions.SelectMany((condition, index) =>
        condition.Connections.Select(node => (Condition: index, Node: indexList.IndexOf(node))))
        .ToLookup(pair => pair.Condition, pair => pair.Node);
  }

  /// <summary>Executes immediately. Complete returns a result owned by the caller.</summary>
  public IResolveHandle StartResolve(RunData runData) {
    ObjectDisposedException.ThrowIf(_disposed, this);
    Validate(runData);
    return new ResolveHandle(this, _nodeConditions, _conditionConnections, runData);
  }

  public IEnabledBuilder GetEnabledBuilder() {
    return new EnabledBuilder(_actionIndexList);
  }

  public IExecutableBuilder GetExecutableBuilder() {
    return new ExecutableBuilder(_actionIndexList);
  }

  public IPositionBuilder GetPositionBuilder() {
    return new PositionBuilder(_actionIndexList);
  }

  public ICostBuilder GetCostBuilder() {
    return new CostBuilder(_actionIndexList);
  }

  public IConditionBuilder GetConditionBuilder() {
    return new ConditionBuilder(_conditionIndexList);
  }

  // The graph is exposed for inspection. Do not mutate it after construction.
  public IGraph GetGraph() {
    return _graph;
  }

  public int GetIndex(IConnectable action) {
    return _actionIndexList.IndexOf(action);
  }

  public IGoapAction GetAction(int index) {
    return (IGoapAction)_actionIndexList[index];
  }

  public IGoal GetGoal(int index) {
    return (IGoal)_actionIndexList[index];
  }

  public void Dispose() {
    _disposed = true;
  }

  private void Validate(RunData data) {
    int count = _actionIndexList.Count;
    if (data.StartIndex == null || data.IsEnabled?.Length != count ||
        data.IsExecutable?.Length != count || data.Costs?.Length != count ||
        data.Positions?.Length != count ||
        data.ConditionsMet?.Length != _conditionIndexList.Count) {
      throw new ArgumentException("RunData array lengths must match this graph.", nameof(data));
    }
    if (data.StartIndex.Any(index => index < 0 || index >= count ||
                                    _actionIndexList[index] is not IGoal)) {
      throw new ArgumentException("StartIndex must contain goal indices from this graph.",
                                  nameof(data));
    }
    if (!float.IsFinite(data.DistanceMultiplier) || data.DistanceMultiplier < 0f ||
        data.Costs!.Any(cost => !float.IsFinite(cost) || cost < 0f) ||
        !IsFinite(data.AgentPosition) || data.Positions!.Any(position => !IsFinite(position))) {
      throw new ArgumentException("Costs and positions must be finite; costs nonnegative.",
                                  nameof(data));
    }
  }

  private static bool IsFinite(System.Numerics.Vector3 position) {
    return float.IsFinite(position.X) && float.IsFinite(position.Y) &&
           float.IsFinite(position.Z);
  }
}
