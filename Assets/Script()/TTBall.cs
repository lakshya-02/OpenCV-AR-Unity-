using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TTBall : MonoBehaviour
{
  private void Reset()
  {
    gameObject.tag = "TTBall";
    var col = GetComponent<Collider2D>();
    col.isTrigger = true;
  }
}
