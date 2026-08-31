using Terraria.Dome.Simulation.Npc.Definitions;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Npc.Components;

public readonly record struct NpcChaseState(float Speed, float StoppingDistance);

public readonly record struct NpcTownHomeState(
  SimulationVector HomePosition,
  bool IsHomeless,
  int ReturnTimeoutTicks);

public readonly record struct NpcFlyingState(
  float HorizontalAcceleration,
  float VerticalAcceleration,
  float MaximumHorizontalSpeed,
  float MaximumVerticalSpeed);

public struct NpcBehaviorStateComponent
{
  public NpcBehaviorStateComponent(
    NpcBehaviorId behaviorId,
    NpcChaseState chase,
    NpcTownHomeState townHome,
    NpcFlyingState flying = default,
    bool isChaseable = true,
    bool doesNotTakeDamage = false,
    bool doesNotTakeDamageFromHostiles = false,
    bool reflectsProjectiles = false)
  {
    BehaviorId = behaviorId;
    Chase = chase;
    TownHome = townHome;
    Flying = flying;
    IsChaseable = isChaseable;
    DoesNotTakeDamage = doesNotTakeDamage;
    DoesNotTakeDamageFromHostiles = doesNotTakeDamageFromHostiles;
    ReflectsProjectiles = reflectsProjectiles;
    PhaseTicks = 0;
  }

  public NpcBehaviorId BehaviorId;
  public NpcChaseState Chase;
  public NpcTownHomeState TownHome;
  public NpcFlyingState Flying;
  public NpcRangedAttackState RangedAttack;
  public bool IsChaseable;
  public bool DoesNotTakeDamage;
  public bool DoesNotTakeDamageFromHostiles;
  public bool ReflectsProjectiles;
  public int PhaseTicks;
}
