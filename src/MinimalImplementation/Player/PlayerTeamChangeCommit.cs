using System;
using System.Collections.Generic;
using System.Linq;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Session;

namespace MinimalImplementation.Server.Player;

/// <summary>Committed team projections and the audience for P07's localized team feedback.</summary>
public sealed class PlayerTeamChangeCommit {
  internal PlayerTeamChangeCommit(
      OwnerCommitResult result,
      PlayerTeam team,
      string playerName,
      IReadOnlyCollection<SessionEpoch> notificationRecipients,
      IReadOnlyCollection<SessionPacketProjection> projections) {
    Result = result;
    Team = team;
    PlayerName = playerName;
    NotificationRecipients = Array.AsReadOnly(notificationRecipients.Distinct().ToArray());
    Projections = Array.AsReadOnly(projections.ToArray());
  }

  public OwnerCommitResult Result { get; }

  public PlayerTeam Team { get; }

  public string PlayerName { get; }

  public IReadOnlyList<SessionEpoch> NotificationRecipients { get; }

  public IReadOnlyList<SessionPacketProjection> Projections { get; }
}
