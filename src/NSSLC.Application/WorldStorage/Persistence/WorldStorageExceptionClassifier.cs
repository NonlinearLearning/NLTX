using System;
using System.IO;

namespace Terraria.NonAuthoritative.Persistence;

internal static class WorldStorageExceptionClassifier
{
  public static bool ContainsCancellation(Exception? exception)
  {
    if (exception is null)
    {
      return false;
    }

    if (exception is OperationCanceledException)
    {
      return true;
    }

    if (exception is AggregateException aggregate)
    {
      foreach (Exception innerException in aggregate.InnerExceptions)
      {
        if (ContainsCancellation(innerException))
        {
          return true;
        }
      }
    }

    return exception.InnerException is not null &&
      ContainsCancellation(exception.InnerException);
  }

  public static WorldStorageFailure ClassifyExternalFailure(
    Exception exception,
    WorldStorageFailureKind fallback)
  {
    ArgumentNullException.ThrowIfNull(exception);
    WorldStorageFailureKind kind = ContainsCancellation(exception)
      ? WorldStorageFailureKind.Canceled
      : exception switch
      {
        UnauthorizedAccessException => WorldStorageFailureKind.PermissionDenied,
        FileNotFoundException or DirectoryNotFoundException => WorldStorageFailureKind.Missing,
        PathTooLongException or ArgumentException or NotSupportedException =>
          WorldStorageFailureKind.InvalidPath,
        IOException => WorldStorageFailureKind.IoFailure,
        _ => fallback
      };

    return WorldStorageFailure.Create(kind, exception.Message);
  }
}
