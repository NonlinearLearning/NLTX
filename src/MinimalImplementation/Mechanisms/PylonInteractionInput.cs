using System.Collections.ObjectModel;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.World;
using TileCoordinate = Terraria.WorldStorage.TileCoordinate;

namespace MinimalImplementation.Server.Mechanisms;

/// <summary>
/// Immutable Version4 pylon inputs assembled from one caller-owned commit window.
/// </summary>
/// <remarks>
/// The world scenes are checked against the supplied snapshot epoch/version and every registered
/// pylon must have exactly one scene. Player, NPC, pylon-registry and active-tile facts still must
/// be captured by their owners in the same exclusive ServerLoop window; this value cannot prove
/// that external capture discipline on their behalf.
/// </remarks>
public sealed class PylonInteractionInput
{
  public PylonInteractionInput(
    WorldSnapshot world,
    PylonPlayerFact player,
    byte targetType,
    TileCoordinate targetAnchor,
    IReadOnlyList<TeleportPylonSnapshot> pylons,
    IReadOnlyList<PylonNpcFact> npcs,
    IReadOnlyList<WorldSceneMetrics> scenes,
    bool downedPlantBoss,
    bool playerNearActivePylonTile)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(player);
    ArgumentNullException.ThrowIfNull(pylons);
    ArgumentNullException.ThrowIfNull(npcs);
    ArgumentNullException.ThrowIfNull(scenes);
    if (world.WorldEpoch == 0 || !world.Version.IsValid)
    {
      throw new ArgumentException(
        "Pylon inputs require a published world snapshot.", nameof(world));
    }

    TeleportPylonSnapshot[] pylonCopy = pylons.ToArray();
    PylonNpcFact[] npcCopy = npcs.ToArray();
    WorldSceneMetrics[] sceneCopy = scenes.ToArray();
    if (pylonCopy.Any(pylon => pylon is null) || npcCopy.Any(npc => npc is null) ||
        sceneCopy.Any(scene => scene is null))
    {
      throw new ArgumentException("Pylon inputs cannot contain null facts.");
    }

    var pylonAnchors = new HashSet<TileCoordinate>();
    foreach (TeleportPylonSnapshot pylon in pylonCopy)
    {
      if (!pylonAnchors.Add(pylon.Anchor))
      {
        throw new ArgumentException(
          "The pylon snapshot contains a duplicate anchor.", nameof(pylons));
      }
    }

    var environments = new Dictionary<TileCoordinate, PylonEnvironmentFacts>();
    foreach (WorldSceneMetrics scene in sceneCopy)
    {
      if (scene.WorldEpoch != world.WorldEpoch || scene.Version != world.Version)
      {
        throw new ArgumentException(
          "Every pylon scene must match the world snapshot epoch and version.", nameof(scenes));
      }

      var anchor = new TileCoordinate(scene.Anchor.X, scene.Anchor.Y);
      if (!pylonAnchors.Contains(anchor) ||
          !environments.TryAdd(anchor, PylonEnvironmentFacts.FromScene(scene, downedPlantBoss)))
      {
        throw new ArgumentException(
          "Pylon scenes must uniquely cover the registered pylon anchors.", nameof(scenes));
      }
    }
    if (environments.Count != pylonAnchors.Count)
    {
      throw new ArgumentException(
        "Every registered pylon requires an environment scene from the same world revision.",
        nameof(scenes));
    }

    WorldEpoch = world.WorldEpoch;
    WorldVersion = world.Version;
    Player = player;
    TargetType = targetType;
    TargetAnchor = targetAnchor;
    Pylons = Array.AsReadOnly(pylonCopy);
    Npcs = Array.AsReadOnly(npcCopy);
    Environments = new ReadOnlyDictionary<TileCoordinate, PylonEnvironmentFacts>(environments);
    PlayerNearActivePylonTile = playerNearActivePylonTile;
  }

  public ulong WorldEpoch { get; }

  public CommitVersion WorldVersion { get; }

  public PylonPlayerFact Player { get; }

  public byte TargetType { get; }

  public TileCoordinate TargetAnchor { get; }

  public IReadOnlyList<TeleportPylonSnapshot> Pylons { get; }

  public IReadOnlyList<PylonNpcFact> Npcs { get; }

  public IReadOnlyDictionary<TileCoordinate, PylonEnvironmentFacts> Environments { get; }

  public bool PlayerNearActivePylonTile { get; }
}
