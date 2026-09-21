using System;

namespace Terraria.WorldSession.Components;

/// <summary>
/// Applies successful generation or altar selections to the saved-tier owner.
/// </summary>
public sealed class WorldSavedOreTierCommitSystem : ISavedOreTierCommitPort
{
  private readonly WorldSavedOreTierStateComponent _state;

  public WorldSavedOreTierCommitSystem(WorldSavedOreTierStateComponent state)
  {
    ArgumentNullException.ThrowIfNull(state);
    _state = state;
  }

  public void CommitGeneration(WorldSavedOreTierGenerationCommit commit)
  {
    ValidateTier(commit.Copper, nameof(commit.Copper));
    ValidateTier(commit.Iron, nameof(commit.Iron));
    ValidateTier(commit.Silver, nameof(commit.Silver));
    ValidateTier(commit.Gold, nameof(commit.Gold));

    OreTierState current = _state.Value;
    _state.Replace(new OreTierState(
      commit.Copper,
      commit.Iron,
      commit.Silver,
      commit.Gold,
      current.Cobalt,
      current.Mythril,
      current.Adamantite));
  }

  public void CommitAltar(WorldSavedOreTierAltarCommit commit)
  {
    ValidateTier(commit.Cobalt, nameof(commit.Cobalt));
    ValidateTier(commit.Mythril, nameof(commit.Mythril));
    ValidateTier(commit.Adamantite, nameof(commit.Adamantite));

    OreTierState current = _state.Value;
    _state.Replace(new OreTierState(
      current.Copper,
      current.Iron,
      current.Silver,
      current.Gold,
      commit.Cobalt,
      commit.Mythril,
      commit.Adamantite));
  }

  private static void ValidateTier(int value, string parameterName)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(value, parameterName);
  }
}
