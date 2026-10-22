namespace MinimalImplementation.Server.Session;

public enum SessionStage
{
  AwaitHello,
  AwaitPassword,
  AwaitPlayerData,
  AwaitSectionRequest,
  Synchronizing,
  Active,
  Closing,
  Closed
}
