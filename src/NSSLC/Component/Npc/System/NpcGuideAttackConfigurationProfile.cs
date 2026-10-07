using System.Numerics;

namespace Terraria.Npc;

public readonly record struct NpcGuideAttackConfigurationInput(
  int TypeId,
  int NetId,
  int AiStyle,
  bool HardMode,
  float AttackTime,
  float AttackTimer,
  float LocalAi3,
  float DamageScale,
  bool ServerAuthority,
  Vector2 Velocity = default,
  Vector2 NpcCenter = default,
  int SpriteDirection = 1,
  bool SelectedTargetAvailable = false,
  Vector2 SelectedTargetCenter = default,
  bool DangerWithinBaseRange = false,
  float LocalAi2 = 0f);

public readonly record struct NpcGuideAttackConfiguration(
  int ProjectileType,
  float ProjectileSpeed,
  int BaseDamage,
  int AttackDamage,
  int SpawnTick,
  int CooldownBase,
  int CooldownRandomExclusive,
  float Knockback,
  int AimOffsetY,
  float Spread);

public readonly record struct NpcGuideProjectileSpawnRequest(
  Vector2 Position,
  Vector2 Velocity,
  int ProjectileType,
  int Damage,
  float Knockback,
  bool NpcProjectile,
  bool NoDropItem);

[Flags]
public enum NpcGuideAttackConfigurationBranch
{
  None = 0,
  HardMode = 1 << 0,
  NormalMode = 1 << 1,
  AttackTimerDecremented = 1 << 2,
  SpawnTickReached = 1 << 3,
  FrameReset = 1 << 4,
  CycleCompletion = 1 << 5,
  ProjectileVelocityFallback = 1 << 6,
}

public readonly record struct NpcGuideAttackConfigurationResult(
  NpcGuideAttackConfiguration Configuration,
  float RemainingAttackTimer,
  float LocalAi3,
  bool FrameResetRequested,
  bool ProjectileSpawnWindowOpen,
  bool CycleCompletionReached,
  NpcGuideAttackConfigurationBranch Branches,
  Vector2 Velocity = default,
  bool ProjectileSpawnRequested = false,
  NpcGuideProjectileSpawnRequest ProjectileSpawn = default,
  float NextLocalAi1 = 0f,
  NpcGuideSourceProfileState NextState = default,
  bool NetworkUpdateRequested = false);

public interface INpcGuideAttackRandomPort
{
  int Next(int maxExclusive);

  float NextSingle(float minInclusive, float maxExclusive);
}

public interface INpcGuideAttackEffectPort
{
  void SpawnProjectile(in NpcGuideProjectileSpawnRequest request);

  void RequestNetworkSync();
}

/// <summary>
/// Source-exact Guide type=22 configuration and launch result for ai[0] == 12.
/// Projectile creation remains an explicit effect-port operation; this profile
/// does not access the projectile store or network host directly.
/// </summary>
public static class NpcGuideAttackConfigurationProfile
{
  public static NpcGuideAttackConfigurationResult Evaluate(
    in NpcGuideAttackConfigurationInput input)
  {
    if (!NpcGuideSourceProfile.CanHandle(input.TypeId, input.NetId, input.AiStyle))
    {
      throw new InvalidOperationException(
        "Guide attack configuration requires type=22, netID=22, and aiStyle=7.");
    }

    if (input.DamageScale < 0f)
    {
      throw new ArgumentOutOfRangeException(
        nameof(input.DamageScale),
        "Guide town-NPC damage scale cannot be negative.");
    }

    int baseDamage = input.HardMode ? 18 : 12;
    NpcGuideAttackConfiguration configuration = new(
      ProjectileType: input.HardMode ? 2 : 1,
      ProjectileSpeed: 10f,
      BaseDamage: baseDamage,
      AttackDamage: (int)(baseDamage * input.DamageScale),
      SpawnTick: 1,
      CooldownBase: input.HardMode ? 15 : 30,
      CooldownRandomExclusive: input.HardMode ? 10 : 20,
      Knockback: 2.75f,
      AimOffsetY: 4,
      Spread: 0.7f);

    float remainingAttackTimer = input.AttackTimer - 1f;
    float localAi3 = input.LocalAi3 + 1f;
    NpcGuideAttackConfigurationBranch branches = input.HardMode
      ? NpcGuideAttackConfigurationBranch.HardMode
      : NpcGuideAttackConfigurationBranch.NormalMode;
    branches |= NpcGuideAttackConfigurationBranch.AttackTimerDecremented;
    bool frameResetRequested = input.AttackTimer == input.AttackTime;
    if (frameResetRequested)
    {
      branches |= NpcGuideAttackConfigurationBranch.FrameReset;
    }

    bool projectileSpawnWindowOpen = input.ServerAuthority &&
      localAi3 == configuration.SpawnTick;
    if (projectileSpawnWindowOpen)
    {
      branches |= NpcGuideAttackConfigurationBranch.SpawnTickReached;
    }

    bool cycleCompletionReached = remainingAttackTimer <= 0f;
    if (cycleCompletionReached)
    {
      branches |= NpcGuideAttackConfigurationBranch.CycleCompletion;
    }

    return new NpcGuideAttackConfigurationResult(
      configuration,
      remainingAttackTimer,
      localAi3,
      frameResetRequested,
      projectileSpawnWindowOpen,
      cycleCompletionReached,
      branches,
      Velocity: new Vector2(input.Velocity.X * 0.8f, input.Velocity.Y));
  }

