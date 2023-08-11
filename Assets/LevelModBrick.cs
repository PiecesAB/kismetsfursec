using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelModBrick : MonoBehaviour {

    public enum Action
    {
        Rotate,FlipV,FlipH,Grow,Shrink
    }

    public Action action;
    [Header("Rotate: How many degrees to rotate")]
    [Header("FlipV/H: Set to 1 for orig. direction only, 2 for opp. only, 3 for both ways")]
    [Header("Grow/Shrink: The factor of scaling")]
    public float X;

    private bool rotating;
    public List<GameObject> mans = new List<GameObject>();
    public Texture2D breakPic;
    public Vector2 breakPicUV;
    public Transform player;

    private bool counteris;
    private float needed;
    private float ts;
    private float clockwise;
    private float counterclockwise;

    // Use this for initialization
    void Start () {
        counteris = (X >= 0) ? false : true;
        clockwise = (X >= 0) ? X : 0;
        counterclockwise = (X >= 0) ? 0 : -X;
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        if (coll.contacts[0].normal == new Vector2(0, 1) && !rotating)
        {
           for (int i = (int)breakPicUV.x; i < 16; i += 4)
            {
                for (int j =(int)breakPicUV.y; j < 16; j += 4)
                {
                    GameObject newPiece = new GameObject();
                    SpriteRenderer newSpr = newPiece.AddComponent<SpriteRenderer>();
                    newSpr.sprite = Sprite.Create(breakPic, new Rect(i, j, 4, 4), new Vector2(0.5f, 0.5f), 1f, 0, SpriteMeshType.FullRect);
                    newSpr.sortingLayerName = "UI";
                    Rigidbody2D newRig = newPiece.AddComponent<Rigidbody2D>();
                    newRig.gravityScale = 50;
                    newRig.velocity = 300 * Fakerand.UnitCircle();
                    newPiece.transform.position = transform.position + new Vector3(i - 8, j - 8, 0);
                }

            }
           

            rotating = true;
            
            needed = 0f;
            Utilities.canPauseGame = false;
            Utilities.canUseInventory = false;
            ts = Time.timeScale;
            Time.timeScale = 0f;
            foreach (Collider2D col in FindObjectsOfType<Collider2D>())
            {
                if (Fastmath.FastV2Dist(col.transform.position, player.position) > 60)
                    col.enabled = false;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

        /*if (objects.Count > 0)
        {
            foreach (var z in objects)
            {
                if (z.eventAName == "+" + Mathf.Floor(counterclockwise) + " degrees" && (z.flags & 32UL) == 32UL && !rotating)
                {
                    rotating = true;
                    counteris = true;
                    player = z.transform;
                    needed = 0;
                    Utilities.canPauseGame = false;
                    Utilities.canUseInventory = false;
                    ts = Time.timeScale;
                    Time.timeScale = 0;
                    foreach (Collider2D col in FindObjectsOfType<Collider2D>())
                    {
                        col.enabled = false;
                    }

                }
                if (z.eventBName == "-" + Mathf.Floor(clockwise) + " degrees" && (z.flags & 64UL) == 64UL && !rotating)
                {
                    rotating = true;
                    counteris = false;
                    player = z.transform;
                    needed = 0;
                    Utilities.canPauseGame = false;
                    Utilities.canUseInventory = false;
                    ts = Time.timeScale;
                    Time.timeScale = 0f;
                    foreach (Collider2D col in FindObjectsOfType<Collider2D>())
                    {
                        if (Fastmath.FastV2Dist(col.transform.position, player.position) > 60)
                            col.enabled = false;
                    }
                }
            }
        }*/

        if (rotating && player != null)
        {
            TestScriptRotateScene tr = FindObjectOfType<TestScriptRotateScene>();
            GameObject[] hello = GameObject.FindGameObjectsWithTag("Player");
            foreach (var man in hello)
            {
                if (man != player.gameObject && man.transform.parent != tr.transform)
                {
                    man.transform.SetParent(tr.transform);
                }
            }

            if ((counteris && counterclockwise - needed >= 2f) || (!counteris && clockwise - needed >= 2f))
            {
                needed += 2f;
                tr.transform.RotateAround(player.position, Vector3.forward, (counteris) ? 2f : -2f);
            }
            else
            {
                float deutsch = (counteris) ? (counterclockwise - needed) : (needed - clockwise);
                tr.transform.RotateAround(player.position, Vector3.forward, (counteris) ? deutsch : deutsch);
                Destroy(GetComponent<Collider2D>());
                Destroy(GetComponent<SpriteRenderer>());
                rotating = false;
                Utilities.canPauseGame = true;
                Utilities.canUseInventory = true;
                Time.timeScale = ts;
                FindObjectOfType<FollowThePlayer>().followCameraBounds = false;
                foreach (Collider2D col in FindObjectsOfType<Collider2D>())
                {
                    col.enabled = true;
                }
                foreach (var man in hello)
                {
                    if (man != player.gameObject)
                    {
                        man.transform.parent = tr.transform.parent;
                    }
                }
            }
        }
    }
}
