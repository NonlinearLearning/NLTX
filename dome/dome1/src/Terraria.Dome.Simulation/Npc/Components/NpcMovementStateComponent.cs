namespace Terraria.Dome.Simulation.Npc.Components;

public struct NpcMovementStateComponent
{
  public NpcMovementStateComponent(
    float waterMovementSpeed = 0.5f,
    float lavaMovementSpeed = 0.5f,
    float honeyMovementSpeed = 0.25f,
    float shimmerMovementSpeed = 0.375f)
  {
    WaterMovementSpeed = waterMovementSpeed;
    LavaMovementSpeed = lavaMovementSpeed;
    HoneyMovementSpeed = honeyMovementSpeed;
    ShimmerMovementSpeed = shimmerMovementSpeed;
    DirectionY = 1;
    OldDirectionY = 1;
    NoGravity = false;
    NoTileCollide = false;
    CollideX = false;
    CollideY = false;
    StepSpeed = 0.0f;
    StairFall = false;
    TeleportStyle = 0;
    TeleportTime = 0.0f;
    IsTeleporting = false;
    Breath = 200;
    BreathCounter = 0;
    LastPortalColorIndex = 0;
  }

  public float WaterMovementSpeed;
  public float LavaMovementSpeed;
  public float HoneyMovementSpeed;
  public float ShimmerMovementSpeed;
  public int DirectionY;
  public int OldDirectionY;
  public bool NoGravity;
  public bool NoTileCollide;
  public bool CollideX;
  public bool CollideY;
  public float StepSpeed;
  public bool StairFall;
  public int TeleportStyle;
  public float TeleportTime;
  public bool IsTeleporting;
  public int Breath;
  public int BreathCounter;
  public int LastPortalColorIndex;
}
