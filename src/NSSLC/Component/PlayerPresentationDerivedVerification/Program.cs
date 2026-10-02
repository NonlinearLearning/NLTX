using System.Numerics;
using Terraria.Player;
using Terraria.Player.Presentation;

VerifyPoseAndAnimation();
VerifyNetworkCamera();
VerifyShadowPresentation();
VerifyVisualAndShaderEffects();
VerifyFootballPresentation();
VerifyAppearanceCustomization();
VerifyTraversalColorProjection();
VerifyCompanionAndEffectProjection();
VerifySpatialDerivedProperties();
VerifyIdentityAndDerivedProperties();
VerifyBiomeZoneProperties();
VerifyVerticalAndWeatherZoneProperties();
VerifyEventAndShoppingZoneProperties();
VerifyInteractionAndSelectionProperties();
VerifyAbilityAndPresentationProperties();

Console.WriteLine("PASS: P11 C01-C15 focused component verification");

static void VerifyPoseAndAnimation()
{
  PlayerPoseAndAnimationStateComponent component = new();
  PlayerPoseAndAnimationSystem system = new();
  PlayerPoseSnapshot snapshot = system.Update(
    component,
    new PlayerPoseInputSnapshot(
      Tick: new SimulationTick(1),
      HeadVelocityOverride: new Vector2(2.0f, 3.0f),
      BodyVelocityOverride: new Vector2(-4.0f, 5.0f),
      LegVelocityOverride: new Vector2(0.5f, -1.0f),
      FartKartCloudDelay: -1));

  Require(snapshot.HeadPosition == new Vector2(2.0f, 3.0f),
    "C01 must advance the head position from explicit velocity.");
  Require(snapshot.BodyRotation == -0.4f, "C01 must derive body rotation from velocity.");
  Require(snapshot.HeadVelocity == new Vector2(1.98f, 3.1f),
    "C01 must apply velocity damping and vertical acceleration.");
  Require(snapshot.FartKartCloudDelay == 0, "C01 must clamp a negative transient delay.");

  PlayerPoseSnapshot held = system.Update(
    component,
    new PlayerPoseInputSnapshot(Tick: new SimulationTick(2), AdvancePose: false));
  Require(held.HeadPosition == snapshot.HeadPosition,
    "C01 early return must preserve the pose position.");

  PlayerPoseSnapshot reset = system.ResetForTeleport(component, new SimulationTick(3));
  Require(reset.HeadPosition == Vector2.Zero && reset.BodyPosition == Vector2.Zero,
    "C01 teleport reset must clear pose positions.");
}

static void VerifyNetworkCamera()
{
  PlayerNetworkCameraStateComponent component = new();
  PlayerNetworkCameraStateSystem system = new();
  PlayerCameraSnapshot first = system.Update(
    component,
    new PlayerNetworkCameraInputSnapshot(
      Tick: new SimulationTick(1),
      Velocity: Vector2.Zero,
      CollisionAdjustedVelocity: Vector2.Zero,
      FakeNetOffset: new Vector2(10.0f, 0.0f),
      CameraTarget: new Vector2(4.0f, 5.0f)));

  Require(first.NetOffset == new Vector2(10.0f, 0.0f),
    "C02 must accept an explicit network offset snapshot.");
  Require(first.NetCameraTarget == new Vector2(4.0f, 5.0f),
    "C02 must preserve the camera target input.");

  system.MarkCameraTargetSynchronized(component);
  PlayerCameraSnapshot decayed = system.Update(
    component,
    new PlayerNetworkCameraInputSnapshot(
      Tick: new SimulationTick(2),
      Velocity: Vector2.Zero,
      CollisionAdjustedVelocity: Vector2.Zero));
  Require(decayed.NetOffset == new Vector2(8.0f, 0.0f),
    "C02 must decay a ten-unit offset by the configured minimum step.");
  Require(decayed.LastSyncedNetCameraTarget == new Vector2(4.0f, 5.0f),
    "C02 synchronization must snapshot the current camera target.");

  PlayerCameraSnapshot projected = new PlayerNetworkCameraProjection().Project(
    component,
    new SimulationTick(2));
  system.Update(
    component,
    new PlayerNetworkCameraInputSnapshot(
      Tick: new SimulationTick(2),
      Velocity: Vector2.Zero,
      CollisionAdjustedVelocity: Vector2.Zero,
      FakeNetOffset: new Vector2(4.0f, 0.0f)));
  Require(projected.NetOffset == new Vector2(8.0f, 0.0f),
    "C02 projection must not alias subsequent component changes.");

  PlayerCameraSnapshot cleared = system.Update(
    component,
    new PlayerNetworkCameraInputSnapshot(
      Tick: new SimulationTick(3),
      Velocity: Vector2.Zero,
      CollisionAdjustedVelocity: Vector2.Zero,
      ClearCameraTarget: true));
  Require(cleared.NetCameraTarget is null, "C02 must clear camera targets explicitly.");
  PlayerCameraSnapshot reset = system.ResetForSpawn(component, new SimulationTick(4));
  Require(reset.NetOffset == Vector2.Zero && reset.NetCameraTarget is null,
    "C02 spawn reset must clear transient camera state.");
}

