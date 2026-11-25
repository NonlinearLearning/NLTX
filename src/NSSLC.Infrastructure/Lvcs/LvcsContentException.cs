namespace NSSLC.Infrastructure.Lvcs;

/// <summary>
/// Signals that content was rejected because the payload is invalid.
/// </summary>
/// <remarks>
/// Codecs throw this for failures the author of the content can fix. Programmer errors must not
/// use it: an uninitialised dependency or a null reference should surface as itself so it is not
/// mistaken for bad data.
/// <para>
/// A codec may report detailed, field level problems to <see cref="LvcsDiagnosticSink" /> before
/// throwing. The diagnostics carry the detail, this exception only carries the fact of failure.
/// </para>
/// </remarks>
public sealed class LvcsContentException : Exception
{
  public LvcsContentException(string message)
      : base(message)
  {
  }

  public LvcsContentException(string message, Exception innerException)
      : base(message, innerException)
  {
  }
}
