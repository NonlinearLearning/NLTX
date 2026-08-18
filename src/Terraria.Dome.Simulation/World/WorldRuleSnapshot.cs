namespace Terraria.Dome.Simulation.WorldModel;

public readonly record struct WorldRuleSnapshot(long Tick, int TimeOfDay, bool IsDayTime);
