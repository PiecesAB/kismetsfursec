using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopUpEnemyCollider : MonoBehaviour {

    public PopUpEnemy parent;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        parent.Collided(collision);
    }
}