  public static NpcGuideAttackConfigurationResult EvaluateWithRandom(
    in NpcGuideAttackConfigurationInput input,
    INpcGuideAttackRandomPort randomPort)
  {
    ArgumentNullException.ThrowIfNull(randomPort);
    NpcGuideAttackConfigurationResult result = Evaluate(in input);
    NpcGuideAttackConfigurationBranch branches = result.Branches;
    bool projectileSpawnRequested = false;
    NpcGuideProjectileSpawnRequest projectileSpawn = default;
    if (result.ProjectileSpawnWindowOpen)
    {
      Vector2 aim = Vector2.Zero;
      if (input.SelectedTargetAvailable)
      {
        Vector2 delta = input.SelectedTargetCenter +
          new Vector2(0f, -result.Configuration.AimOffsetY) -
          input.NpcCenter;
        float distance = delta.Length();
        if (distance > 0f)
        {
          aim = delta / distance;
        }
      }

      if (aim.LengthSquared() == 0f ||
          MathF.Sign(aim.X) != input.SpriteDirection)
      {
        aim = new Vector2(input.SpriteDirection, 0f);
        branches |= NpcGuideAttackConfigurationBranch.ProjectileVelocityFallback;
      }

      aim *= result.Configuration.ProjectileSpeed;
      aim.X += randomPort.NextSingle(
        -result.Configuration.Spread,
        result.Configuration.Spread);
      aim.Y += randomPort.NextSingle(
        -result.Configuration.Spread,
        result.Configuration.Spread);
      projectileSpawn = new NpcGuideProjectileSpawnRequest(
        input.NpcCenter + new Vector2(input.SpriteDirection * 16f, -2f),
        aim,
        result.Configuration.ProjectileType,
        result.Configuration.AttackDamage,
        result.Configuration.Knockback,
        NpcProjectile: true,
        NoDropItem: true);
      projectileSpawnRequested = true;
    }

    NpcGuideSourceProfileState nextState = default;
    float nextLocalAi1 = 0f;
    bool networkUpdateRequested = false;
    if (result.CycleCompletionReached)
    {
      int cooldownRoll = randomPort.Next(
        result.Configuration.CooldownRandomExclusive);
      int localRoll = randomPort.Next(
        result.Configuration.CooldownRandomExclusive);
      float nextAi0 = input.LocalAi2 == 8f && input.DangerWithinBaseRange
        ? 8f
        : 0f;
      float nextAi1 = result.Configuration.CooldownBase + cooldownRoll;
      float nextLocalAi3 = result.Configuration.CooldownBase / 2f + localRoll;
      nextLocalAi1 = nextLocalAi3;
      nextState = new NpcGuideSourceProfileState(
        nextAi0,
        nextAi1,
        0f,
        nextLocalAi3,
        input.LocalAi2);
      networkUpdateRequested = true;
    }

    return result with
    {
      Branches = branches,
      ProjectileSpawnRequested = projectileSpawnRequested,
      ProjectileSpawn = projectileSpawn,
      NextLocalAi1 = nextLocalAi1,
      NextState = nextState,
      NetworkUpdateRequested = networkUpdateRequested,
    };
  }

  public static void ApplyEffects(
    in NpcGuideAttackConfigurationResult result,
    INpcGuideAttackEffectPort effectPort)
  {
    ArgumentNullException.ThrowIfNull(effectPort);
    if (result.ProjectileSpawnRequested)
    {
      NpcGuideProjectileSpawnRequest projectileSpawn = result.ProjectileSpawn;
      effectPort.SpawnProjectile(in projectileSpawn);
    }

    if (result.NetworkUpdateRequested)
    {
      effectPort.RequestNetworkSync();
    }
  }
}