static void VerifyShadowPresentation()
{
  PlayerShadowPresentationComponent component = new();
  PlayerShadowPresentationSystem system = new();
  PlayerShadowSnapshot first = system.Update(
    component,
    new PlayerShadowPresentationInput(
      Tick: 1,
      Position: new Vector2(10.0f, 20.0f),
      GfxOffY: 1.0f,
      FullRotation: 0.5f,
      FullRotationOrigin: new Vector2(2.0f, 3.0f),
      Direction: 1,
      AddAdvancedShadow: true,
      AdvancedShadow: new PlayerAdvancedShadowSlot(0, 1)));

  Require(first.Positions.Count == PlayerShadowPresentationComponent.SocialShadowCapacity,
    "C03 must expose the fixed social-shadow capacity.");
  Require(first.AdvancedShadows.Count ==
      PlayerShadowPresentationComponent.AdvancedShadowCapacity,
    "C03 must expose the fixed advanced-shadow ring capacity.");
  Require(first.AvailableAdvancedShadowsCount == 1,
    "C03 must count one advanced shadow after one insertion.");

  for (int index = 1; index < PlayerShadowPresentationComponent.AdvancedShadowCapacity;
    index++)
  {
    system.Update(
      component,
      new PlayerShadowPresentationInput(
        Tick: index + 1,
        Position: Vector2.Zero,
        GfxOffY: 0.0f,
        FullRotation: 0.0f,
        FullRotationOrigin: Vector2.Zero,
        Direction: 1,
        AddAdvancedShadow: true,
        AdvancedShadow: new PlayerAdvancedShadowSlot(0, index)));
  }

  PlayerShadowSnapshot full = component.ToSnapshot();
  Require(full.AvailableAdvancedShadowsCount ==
      PlayerShadowPresentationComponent.AdvancedShadowCapacity,
    "C03 must cap the advanced-shadow count at the ring capacity.");
  Require(full.AdvancedShadows[0].SourceRevision == 59,
    "C03 must retain the newest wrapped advanced-shadow value.");
  system.ResetForSpawn(component);
  Require(component.ToSnapshot().AvailableAdvancedShadowsCount == 0,
    "C03 spawn reset must clear advanced-shadow availability.");

  PlayerShadowSnapshot seeded = system.ResetForTeleport(
    component,
    new PlayerShadowPresentationInput(
      Tick: 100,
      Position: new Vector2(30.0f, 40.0f),
      GfxOffY: 2.0f,
      FullRotation: 0.75f,
      FullRotationOrigin: new Vector2(4.0f, 5.0f),
      Direction: -1));
  Require(seeded.ShadowCount == 0,
    "C03 lifecycle reset must reset the social-shadow sample cursor.");
  Require(seeded.Positions[0] == new Vector2(30.0f, 42.0f),
    "C03 lifecycle reset must seed the current social-shadow position.");
  Require(seeded.Rotations[0] == 0.75f && seeded.Origins[0] == new Vector2(4.0f, 5.0f),
    "C03 lifecycle reset must seed the current social-shadow rotation and origin.");
  Require(seeded.Directions[0] == -1,
    "C03 lifecycle reset must seed the current social-shadow direction.");

  PlayerShadowSnapshot removal = system.ResetForRemoval(component);
  Require(removal.ShadowCount == 0 && removal.AvailableAdvancedShadowsCount == 0 &&
      removal.Positions.All(position => position == Vector2.Zero) &&
      removal.Rotations.All(rotation => rotation == 0.0f) &&
      removal.Origins.All(origin => origin == Vector2.Zero) &&
      removal.Directions.All(direction => direction == 0) &&
      removal.AdvancedShadows.All(shadow => shadow == default) &&
      !removal.CursorItemIconReversed && removal.RunSoundDelay == 0 &&
      !removal.SkipAnimatingValuesInPlayerFrame,
    "C03 removal reset must clear all presentation shadow state.");
}

