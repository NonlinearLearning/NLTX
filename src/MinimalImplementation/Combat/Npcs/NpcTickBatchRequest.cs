using MinimalImplementation.Combat.Identity;

namespace MinimalImplementation.Combat.Npcs;

/// <summary>All immutable per-frame inputs shared by one stable NPC capture.</summary>
public sealed class NpcTickBatchRequest {
  public NpcTickBatchRequest(
      NpcTickFrameContext frame,
      INpcTickMotionSnapshot motionSnapshot,
      INpcTickWorldRevisionGuard worldRevisionGuard,
      INpcAiRandom aiRandom,
      IReadOnlyList<NpcSlimeTickInput> slimes,
      IReadOnlyList<NpcEyeTickInput> eyes,
      IReadOnlyList<NpcZombieTickInput> zombies,
      IReadOnlyList<NpcGuideTickInput>? guides = null) {
    ArgumentNullException.ThrowIfNull(frame);
    ArgumentNullException.ThrowIfNull(motionSnapshot);
    ArgumentNullException.ThrowIfNull(worldRevisionGuard);
    ArgumentNullException.ThrowIfNull(aiRandom);
    ArgumentNullException.ThrowIfNull(slimes);
    ArgumentNullException.ThrowIfNull(eyes);
    ArgumentNullException.ThrowIfNull(zombies);

    Frame = frame;
    MotionSnapshot = motionSnapshot;
    WorldRevisionGuard = worldRevisionGuard;
    AiRandom = aiRandom;
    Slimes = Array.AsReadOnly(CopyInputs(slimes, nameof(slimes)));
    Eyes = Array.AsReadOnly(CopyInputs(eyes, nameof(eyes)));
    Zombies = Array.AsReadOnly(CopyInputs(zombies, nameof(zombies)));
    Guides = Array.AsReadOnly(CopyInputs(
        guides ?? Array.Empty<NpcGuideTickInput>(), nameof(guides)));
    EnsureUniqueEntities();
    EnsureWorldFactsMatchFrame();
  }

  public NpcTickFrameContext Frame { get; }

  public INpcTickMotionSnapshot MotionSnapshot { get; }

  public INpcTickWorldRevisionGuard WorldRevisionGuard { get; }

  public INpcAiRandom AiRandom { get; }

  public IReadOnlyList<NpcSlimeTickInput> Slimes { get; }

  public IReadOnlyList<NpcEyeTickInput> Eyes { get; }

  public IReadOnlyList<NpcZombieTickInput> Zombies { get; }

  public IReadOnlyList<NpcGuideTickInput> Guides { get; }

  private static TInput[] CopyInputs<TInput>(IReadOnlyList<TInput> inputs, string name)
      where TInput : class {
    TInput[] copy = inputs.ToArray();
    if (copy.Any(input => input is null)) {
      throw new ArgumentException("NPC tick input lists cannot contain null entries.", name);
    }
    return copy;
  }

  private void EnsureUniqueEntities() {
    var entities = new HashSet<CombatEntityIdentity>();
    foreach (NpcSlimeTickInput input in Slimes) {
      if (!entities.Add(input.Entity)) {
        throw new ArgumentException("A batch can contain only one input per NPC generation.");
      }
    }
    foreach (NpcEyeTickInput input in Eyes) {
      if (!entities.Add(input.Entity)) {
        throw new ArgumentException("A batch can contain only one input per NPC generation.");
      }
    }
    foreach (NpcZombieTickInput input in Zombies) {
      if (!entities.Add(input.Entity)) {
        throw new ArgumentException("A batch can contain only one input per NPC generation.");
      }
    }
    foreach (NpcGuideTickInput input in Guides) {
      if (!entities.Add(input.Entity)) {
        throw new ArgumentException("A batch can contain only one input per NPC generation.");
      }
    }
  }

  private void EnsureWorldFactsMatchFrame() {
    bool mismatch = Slimes.Any(input =>
        input.SourceWorldEpoch != Frame.WorldEpoch ||
        input.SourceWorldCommitVersion != Frame.WorldCommitVersion) ||
        Eyes.Any(input => input.SourceWorldEpoch != Frame.WorldEpoch ||
        input.SourceWorldCommitVersion != Frame.WorldCommitVersion) ||
        Zombies.Any(input => input.SourceWorldEpoch != Frame.WorldEpoch ||
        input.SourceWorldCommitVersion != Frame.WorldCommitVersion) ||
        Guides.Any(input => input.SourceWorldEpoch != Frame.WorldEpoch ||
        input.SourceWorldCommitVersion != Frame.WorldCommitVersion);
    if (mismatch) {
      throw new ArgumentException("Typed NPC facts must use the frame's world revision.");
    }
  }
}
