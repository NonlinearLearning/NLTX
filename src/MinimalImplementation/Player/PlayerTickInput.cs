using System;
using System.Collections.Generic;
using System.Linq;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Session;

namespace MinimalImplementation.Server.Player;

/// <summary>Facts captured for one active player on the server commit loop.</summary>
public sealed class PlayerTickInput
{
  public PlayerTickInput(
    SessionBinding binding,
    bool lanternsUp,
    int dayRate,
    IReadOnlyCollection<SessionEpoch> recipients)
  {
    ArgumentNullException.ThrowIfNull(binding);
    ArgumentNullException.ThrowIfNull(recipients);
    ArgumentOutOfRangeException.ThrowIfNegative(dayRate);
    Binding = binding;
    LanternsUp = lanternsUp;
    DayRate = dayRate;
    Recipients = Array.AsReadOnly(recipients.Where(epoch => epoch != binding.Epoch)
      .Distinct().ToArray());
  }

  public PlayerTickInput(
    SessionBinding binding,
    IPlayerTickWorldFacts worldFacts,
    IReadOnlyCollection<SessionEpoch> recipients)
    : this(binding,
      worldFacts?.LanternsUp ?? throw new ArgumentNullException(nameof(worldFacts)),
      worldFacts.DayRate,
      recipients)
  {
    if (worldFacts.Epoch != binding.Epoch || worldFacts.PlayerHandle != binding.PlayerHandle
      || !worldFacts.PlayerVersion.IsValid || !worldFacts.WorldRevision.IsValid)
    {
      throw new ArgumentException("World tick facts do not match the active Player binding.",
        nameof(worldFacts));
    }
    WorldFacts = worldFacts;
  }

  public SessionBinding Binding { get; }

  public bool LanternsUp { get; }

  public int DayRate { get; }

  public IReadOnlyList<SessionEpoch> Recipients { get; }

  public IPlayerTickWorldFacts? WorldFacts { get; }

  public OwnerCommitResult ValidateSources(Player player)
  {
    ArgumentNullException.ThrowIfNull(player);
    if (player.Epoch != Binding.Epoch || player.Handle != Binding.PlayerHandle)
    {
      return OwnerCommitResult.Rejected(ProtocolErrorCode.StaleEntity,
        "The tick input belongs to another Player binding.");
    }
    if (WorldFacts is null)
    {
      return OwnerCommitResult.Applied(player.Version);
    }
    if (WorldFacts.PlayerVersion != player.Version)
    {
      return new OwnerCommitResult(OwnerCommitStatus.Conflict, null, ProtocolErrorCode.Conflict,
        "The captured world tick facts belong to an older Player version.");
    }
    return WorldFacts.Validate();
  }
}
