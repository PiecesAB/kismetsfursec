using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class primFollowNearestPlayer : MonoBehaviour
{

    //modes soon
    public float speed;
    public Renderer myRenderer;

    private static List<primFollowNearestPlayer> allSkulls = new List<primFollowNearestPlayer>(); 
    private int oscTimer;

    private static Vector2[] allDirections = new Vector2[8] {
        Vector2.right,
        new Vector2(0.70710678f,0.70710678f),
        Vector2.up,
        new Vector2(-0.70710678f,0.70710678f),
        Vector2.left,
        new Vector2(-0.70710678f,-0.70710678f),
        Vector2.down,
        new Vector2(0.70710678f,-0.70710678f)
    };

    Transform getClosestPlayer(out Vector2 dist)
    {
        Transform o = null;
        float d = 1000000f;
        Vector2 t1 = Vector2.zero;
        foreach (GameObject t in LevelInfoContainer.allBoxPhysicsObjects) //outdated check lol
        {
            if (t && t.CompareTag("Player"))
            {
                t1 = (t.transform.position - transform.position);
                float ez = t1.sqrMagnitude;
                if (ez < d * d)
                {
                    o = t.transform;
                    d = Fastmath.FastSqrt(ez);
                }
            }
        }
        dist = t1;
        return o;
    }

    Vector2 ClosestVector(Vector2 t)
    {
        float max = Mathf.NegativeInfinity;
        Vector2 closest = Vector2.zero;
        for (int i = 0; i < allDirections.Length; i++)
        {
            float dot = Vector2.Dot(t, allDirections[i]);
            if (dot > max)
            {
                max = dot;
                closest = allDirections[i];
            }
        }
        return closest;
    }

    void Start()
    {
        oscTimer = 6;
        allSkulls.Add(this);
    }

    void OnDestroy()
    {
        allSkulls.Remove(this);
    }


    void Update()
    {
        if (Time.timeScale > 0 && myRenderer.isVisible && !Door1.levelComplete)
        {
            oscTimer--;
            if (oscTimer <= 0)
            {
                oscTimer = 6;
            }

            Vector2 closestDir;
            Transform closest = getClosestPlayer(out closestDir);
            Vector2 closestN = closestDir.normalized;

            Rigidbody2D rg2 = GetComponent<Rigidbody2D>();

            // extra check: don't smush the player. just trap them.
            Vector3 mv = ClosestVector(closestDir) * speed * Time.timeScale;
            RaycastHit2D[] rh = Physics2D.RaycastAll(transform.position, closestDir.normalized, 48f, 256 + 512);
            bool stopDueToSmush = false;
            foreach (RaycastHit2D r in rh)
            {
                if (r.collider.gameObject != gameObject) // player is not detected by layer mask
                {
                    stopDueToSmush = true;
                    break;
                }
            }

            if (oscTimer <= 2 && !stopDueToSmush) //move
            {
                if (rg2)
                {
                    rg2.velocity = mv/Time.deltaTime;
                }
                else
                {
                    transform.position += mv;
                }
            }
            else
            {
                if (rg2)
                {
                    rg2.velocity = Vector2.zero;
                }
            }
        }
    }
}
