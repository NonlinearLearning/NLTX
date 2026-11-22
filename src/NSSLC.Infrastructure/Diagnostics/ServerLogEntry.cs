using System;

namespace Terraria.Diagnostics;

/// <summary>An immutable diagnostic entry handed to a server log sink.</summary>
public readonly record struct ServerLogEntry(
    DateTimeOffset Timestamp,
    ServerLogLevel Level,
    string EventName,
    string Details);
