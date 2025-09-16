using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class TicTacToeCell : MonoBehaviour
{
  [SerializeField] private int row;
  [SerializeField] private int col;

  private TicTacToeBoard board;

  public void Initialize(TicTacToeBoard owner)
  {
    board = owner;
    board.RegisterCell(this, row, col);
    var img = GetComponent<Image>();
    if (img != null)
    {
      img.enabled = false; // hidden until placed
    }
  }

  private void OnTriggerEnter2D(Collider2D other)
  {
    if (other.CompareTag("TTBall"))
    {
      board?.PlaceMark(this);
    }
  }
}
