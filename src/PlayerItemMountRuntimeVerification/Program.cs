using System.Numerics;
using Terraria.Player;

PlayerSceneMetricsSnapshot sceneMetrics = new(Revision: 7);
PlayerItemMountAndRuntimePropertiesInput input = new(
  MinionRestTargetPoint: new Vector2(1.0f, 2.0f),
  ItemTime: 0,
  ItemAnimation: 4,
  ItemAnimationMax: 5,
  ReuseDelay: 0,
  IsChanneling: false,
  HasPendingItemReuse: false,
  SceneMetrics: sceneMetrics,
  Position: new Vector2(10.0f, 20.0f),
  SpectatingCameraTarget: new PlayerSpectatingCameraTargetSnapshot(
    new Vector2(100.0f, 200.0f),
    GfxOffY: 3.0f,
    NetOffset: new Vector2(4.0f, 5.0f)),
  MountActive: true,
  MountIsSlime: true,
  WetSlime: 1,
  ControlJump: false,
  SelectedItemTypeId: 186,
  MountAllowsHeldItems: true,
  MountIsCart: false,
  MountCanGrindRails: true,
  OnTrack: true,
  MouthPositionOverride: new Vector2(9.0f, 10.0f),
  MouthPositionFallback: new Vector2(1.0f, 2.0f),
  HandPositionOverride: new Vector2(11.0f, 12.0f),
  HandPositionFallback: new Vector2(3.0f, 4.0f));

PlayerItemMountAndRuntimePropertiesSnapshot snapshot =
  PlayerItemMountAndRuntimePropertiesQuery.Evaluate(input);

Require(snapshot.HasMinionRestTarget, "A non-zero rest target must be reported.");
Require(snapshot.ItemTimeIsZero, "Zero item time must be reported.");
Require(snapshot.ItemAnimationJustStarted, "Animation max minus one must be the start boundary.");
Require(snapshot.UsingOrReusingItem, "An active animation must count as using or reusing an item.");
Require(snapshot.SceneMetrics == sceneMetrics, "Scene metrics must preserve the supplied snapshot.");
Require(
  snapshot.SpectatingCameraPosition == new Vector2(104.0f, 187.0f),
  "Spectating camera position must preserve bottom, gfx offset and network offset.");
Require(snapshot.SlimeDontHyperJump, "A wet slime mount without jump input must block hyper-jump.");
Require(snapshot.HasBreathingReed, "The breathing reed must be usable when the mount holds items.");
Require(snapshot.IsRidingTracks, "A grinding mount on a track must be riding tracks.");
Require(
  snapshot.MouthPosition == new Vector2(9.0f, 10.0f),
  "A mount mouth override must take precedence over the fallback.");
Require(
  snapshot.HandPosition == new Vector2(11.0f, 12.0f),
  "A mount hand override must take precedence over the fallback.");

PlayerItemMountAndRuntimePropertiesSnapshot fallbackSnapshot =
  PlayerItemMountAndRuntimePropertiesQuery.Evaluate(
    input with
    {
      MountAllowsHeldItems = false,
      MountIsCart = true,
      MouthPositionOverride = null,
      HandPositionOverride = null,
    });
Require(!fallbackSnapshot.HasBreathingReed, "A mount that hides held items must hide the reed.");
Require(fallbackSnapshot.IsRidingTracks, "A cart mount must count as riding tracks.");
Require(
  fallbackSnapshot.MouthPosition == new Vector2(1.0f, 2.0f),
  "The mouth fallback must be used when no mount override is supplied.");
Require(
  fallbackSnapshot.HandPosition == new Vector2(3.0f, 4.0f),
  "The hand fallback must be used when no mount override is supplied.");

PlayerItemMountAndRuntimePropertiesSnapshot localSnapshot =
  PlayerItemMountAndRuntimePropertiesQuery.Evaluate(
    input with
    {
      SpectatingCameraTarget = null,
      MountActive = false,
      MountIsSlime = false,
      MountAllowsHeldItems = false,
      MountIsCart = false,
      MountCanGrindRails = false,
      OnTrack = false,
      MouthPositionOverride = new Vector2(9.0f, 10.0f),
      HandPositionOverride = new Vector2(11.0f, 12.0f),
    });
Require(
  localSnapshot.SpectatingCameraPosition == input.Position,
  "Without a spectating target the camera must use the local position.");
Require(!localSnapshot.SlimeDontHyperJump, "An inactive mount cannot block hyper-jump.");
Require(localSnapshot.HasBreathingReed, "The reed is usable when no mount blocks held items.");
Require(!localSnapshot.IsRidingTracks, "An inactive mount cannot be riding tracks.");
Require(
  localSnapshot.MouthPosition == input.MouthPositionFallback,
  "An inactive mount must use the explicit mouth fallback.");
Require(
  localSnapshot.HandPosition == input.HandPositionFallback,
  "An inactive mount must use the explicit hand fallback.");

Console.WriteLine("PASS: player item, mount and runtime derived query");

static void Require(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}