static void VerifyVisualAndShaderEffects()
{
  PlayerVisualAndShaderEffectsInput input = new(
    DontStarveShader: true,
    NoirShader: false,
    EyebrellaCloud: true,
    Yoraiz0rEye: 2,
    Yoraiz0rDarkness: true,
    HasUnicornHorn: false,
    HasAngelHalo: true,
    HasRainbowCursor: false,
    LeinforsHair: true,
    MusicBoxSilence: false,
    StardustMonolithShader: true,
    NebulaMonolithShader: false,
    VortexMonolithShader: true,
    SolarMonolithShader: false,
    MoonLordMonolithShader: true,
    BloodMoonMonolithShader: false,
    ShimmerMonolithShader: true,
    CrtMonolithShader: false,
    RetroMonolithShader: true,
    MusicBox: 7,
    OverrideFishingBobber: 9);
  PlayerVisualAndShaderEffectsSnapshot snapshot =
    new PlayerVisualAndShaderEffectsProjection().Project(input);

  Require(snapshot.DontStarveShader && snapshot.EyebrellaCloud && snapshot.Yoraiz0rEye == 2,
    "C04 must project shader flags and values.");
  Require(snapshot.StardustMonolithShader && snapshot.VortexMonolithShader &&
      snapshot.MoonLordMonolithShader && snapshot.RetroMonolithShader,
    "C04 must preserve monolith shader flags.");
  Require(snapshot.MusicBox == 7 && snapshot.OverrideFishingBobber == 9,
    "C04 must preserve explicit audio and fishing inputs.");
}

static void VerifyFootballPresentation()
{
  PlayerFootballPresentationStateComponent component = new();
  PlayerFootballPresentationStateSystem system = new();
  PlayerFootballPresentationSnapshot drawing = system.Update(
    component,
    new PlayerFootballPresentationInput(HasFootball: true, CanDrawFootball: true));
  Require(drawing.HasFootball && drawing.IsDrawingFootball,
    "C05 must draw a held football when the draw gate is open.");

  PlayerFootballPresentationSnapshot suppressed = system.Update(
    component,
    new PlayerFootballPresentationInput(
      HasFootball: true,
      CanDrawFootball: true,
      IsFootballItemAnimating: true));
  Require(suppressed.HasFootball && !suppressed.IsDrawingFootball,
    "C05 must suppress drawing while the football item animates.");
  PlayerFootballPresentationSnapshot lost = system.ResetForItemLoss(component);
  Require(!lost.HasFootball && !lost.IsDrawingFootball,
    "C05 item-loss reset must clear possession and draw state.");
}

static void VerifyAppearanceCustomization()
{
  PlayerAppearanceCustomizationComponent component = new();
  PlayerAppearanceCustomizationSystem system = new();
  Require(component.HairColor == new PlayerAppearanceColor(215, 90, 55),
    "C06 must use the documented default hair color.");
  Require(component.SkinColor == new PlayerAppearanceColor(255, 125, 90),
    "C06 must use the documented default skin color.");

  PlayerAppearanceCustomizationInput input = new(
    HairDye: 3,
    SkinDyePacked: 0x102030,
    HairColor: new PlayerAppearanceColor(1, 2, 3),
    SkinColor: new PlayerAppearanceColor(4, 5, 6),
    EyeColor: new PlayerAppearanceColor(7, 8, 9),
    ShirtColor: new PlayerAppearanceColor(10, 11, 12),
    UnderShirtColor: new PlayerAppearanceColor(13, 14, 15),
    PantsColor: new PlayerAppearanceColor(16, 17, 18),
    ShoeColor: new PlayerAppearanceColor(19, 20, 21),
    Hair: 42);
  PlayerAppearanceCustomizationSnapshot snapshot = system.Update(component, input);
  Require(snapshot.HairDye == 3 && snapshot.SkinDyePacked == 0x102030 && snapshot.Hair == 42,
    "C06 must commit all scalar customization fields.");
  Require(snapshot.EyeColor == input.EyeColor && snapshot.ShoeColor == input.ShoeColor,
    "C06 must commit all color value objects.");
}

static void VerifyTraversalColorProjection()
{
  PlayerTraversalColorSnapshot snapshot = new PlayerTraversalColorProjection().Project(
    new PlayerTraversalColorInput(
      Wings: 1,
      Carpet: 2,
      FloatingTube: 3,
      Grapple: 4,
      Mount: 5,
      Minecart: 6));
  Require(snapshot == new PlayerTraversalColorSnapshot(1, 2, 3, 4, 5, 6),
    "C07 must preserve traversal color IDs in report order.");
}

