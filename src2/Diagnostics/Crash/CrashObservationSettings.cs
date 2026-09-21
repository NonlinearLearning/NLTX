namespace Terraria.NonAuthoritative.Diagnostics;

public readonly record struct CrashObservationSettings(
  bool LogAllExceptions,
  bool DumpOnException,
  bool DumpOnCrash,
  string DumpPath);
