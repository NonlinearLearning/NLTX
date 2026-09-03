namespace Terraria.Player;

public struct InputIntentComponent
{
  public InputIntentComponent(
    bool moveLeft,
    bool moveRight,
    bool moveUp,
    bool moveDown,
    bool jump,
    bool useItem)
  {
    MoveLeft = moveLeft;
    MoveRight = moveRight;
    MoveUp = moveUp;
    MoveDown = moveDown;
    Jump = jump;
    UseItem = useItem;
  }

  public bool MoveLeft;
  public bool MoveRight;
  public bool MoveUp;
  public bool MoveDown;
  public bool Jump;
  public bool UseItem;
}
