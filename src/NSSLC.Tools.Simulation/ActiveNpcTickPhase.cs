using Terraria.NonAuthoritative.Simulation;
using Terraria.Npc;
using Terraria.Relationships;
using RuntimeMain = NSSLC.WorldGeneration.Main;

namespace Terraria.NonAuthoritative.SimulationHost;

internal sealed class ActiveNpcTickPhase : IWorldSimulationTickPhase
{
  private readonly RuntimeNpcStore _npcs;
  private readonly RuntimePlayerStore _players;
  private readonly RuntimeNpcNaturalSpawnPass _naturalSpawns;

  public ActiveNpcTickPhase(
    RuntimeNpcStore npcs,
    RuntimePlayerStore players,
    RuntimeNpcNaturalSpawnPass naturalSpawns)
  {
    _npcs = npcs ?? throw new ArgumentNullException(nameof(npcs));
    _players = players ?? throw new ArgumentNullException(nameof(players));
    _naturalSpawns = naturalSpawns ?? throw new ArgumentNullException(nameof(naturalSpawns));
  }

  public WorldSimulationPhase Phase => WorldSimulationPhase.Npc;

  public IReadOnlyList<EntityReference> NpcReferencesAtUpdateStart { get; private set; } =
    Array.Empty<EntityReference>();

  // A stop-only kernel step leaves this value at the last executed NPC phase tick.
  public long NpcUpdateTickNumberAtUpdateStart { get; private set; }

  public void Execute(WorldSimulationTickContext context)
  {
    // A client may observe replicated NPC state, but it must not run the
    // authoritative spawn, AI, movement, collision, or contact-damage pass.
    if (!NpcAiAuthorityGate.IsAuthoritative(RuntimeMain.netMode))
    {
      CaptureNpcReferencesAtUpdateStart(context.TickNumber);
      return;
    }

    _naturalSpawns.Update(context.TickNumber);
    CaptureNpcReferencesAtUpdateStart(context.TickNumber);
    _npcs.Update(context.TickNumber, context.Session, _players);
    _players.ApplyNpcContactDamage(context.TickNumber, _npcs.CreateContactSnapshots());
  }

  private void CaptureNpcReferencesAtUpdateStart(long tickNumber)
  {
    IReadOnlyList<RuntimeNpcEntity> activeNpcs = _npcs.CreateActiveSnapshot();
    var npcReferences = new EntityReference[activeNpcs.Count];
    for (int index = 0; index < activeNpcs.Count; index++)
    {
      RuntimeNpcEntity npc = activeNpcs[index];
      if (!_npcs.TryGetEntityReference(npc.InstanceId, out npcReferences[index]))
      {
        throw new InvalidOperationException(
          "An NPC scheduled for update has no runtime entity reference.");
      }
    }
    NpcReferencesAtUpdateStart = Array.AsReadOnly(npcReferences);
    NpcUpdateTickNumberAtUpdateStart = tickNumber;
  }
}
