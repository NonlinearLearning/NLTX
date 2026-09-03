namespace Terraria.Dome.Simulation.WorldModel;

public readonly record struct WorldRuleSnapshot(long Tick, double TimeOfDay, bool IsDayTime);
