using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingWallCollider : MonoBehaviour {

    public RotatingWall mom;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        mom.Collided(collision);
    }

}
