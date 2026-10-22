using MinimalImplementation.Contracts;
using MinimalImplementation.Server.World;

namespace MinimalImplementation.Server.Host;

/// <summary>
/// Tracks which section baselines have entered a session's ordered outbound stream.
/// Call these methods on the server commit loop; the lock protects against accidental off-loop reads.
/// </summary>
public sealed class WorldSectionDeliveryLedger
{
  private readonly object _gate = new();
  private readonly Dictionary<SessionEpoch, SessionSections> _sessions = [];

  /// <summary>Starts or continues section tracking for this session and published world.</summary>
  public bool BeginWorld(
    SessionEpoch sessionEpoch,
    ulong worldEpoch,
    WorldGeometry geometry)
  {
    if (!sessionEpoch.IsValid || worldEpoch == 0 ||
        geometry.Width <= 0 || geometry.Height <= 0)
    {
      return false;
    }

    lock (_gate)
    {
      if (!_sessions.TryGetValue(sessionEpoch, out SessionSections? session))
      {
        _sessions.Add(sessionEpoch, new SessionSections(worldEpoch, geometry));
        return true;
      }

      if (worldEpoch < session.WorldEpoch)
      {
        return false;
      }

      if (worldEpoch == session.WorldEpoch)
      {
        return session.Geometry == geometry;
      }

      _sessions[sessionEpoch] = new SessionSections(worldEpoch, geometry);
      return true;
    }
  }

  /// <summary>
  /// Records a complete packet-10 fragment group only when every fragment was queued or locally
  /// sent in sequence. A transport failure invalidates all section visibility for that session.
  /// </summary>
  public WorldSectionDeliveryResult RecordPacket10Delivery(
    SessionEpoch sessionEpoch,
    ulong worldEpoch,
    WorldSectionCoordinate section,
    CommitVersion version,
    IReadOnlyList<SendReceipt> fragmentReceipts)
  {
    ArgumentNullException.ThrowIfNull(fragmentReceipts);
    if (!sessionEpoch.IsValid || worldEpoch == 0 || !version.IsValid || fragmentReceipts.Count == 0)
    {
      return WorldSectionDeliveryResult.InvalidInput;
    }

    lock (_gate)
    {
      if (!_sessions.TryGetValue(sessionEpoch, out SessionSections? session) ||
          session.WorldEpoch != worldEpoch)
      {
        return WorldSectionDeliveryResult.StaleWorld;
      }

      if (!session.Geometry.ContainsSection(section))
      {
        return WorldSectionDeliveryResult.InvalidSection;
      }

      if (session.Sections.TryGetValue(section, out SectionDelivery? current) &&
          version.Value < current.Version.Value)
      {
        return WorldSectionDeliveryResult.StaleVersion;
      }

      bool allLocallySent = true;
      bool anyNotSubmitted = false;
      bool anySubmitted = false;
      long previousSequence = -1;
      foreach (SendReceipt receipt in fragmentReceipts)
      {
        if (receipt is null || receipt.Epoch != sessionEpoch ||
            receipt.Sequence.Value <= previousSequence)
        {
          return WorldSectionDeliveryResult.InvalidReceipt;
        }

        previousSequence = receipt.Sequence.Value;
        if (receipt.Certainty is TransportSendCertainty.Failed or
            TransportSendCertainty.OutcomeUnknown)
        {
          _sessions.Remove(sessionEpoch);
          return WorldSectionDeliveryResult.SessionInvalidated;
        }

        if (receipt.Certainty == TransportSendCertainty.NotSubmitted)
        {
          anyNotSubmitted = true;
          continue;
        }

        if (receipt.Certainty is not TransportSendCertainty.Queued and
            not TransportSendCertainty.LocallySent)
        {
          return WorldSectionDeliveryResult.InvalidReceipt;
        }

        anySubmitted = true;
        if (receipt.Certainty != TransportSendCertainty.LocallySent)
        {
          allLocallySent = false;
        }
      }

      if (anyNotSubmitted)
      {
        if (anySubmitted)
        {
          _sessions.Remove(sessionEpoch);
          return WorldSectionDeliveryResult.SessionInvalidated;
        }

        // Nothing from this attempt entered the ordered stream, so the last complete baseline
        // remains the session's visible state. Keep it until a newer complete group is submitted.
        return WorldSectionDeliveryResult.NotSubmitted;
      }

      WorldSectionDeliveryStatus status = allLocallySent
        ? WorldSectionDeliveryStatus.LocallySent
        : WorldSectionDeliveryStatus.Queued;
      session.Sections[section] = new SectionDelivery(version, status);
      return status == WorldSectionDeliveryStatus.LocallySent
        ? WorldSectionDeliveryResult.LocallySent
        : WorldSectionDeliveryResult.Queued;
    }
  }

