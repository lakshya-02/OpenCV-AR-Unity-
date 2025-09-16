using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TicTacToeBoard : MonoBehaviour
{
  public enum Player { X, O }

  [Header("Marks")]
  [SerializeField] private Sprite xSprite;
  [SerializeField] private Sprite oSprite;
  [SerializeField] private Color placedColor = Color.white;

  [Header("Win Line")]
  [SerializeField] private Image winLinePrefab; // simple UI image used as a line
  [SerializeField] private float winLineThickness = 8f;

  private Player currentPlayer = Player.X;
  private TicTacToeCell[,] cells = new TicTacToeCell[3, 3];
  private char[,] state = new char[3, 3]; // '\0', 'X', 'O'
  private RectTransform rect;
  private Image activeWinLine;

  private void Awake()
  {
    rect = GetComponent<RectTransform>();
    // auto-register children cells if not registered by prefab
    foreach (var cell in GetComponentsInChildren<TicTacToeCell>(true))
    {
      cell.Initialize(this);
    }
  }

  public void RegisterCell(TicTacToeCell cell, int row, int col)
  {
    cells[row, col] = cell;
    state[row, col] = '\0';
  }

  public bool PlaceMark(TicTacToeCell cell)
  {
    if (activeWinLine != null) return false; // game finished
    var (r, c) = FindCell(cell);
    if (r < 0) return false;
    if (state[r, c] != '\0') return false;

    state[r, c] = currentPlayer == Player.X ? 'X' : 'O';
    var img = cell.GetComponent<Image>();
    if (img != null)
    {
      img.sprite = currentPlayer == Player.X ? xSprite : oSprite;
      img.color = placedColor;
      img.enabled = true;
    }

    if (TryGetWin(out var winCells))
    {
      DrawWinLine(winCells[0].GetComponent<RectTransform>(), winCells[1].GetComponent<RectTransform>());
      return true;
    }

    if (IsBoardFull())
    {
      // optional: indicate draw
      return true;
    }

    currentPlayer = currentPlayer == Player.X ? Player.O : Player.X;
    return true;
  }

  private (int, int) FindCell(TicTacToeCell cell)
  {
    for (int r = 0; r < 3; r++)
      for (int c = 0; c < 3; c++)
        if (cells[r, c] == cell) return (r, c);
    return (-1, -1);
  }

  private bool TryGetWin(out List<TicTacToeCell> line)
  {
    line = new List<TicTacToeCell>(2);
    // rows
    for (int r = 0; r < 3; r++)
    {
      if (state[r, 0] != '\0' && state[r, 0] == state[r, 1] && state[r, 1] == state[r, 2])
      {
        line.Add(cells[r, 0]); line.Add(cells[r, 2]);
        return true;
      }
    }
    // cols
    for (int c = 0; c < 3; c++)
    {
      if (state[0, c] != '\0' && state[0, c] == state[1, c] && state[1, c] == state[2, c])
      {
        line.Add(cells[0, c]); line.Add(cells[2, c]);
        return true;
      }
    }
    // diag
    if (state[0, 0] != '\0' && state[0, 0] == state[1, 1] && state[1, 1] == state[2, 2])
    {
      line.Add(cells[0, 0]); line.Add(cells[2, 2]);
      return true;
    }
    if (state[0, 2] != '\0' && state[0, 2] == state[1, 1] && state[1, 1] == state[2, 0])
    {
      line.Add(cells[0, 2]); line.Add(cells[2, 0]);
      return true;
    }
    return false;
  }

  private bool IsBoardFull()
  {
    for (int r = 0; r < 3; r++)
      for (int c = 0; c < 3; c++)
        if (state[r, c] == '\0') return false;
    return true;
  }

  // Called via SendMessage from CounterFinder
  public void PlaceMarkAt(Vector2Int rc)
  {
    int r = Mathf.Clamp(rc.x, 0, 2);
    int c = Mathf.Clamp(rc.y, 0, 2);
    var cell = cells[r, c];
    if (cell != null)
    {
      PlaceMark(cell);
    }
  }

  private void DrawWinLine(RectTransform a, RectTransform b)
  {
    if (winLinePrefab == null) return;
    activeWinLine = Instantiate(winLinePrefab, rect);
    var lineRect = activeWinLine.rectTransform;
    Vector2 aPos = WorldToLocal(rect, a);
    Vector2 bPos = WorldToLocal(rect, b);
    Vector2 mid = (aPos + bPos) * 0.5f;
    float length = Vector2.Distance(aPos, bPos);
    float angle = Mathf.Atan2(bPos.y - aPos.y, bPos.x - aPos.x) * Mathf.Rad2Deg;

    lineRect.anchoredPosition = mid;
    lineRect.sizeDelta = new Vector2(length, winLineThickness);
    lineRect.localRotation = Quaternion.Euler(0, 0, angle);
    activeWinLine.enabled = true;
  }

  private static Vector2 WorldToLocal(RectTransform parent, RectTransform child)
  {
    Vector2 world;
    RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, RectTransformUtility.WorldToScreenPoint(null, child.position), null, out world);
    return world;
  }
}
