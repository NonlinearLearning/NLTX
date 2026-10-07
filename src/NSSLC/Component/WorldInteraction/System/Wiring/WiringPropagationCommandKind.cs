namespace Terraria.WorldInteraction.Wiring;

public enum WiringPropagationCommandKind
{
  Invalid,
  Initialize,
  BeginTrip,
  BeginWireColorPass,
  SkipTile,
  QueueTile,
  QueueLamp,
  QueueGate,
  QueueNextGate,
  CompleteGate,
  RecordPixelBoxTrigger,
  EndTrip,
  Reset
}