  /// <summary>Returns sections whose complete baseline groups precede later queued packets.</summary>
  public IReadOnlyCollection<WorldSectionCoordinate> CaptureQueuedSections(
    SessionEpoch sessionEpoch,
    ulong worldEpoch)
  {
    lock (_gate)
    {
      if (!_sessions.TryGetValue(sessionEpoch, out SessionSections? session) ||
          session.WorldEpoch != worldEpoch)
      {
        return Array.Empty<WorldSectionCoordinate>();
      }

      return Array.AsReadOnly(session.Sections.Keys
        .OrderBy(coordinate => coordinate.Y)
        .ThenBy(coordinate => coordinate.X)
        .ToArray());
    }
  }

  /// <summary>Reports whether a complete section baseline was queued for this session.</summary>
  public bool HasQueuedSection(
    SessionEpoch sessionEpoch,
    ulong worldEpoch,
    WorldSectionCoordinate section)
  {
    lock (_gate)
    {
      return _sessions.TryGetValue(sessionEpoch, out SessionSections? session) &&
        session.WorldEpoch == worldEpoch &&
        session.Sections.ContainsKey(section);
    }
  }

  /// <summary>Reports whether all fragments of a section baseline reached the local socket.</summary>
  public bool HasLocallySentSection(
    SessionEpoch sessionEpoch,
    ulong worldEpoch,
    WorldSectionCoordinate section)
  {
    lock (_gate)
    {
      return _sessions.TryGetValue(sessionEpoch, out SessionSections? session) &&
        session.WorldEpoch == worldEpoch &&
        session.Sections.TryGetValue(section, out SectionDelivery? delivery) &&
        delivery.Status == WorldSectionDeliveryStatus.LocallySent;
    }
  }

  /// <summary>Removes all section state tied to a disconnected or retired session epoch.</summary>
  public void RemoveSession(SessionEpoch sessionEpoch)
  {
    lock (_gate)
    {
      _sessions.Remove(sessionEpoch);
    }
  }

  /// <summary>Removes only state belonging to the world epoch being unloaded.</summary>
  public void RemoveWorld(ulong worldEpoch)
  {
    if (worldEpoch == 0)
    {
      return;
    }

    lock (_gate)
    {
      SessionEpoch[] sessions = _sessions
        .Where(pair => pair.Value.WorldEpoch == worldEpoch)
        .Select(pair => pair.Key)
        .ToArray();
      foreach (SessionEpoch session in sessions)
      {
        _sessions.Remove(session);
      }
    }
  }

  private sealed class SessionSections(ulong worldEpoch, WorldGeometry geometry)
  {
    public ulong WorldEpoch { get; } = worldEpoch;

    public WorldGeometry Geometry { get; } = geometry;

    public Dictionary<WorldSectionCoordinate, SectionDelivery> Sections { get; } = [];
  }

  private sealed record SectionDelivery(
    CommitVersion Version,
    WorldSectionDeliveryStatus Status);
}

public enum WorldSectionDeliveryStatus
{
  Queued,
  LocallySent
}

public enum WorldSectionDeliveryResult
{
  InvalidInput,
  InvalidSection,
  InvalidReceipt,
  StaleWorld,
  StaleVersion,
  NotSubmitted,
  SessionInvalidated,
  Queued,
  LocallySent
}