static void VerifyCompanionAndEffectProjection()
{
  PlayerAppearanceCompanionAndEffectSnapshot snapshot =
    new PlayerAppearanceCompanionAndEffectProjection().Project(
      new PlayerAppearanceCompanionAndEffectInput(
        Pet: 1,
        Light: 2,
        Yorai: 3,
        PortableStool: 4,
        UnicornHorn: 5,
        AngelHalo: 6,
        Beard: 7,
        Minion: 8,
        LeinShampoo: 9,
        FlameWaker: 10,
        Coat: 11));
  Require(snapshot == new PlayerAppearanceCompanionAndEffectSnapshot(1, 2, 3, 4, 5, 6, 7, 8, 9,
      10, 11),
    "C08 must preserve companion and effect IDs in report order.");
}

static void VerifySpatialDerivedProperties()
{
  PlayerSpatialSnapshot snapshot = new(
    Position: new Vector2(100.0f, 200.0f),
    Velocity: new Vector2(0.049f, -0.049f),
    Width: 20,
    Height: 40,
    MountActive: false,
    MountPlayerOffsetHitbox: 99.0f,
    MountHeightBoost: 50,
    PortableStoolInUse: true,
    PortableStoolHeightBoost: 8,
    PortableStoolVisualYOffset: 1.5f,
    GfxOffY: 2.0f,
    Frozen: false,
    Webbed: true,
    Stoned: false);

  Require(PlayerSpatialDerivedPropertiesQuery.IsConsideredStandingStill(snapshot),
    "C09 must use a strict 0.05 standing-still threshold.");
  Require(PlayerSpatialDerivedPropertiesQuery.HeightOffsetBoost(snapshot) == 8,
    "C09 must use the portable-stool height boost when no mount is active.");
  Require(PlayerSpatialDerivedPropertiesQuery.MountedCenter(snapshot) ==
      new Vector2(110.0f, 222.5f),
    "C09 must combine base height and the stool hitbox offset.");
  Require(PlayerSpatialDerivedPropertiesQuery.VisualPosition(snapshot) ==
      new Vector2(100.0f, 202.0f),
    "C09 must apply the graphics Y offset to visual position.");
  Require(PlayerSpatialDerivedPropertiesQuery.CCed(snapshot),
    "C09 must report crowd control from frozen, webbed, or stoned inputs.");
  Require(PlayerSpatialDerivedPropertiesQuery.HitboxForBestiaryNearbyCheck(snapshot) ==
      new PlayerSpatialHitbox(-200, 0, 620, 440),
    "C09 must inflate the integer position hitbox by the documented bounds.");
  Require(!PlayerSpatialDerivedPropertiesQuery.IsConsideredStandingStill(
      snapshot with { Velocity = new Vector2(0.05f, 0.0f) }),
    "C09 must reject the exact standing-still boundary.");
}

static void VerifyIdentityAndDerivedProperties()
{
  PlayerIdentityAndDerivedPropertiesSnapshot snapshot =
    PlayerIdentityAndDerivedPropertiesQuery.Evaluate(
      new PlayerIdentityAndDerivedPropertiesInput(MiscCounter: 600, IsMale: true));
  Require(snapshot.MiscCounterNormalized == 2.0f && snapshot.IsMale,
    "C10 must normalize miscCounter by 300 and preserve current identity.");

  PlayerMaleChangeCommand command = new(HasChange: true, SkinVariant: 17);
  Require(command.HasChange && command.SkinVariant == 17,
    "C10 must represent a male transition as an explicit command intent.");
}

static void VerifyBiomeZoneProperties()
{
  PlayerZoneSnapshot input = new(
    ZoneDungeon: true,
    ZoneCorrupt: false,
    ZoneHallow: true,
    ZoneMeteor: false,
    ZoneJungle: true,
    ZoneSnow: false,
    ZoneCrimson: true,
    ZoneWaterCandle: false,
    ZonePeaceCandle: true,
    ZoneTowerSolar: false,
    ZoneTowerVortex: true,
    ZoneTowerNebula: false,
    ZoneTowerStardust: true,
    ZoneDesert: false,
    ZoneGlowshroom: true,
    ZoneUndergroundDesert: false);
  PlayerBiomeZonePropertiesSnapshot snapshot = PlayerBiomeZonePropertiesQuery.Evaluate(input);
  Require(snapshot == new PlayerBiomeZonePropertiesSnapshot(
      true, false, true, false, true, false, true, false, true, false, true, false, true, false,
      true, false),
    "C11 must preserve all sixteen zone slots in report order.");
  Require(new PlayerZoneFlagChangeCommand(PlayerZoneFlag.Desert, true).Enabled,
    "C11 must expose zone writes as explicit command intents.");
}

