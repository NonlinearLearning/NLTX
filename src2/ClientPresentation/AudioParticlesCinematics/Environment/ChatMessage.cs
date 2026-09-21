namespace NLTX.ClientPresentation.AudioParticlesCinematics.Environment;

public readonly record struct ChatMessage
{
  public ChatMessage(string Text, ChatMessageKind Kind)
  {
    if (string.IsNullOrWhiteSpace(Text))
    {
      throw new ArgumentException("Chat text is required.", nameof(Text));
    }

    this.Text = Text;
    this.Kind = Kind;
  }

  public string Text { get; }

  public ChatMessageKind Kind { get; }
}
