using Terraria.Dome.Simulation.Wiring.Components;

namespace Terraria.Dome.Simulation.Wiring.Commands;

public readonly record struct WireTraversalNode(int Sequence, int X, int Y, WireColor Color);