static void VerifyVerticalAndWeatherZoneProperties()
{
  PlayerVerticalAndWeatherZoneInput input = new(
    ZoneSkyHeight: true,
    ZoneOverworldHeight: false,
    ZoneUnderworldHeight: true,
    ZoneBeach: false,
    ZoneRain: true,
    ZoneSandstorm: false);
  PlayerVerticalAndWeatherZonePropertiesSnapshot snapshot =
    PlayerVerticalAndWeatherZonePropertiesQuery.Evaluate(input);
  Require(snapshot == new PlayerVerticalAndWeatherZonePropertiesSnapshot(true, false, true, false,
      true, false),
    "C12 must preserve the non-contiguous zone3 mapping.");
  Require(new PlayerVerticalAndWeatherZoneFlagChangeCommand(
      PlayerVerticalAndWeatherZoneFlag.Rain,
      false).Flag == PlayerVerticalAndWeatherZoneFlag.Rain,
    "C12 must expose weather writes as explicit command intents.");
}

static void VerifyEventAndShoppingZoneProperties()
{
  PlayerEventAndShoppingZoneInput boundary = new(
    ZoneOldOneArmy: true,
    ZoneLihzhardTemple: false,
    ZoneGraveyard: true,
    ZoneShadowCandle: false,
    ZoneShimmer: true,
    ZoneDungeon: false,
    ZoneCorrupt: false,
    ZoneCrimson: false,
    ZoneGlowshroom: false,
    ZoneHallow: false,
    ZoneJungle: false,
    ZoneSnow: false,
    ZoneBeach: false,
    ZoneDesert: false,
    Position: new Vector2(0.0f, 1600.0f),
    WorldSurface: 100.0);
  PlayerEventAndShoppingZonePropertiesSnapshot atSurface =
    PlayerEventAndShoppingZonePropertiesQuery.Evaluate(boundary);
  Require(!atSurface.ShoppingZoneAnyBiome && !atSurface.ShoppingZoneBelowSurface,
    "C13 must require a biome and a strict below-surface comparison.");

  PlayerEventAndShoppingZonePropertiesSnapshot below =
    PlayerEventAndShoppingZonePropertiesQuery.Evaluate(
      boundary with
      {
        ZoneDesert = true,
        Position = new Vector2(0.0f, 1600.1f),
      });
  Require(below.ShoppingZoneAnyBiome && below.ShoppingZoneBelowSurface,
    "C13 must recognize the desert fallback and below-surface position.");
  Require(new PlayerEventZoneFlagChangeCommand(PlayerEventZoneFlag.Shimmer, false).Flag ==
      PlayerEventZoneFlag.Shimmer,
    "C13 must expose event and shimmer writes as explicit command intents.");
}

static void VerifyInteractionAndSelectionProperties()
{
  ItemEntityRef firstItem = new(Guid.Parse("11111111-1111-1111-1111-111111111111"));
  ItemEntityRef secondItem = new(Guid.Parse("22222222-2222-2222-2222-222222222222"));
  IReadOnlyList<ItemEntityRef> inventory = new[] { firstItem, secondItem };
  PlayerInteractionAndSelectionPropertiesInput input = new(
    Direction: -1,
    GravityDirection: -1.0f,
    SelectedItem: 1,
    Inventory: inventory,
    CanFloatInWater: true,
    ControlDown: false,
    MountActive: true,
    MountType: new ContentId<MountDefinition>(37),
    Active: true,
    Dead: false,
    ShouldNotDraw: false,
    Stealth: 1.0f,
    IsVoidVaultEnabled: true,
    Position: new Vector2(10.0f, 20.0f),
    NetCameraTarget: new Vector2(100.0f, 200.0f),
    ControlUp: false,
    TryKeepingHoveringUp: true,
    TryKeepingHoveringDown: true);
  PlayerInteractionAndSelectionPropertiesSnapshot snapshot =
    PlayerInteractionAndSelectionPropertiesQuery.Evaluate(input);

  Require(snapshot.Directions == new Vector2(-1.0f, -1.0f),
    "C14 must preserve direction and gravity direction.");
  Require(snapshot.HeldItem == secondItem && snapshot.SelectedItem == 1,
    "C14 must select the requested held item without copying item authority.");
  Require(snapshot.ShouldFloatInWater && snapshot.CanBeTalkedTo,
    "C14 must apply floating and talk gating from explicit inputs.");
  Require(snapshot.ReportedCameraPosition == new Vector2(100.0f, 200.0f) &&
      snapshot.TryingToHoverUp && snapshot.TryingToHoverDown,
    "C14 must prefer the network camera and combine hover inputs.");

  PlayerInteractionAndSelectionPropertiesSnapshot fallback =
    PlayerInteractionAndSelectionPropertiesQuery.Evaluate(
      input with { SelectedItem = -1, NetCameraTarget = null, MountType = new ContentId<MountDefinition>(99) });
  Require(fallback.HeldItem.IsEmpty && fallback.ReportedCameraPosition == input.Position,
    "C14 must return an empty item for an invalid slot and fall back to position.");
  Require(!fallback.ShouldFloatInWater,
    "C14 must reject floating for a non-floating active mount.");
  Require(new PlayerVoidVaultStateChangeCommand(true).Enabled,
    "C14 must expose vault mutation as an explicit command intent.");
}

