namespace Terraria.World;

/// <summary>Outcome of processing one world interaction request.</summary>
public enum WorldInteractionResultStatus : byte
{
  None = 0,
  Succeeded = 1,
  Rejected = 2
}
