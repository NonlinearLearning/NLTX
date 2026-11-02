namespace Terraria.Movement;

/// <summary>Describes how a movement capability interprets velocity and gravity.</summary>
public enum MovementMode : byte
{
 Grounded = 0,
 Free = 1,
 Flight = 2
}