static void VerifyAbilityAndPresentationProperties()
{
  PlayerAbilityAndPresentationPropertiesInput input = new(
    UnlockedBiomeTorches: true,
    BiomeTorchPreferenceEnabled: true,
    UnlockedSuperCart: true,
    EnabledSuperCart: true,
    RangedDamage: 10.0f,
    RangedMultDamage: 2.0f,
    ArrowDamageAdditiveStack: 0.5f,
    ArrowDamage: 3.0f,
    BulletDamage: 1.5f,
    RocketDamage: 2.0f,
    IsPerformingJumpDownDash: false,
    IsMerman: false,
    IsInvisible: true,
    ItemAnimation: 0,
    IsDisplayDollOrInanimate: false,
    IsHatRackDoll: false,
    TalkNpc: 7,
    IsSitting: true,
    IsSleeping: false,
    PortalPhysicsRemainingTicks: 4,
    MountActive: false,
    Life: 50,
    MaximumLife: 100,
    IsWet: false,
    IsLavaWet: false,
    IsHoneyWet: false,
    IsDripping: false,
    MountFishronSpecialCounter: 0.0f,
    IsRaining: true,
    IsInPlaceWithWind: true);
  PlayerAbilityAndPresentationPropertiesSnapshot snapshot =
    PlayerAbilityAndPresentationPropertiesQuery.Evaluate(input);

  Require(snapshot.UsingBiomeTorches && snapshot.UsingSuperCart,
    "C15 must apply unlock and preference gates.");
  Require(snapshot.BowEffectiveDamage == 33.0f && snapshot.GunEffectiveDamage == 15.0f &&
      snapshot.SpecialistEffectiveDamage == 20.0f,
    "C15 must preserve the Version4 effective-damage formulas.");
  Require(snapshot.CanUseBootFlyingAbilities && snapshot.CanUseWingAbilities && snapshot.ShouldNotDraw,
    "C15 must apply dash, merman, and draw gates.");
  Require(snapshot.TalkNpc == 7 && snapshot.IsLockedToATile && snapshot.PortalPhysicsEnabled,
    "C15 must preserve talk, rest, and portal outputs.");
  Require(snapshot.MountFishronSpecial,
    "C15 must allow the Fishron special when all explicit conditions are satisfied.");

  PlayerAbilityAndPresentationPropertiesSnapshot blocked =
    PlayerAbilityAndPresentationPropertiesQuery.Evaluate(
      input with
      {
        IsPerformingJumpDownDash = true,
        IsMerman = true,
        ItemAnimation = 1,
        MountActive = true,
        IsRaining = false,
      });
  Require(!blocked.CanUseBootFlyingAbilities && !blocked.CanUseWingAbilities &&
      !blocked.ShouldNotDraw && !blocked.PortalPhysicsEnabled,
    "C15 must close ability and draw gates when owner facts block them.");
  Require(!PlayerAbilityAndPresentationPropertiesQuery.Evaluate(
      input with { IsRaining = false }).MountFishronSpecial,
    "C15 must require rain and wind for the Fishron special path.");
  Require(PlayerAbilityAndPresentationPropertiesQuery.Evaluate(input) == snapshot,
    "C15 query evaluation must remain deterministic and side-effect free.");
}

static void Require(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}
