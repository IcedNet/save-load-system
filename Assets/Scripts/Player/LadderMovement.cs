using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;

public class LadderMovement : MonoBehaviour
{
    private float vertical;
    private float speed = 8f;
    private bool isLadder;
    private bool isClimbing;
    private DebugLogger _log = new DebugLogger(nameof(LadderMovement));

    [SerializeField]
    private Rigidbody2D rb;
    private readonly string LADDER_TAG = "Ladder";
    private readonly float GRAVITY_DEFAULT = 2.0f;

    void Update()
    {
        Vector2 moveDirection = InputManager.instance.GetMoveDirection();
        vertical = moveDirection.y;
        if (isLadder && Mathf.Abs(vertical) > 0)
        {
            _log.Log(
                ".Update if (isLadder && Mathf.Abs(vertical) > 0) isClimbing = true -- moveDirection = "
                    + moveDirection.ToString()
                    + " -- moveDirection.y"
                    + moveDirection.y
            );
            isClimbing = true;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    private void FixedUpdate()
    {
        if (isClimbing)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, vertical * speed);
        }
        else
        {
            rb.gravityScale = GRAVITY_DEFAULT;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        _log.Log(".OnTriggerEnter2D collision.gameObject.tag = " + collision.gameObject.tag);
        if (collision.CompareTag(LADDER_TAG))
        {
            isLadder = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        _log.Log(".OnTriggerExit2D collision.gameObject.tag = " + collision.gameObject.tag);
        if (collision.CompareTag(LADDER_TAG))
        {
            isLadder = false;
            isClimbing = false;
            rb.bodyType = RigidbodyType2D.Dynamic;
        }
    }
}
