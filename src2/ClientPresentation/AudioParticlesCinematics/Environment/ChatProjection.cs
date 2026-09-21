namespace NLTX.ClientPresentation.AudioParticlesCinematics.Environment;

public sealed class ChatProjection
{
  public ChatMessage CreateSystemMessage(string text)
  {
    return new ChatMessage(text, ChatMessageKind.System);
  }

  public ChatMessage CreateWorldMessage(string text)
  {
    return new ChatMessage(text, ChatMessageKind.World);
  }
}
