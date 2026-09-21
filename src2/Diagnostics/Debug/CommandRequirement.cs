namespace Terraria.NonAuthoritative.Diagnostics;

[Flags]
public enum CommandRequirement
{
  None = 0,
  LocalPlayer = 1,
  Server = 2
}
