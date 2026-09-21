namespace Terraria.NonAuthoritative.Diagnostics;

public interface IDiagnosticLogOutput
{
  void Write(string line);
}
