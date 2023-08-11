using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class selfReplicatingBlock : MonoBehaviour
{

    //doesn't yet work when rotated!

    public float progress;
    public GameObject selfPrefab;
    public BoxCollider2D myCollider;
    public SpriteRenderer mySprite;
    public GameObject explosionPrefab;
    public AudioSource myBubblingSound;
    public ParticleSystem bubbles;
    public float wait;
    //public static List<Vector2> positions = new List<Vector2>();

    // we have to assume these blocks never move!
    public static PositionHashtable<selfReplicatingBlock> allBlocks = new PositionHashtable<selfReplicatingBlock>(1024, (e) => { return e.transform.position; });

    private const float waitTime = 1.5f;
    private Vector2[] directions = new Vector2[4]{Vector2.right, Vector2.up, Vector2.left, Vector2.down};
    private const float size = 16f;

    void Start()
    {
        wait = 0f;
        mySprite.color = new Color(1f, 1f, 1f, progress);
        if (allBlocks.Fetch(transform.position, 1000000f) == null)
        {
            allBlocks.Add(this);
        }
        else //abort
        {
            Destroy(gameObject);
        }

        if (progress <= 0f)
        {
            myCollider.enabled = false;
        }
    }

    void OnDestroy()
    {
        allBlocks.Remove(this);
    }

    void Update()
    {
        if (Time.timeScale > 0 && enabled)
        {
            bool burning = (!GetComponent<primExtraTags>() || !GetComponent<primExtraTags>().tags.Contains("flammable"));
            var emisTemp = bubbles.emission;
            myCollider.enabled = (progress > 0.5f);
            mySprite.color = new Color(1f, 1f, 1f, progress);

            if (burning) //don't replicate when flaming
            {
                enabled = false;
            }

            if (progress >= 1f) // ready to reproduce
            {
                progress = 1f;
                if (wait <= 0f)
                {
                    foreach (Vector2 dir in directions)
                    {
                        Vector2 fetchCheck = (Vector2)transform.position + size * dir;
                        if (allBlocks.Fetch(fetchCheck, 1000000f) == null && AmorphousGroundTileNormal.allBlocks.Fetch(fetchCheck, 1000000f) == null)
                        {
                            GameObject newCup = Instantiate(selfPrefab, transform.position + (Vector3)(size * dir), Quaternion.identity);
                            if (transform.parent)
                            {
                                newCup.transform.SetParent(transform.parent);
                            }
                            newCup.GetComponent<selfReplicatingBlock>().progress = 0f;
                        }
                    }
                    wait = 1.5f;
                }
                else
                {
                    wait -= Time.deltaTime;
                }
                emisTemp.enabled = false;
            }
            else
            {
                progress += 0.0083333333f * Time.timeScale;
                emisTemp.enabled = true;
            }

            if (burning)
            {
                emisTemp.enabled = false;
            }
        }
    }
}
