// Adapted from CrashKonijn/GOAP 7fb29cfe59dc61aa8cf1f69880d3ef95fc29126c.
// Licensed under Apache-2.0; see LICENSE.txt and NOTICE.txt.
using System.Numerics;

namespace Terraria.Share.Ai.Goap.Core;

public interface IAgent {
  IActionState ActionState { get; }
  IAgentEnvironment Environment { get; }
  Vector3 Position { get; }
  AgentState State { get; }
  AgentMoveState MoveState { get; }
  ITarget? CurrentTarget { get; }
  bool IsPaused { get; }
  bool IsInRange();
  void SetState(AgentState state);
  void SetMoveState(AgentMoveState state);
  void SetAction(IAction action, ITarget? target);
  void StopAction(bool resolveAction = true);
  void CompleteAction(bool resolveAction = true);
  void ResolveAction();
  bool IsDisabled(IAction action);
}
