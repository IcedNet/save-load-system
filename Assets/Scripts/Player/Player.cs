using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody2D rb;
    private float moveSpeed,
        dirX,
        dirY;

    public bool ClimbingAllowed { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        moveSpeed = 5f;
    }

    // Update is called once per frame
    void Update()
    {
        dirX = Input.GetAxisRaw("Horizontal") * moveSpeed;

        if (ClimbingAllowed)
        {
            dirY = Input.GetAxisRaw("Vertical") * moveSpeed;
        }
    }

    private void FixedUpdate()
    {
        if (ClimbingAllowed)
        {
            rb.bodyType = RigidbodyType2D.Kinematic; // isKinematic = true;
            rb.linearVelocity = new Vector2(dirX, dirY);
        }
        else
        {
            rb.bodyType = RigidbodyType2D.Dynamic; // isKinematic = false;
            rb.linearVelocity = new Vector2(dirX, rb.linearVelocity.y);
        }
    }
}
