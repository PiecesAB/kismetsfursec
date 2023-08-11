using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using UnityEngine.SceneManagement;

[ExecuteInEditMode]
public class AmorphousGroundTileNormal : MonoBehaviour {

    public AmorphousTileDatabase databaseObject;
    public bool isHidingSomething;
    public List<AmorphousGroundTileNormal> neighbors;
    [Header("If hidesfx is null, then the block will just be walkthrough, not disappearing.")]
    public AudioSource hidesfx;
    private Vector3 oldPos;
    private AmorphousTileDatabase oldTiles;
    public bool deleteColliderOnceMade;
    public bool updatesAtAll = false;
    public bool solidifiesWhenVisible = false;
    private byte store = 0;

    public bool destroyScriptAndColliderOnUpdate = false;

    /*    _______________________
     *   |       |       |       |
     *   |   1   |   2   |   4   |
     *    _______________________
     *   |       |       |       |
     *   |   8   |   X   |  16   |
     *    _______________________
     *   |       |       |       |
     *   |  32   |  64   |  128  |
     *    _______________________
     */

    public static Vector2 dir1 = new Vector2(-16f, 16f);
    public static Vector2 dir2 = new Vector2(0f, 16f);
    public static Vector2 dir4 = new Vector2(16f, 16f);
    public static Vector2 dir8 = new Vector2(-16f, 0f);
    public static Vector2 dir16 = new Vector2(16f, 0f);
    public static Vector2 dir32 = new Vector2(-16f, -16f);
    public static Vector2 dir64 = new Vector2(0f, -16f);
    public static Vector2 dir128 = new Vector2(16f, -16f);

    public static PositionHashtable<AmorphousGroundTileNormal> allBlocks = new PositionHashtable<AmorphousGroundTileNormal>(8192, (a) => {
        if (a == null) { return Vector3.negativeInfinity; }
        return a.transform.position;
    });

    private bool fakePlace;

    private bool scheduleChangeShape;

    public static AmorphousGroundTileNormal meldMaker = null;

    private void OnDestroy()
    {
        allBlocks.Remove(this);

        // removing a static block will cause this
        if (Application.isPlaying && !fakePlace && !updatesAtAll && !solidifiesWhenVisible && AmorphousColliderMeld.blocksToMeld.Count > 0)
        {
            AmorphousColliderMeld.blocksToMeld.Clear();
        }
    }

    private void Awake()
    {
        if (Application.isPlaying && !fakePlace && !updatesAtAll && !solidifiesWhenVisible) //&& !GetComponent<Rigidbody2D>())
        {
            AmorphousColliderMeld.blocksToMeld.Add(this);
        }
    }

    private void Start() {
        scheduleChangeShape = false;
        allBlocks.Add(this);
        if (Application.isEditor && !Application.isPlaying)
        {
            scheduleChangeShape = true;
            oldPos = transform.position;
            oldTiles = databaseObject;
        }

        if (meldMaker == null) { meldMaker = this; }

        fakePlace = (Application.isEditor && !Application.isPlaying);
        if (!fakePlace && !updatesAtAll && !solidifiesWhenVisible)
        {
            if (meldMaker != this) { enabled = false; }
            //Destroy(this);
        }

        if (solidifiesWhenVisible)
        {
            GetComponent<Collider2D>().enabled = false;
        }

        GetComponent<Collider2D>().isTrigger = isHidingSomething && Application.isPlaying;
        GetComponent<BoxCollider2D>().size = isHidingSomething ? new Vector2(15.5f, 15.5f) : new Vector2(16f, 16f);
    }

    void Update()
    {
        if (Application.isPlaying && meldMaker == this)
        {
            AmorphousColliderMeld.CreateMeld(); meldMaker = null;
            if (!fakePlace && !updatesAtAll && !solidifiesWhenVisible)
            {
                enabled = false;
                return;
            }
        }

        //GetComponent<Rigidbody2D>().useFullKinematicContacts = true;
        if (destroyScriptAndColliderOnUpdate)
            {
                Destroy(GetComponent<Rigidbody2D>());
                Destroy(GetComponent<Collider2D>());
                Destroy(this);
            }

            if (solidifiesWhenVisible && GetComponent<Renderer>().isVisible)
            {
                GetComponent<Collider2D>().enabled = true;
            }

        bool del = false;
            transform.hasChanged = false;
            /*if (neighbors.Contains(null))
            {
                del = true;
            }*/

            if (scheduleChangeShape)
            {
                scheduleChangeShape = false;
                ChangeShape();
            }

            if (fakePlace && (oldPos != transform.position || oldTiles != databaseObject || del))
            {
                allBlocks.Reposition(this);
                ChangeShape();
                oldPos = transform.position;
                oldTiles = databaseObject;

                foreach (var b in neighbors)
                {
                    b.GetComponent<AmorphousGroundTileNormal>().ChangeShape();
                }
            }
    }
    
    bool TopLeft()
    {
        return (store & 1) == 1;
    }
    bool Top()
    {
        return (store & 2) == 2;
    }
    bool TopRight()
    {
        return (store & 4) == 4;
    }
    bool Left()
    {
        return (store & 8) == 8;
    }
    bool Right()
    {
        return (store & 16) == 16;
    }
    bool BottomLeft()
    {
        return (store & 32) == 32;
    }
    bool Bottom()
    {
        return (store & 64) == 64;
    }
    bool BottomRight()
    {
        return (store & 128) == 128;
    } //looks a bit better

    void Set(Sprite[] s)
    {
        if (s != null && s.Length != 0)
        {
            int r = Fakerand.Int(0, s.Length);
            GetComponent<SpriteRenderer>().sprite = s[r];
        }
    }

    public void OnTriggerEnter2D(Collider2D col)
    {
        if (col && col.GetComponent<Rigidbody2D>() && !col.GetComponent<Rigidbody2D>().isKinematic)
        {
            if (isHidingSomething)
            {
                StartCoroutine(RevealIfSecret(false));
            }
        }
    }

    public IEnumerator RevealIfSecret(bool me)
    {
        allBlocks.Remove(this);
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        bool spread = false;
        Destroy(GetComponent<Collider2D>());
        if (hidesfx == null)
        {
            yield break;
        }
        if (!me)
        {
            //hidesfx.Stop();
            if (!hidesfx.isPlaying)
            {
                hidesfx.Play();
            }
        }
        while (sr.color.a > 0)
        {
            sr.color = new Color(Fakerand.Single(), Fakerand.Single(), Fakerand.Single(), sr.color.a - 0.1f);
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            if (!spread)
            {
                spread = true;
                foreach (AmorphousGroundTileNormal col in GetNeighbors())
                {
                    if (col != null)
                    {
                        if (col.isHidingSomething)
                        {
                            col.gameObject.GetComponent<MonoBehaviour>().StartCoroutine(col.RevealIfSecret(true));
                        }
                    }
                }
            }
        }
        foreach (AmorphousGroundTileNormal col in neighbors)
        {
            if (col != null)
            {
                if (!col.isHidingSomething)
                {
                    col.ChangeShape();
                }
            }
        }
        Destroy(gameObject);
        yield return new WaitForEndOfFrame();
    }

    private List<AmorphousGroundTileNormal> GetNeighbors()
    {
        if (Application.isPlaying)
        {
            Vector2[] checks = { dir1, dir2, dir4, dir8, dir16, dir32, dir64, dir128 };
            List<AmorphousGroundTileNormal> result = new List<AmorphousGroundTileNormal>(8);
            for (int i = 0; i < checks.Length; ++i)
            {
                AmorphousGroundTileNormal fetched = allBlocks.Fetch(transform.position + (Vector3)checks[i]);
                if (fetched) { result.Add(fetched); }
            }
            return result;
        }

        // because there's no hashing in the editor.

        List<Collider2D> ret = new List<Collider2D>(Physics2D.OverlapCircleAll(transform.position, 19f, 1 << 8, transform.position.z - 8, transform.position.z + 8)); //lag, fix later

        for (int i = 0; i < ret.Count; ++i)
        {
            if (ret[i].GetComponent<AmorphousGroundTileNormal>() == null)
            {
                ret.RemoveAt(i); --i;
            }
        }

        List<AmorphousGroundTileNormal> ret2 = new List<AmorphousGroundTileNormal>();
        foreach (Collider2D c in ret)
        {
            ret2.Add(c.GetComponent<AmorphousGroundTileNormal>());
        }

        return ret2;
    }

    public void ChangeShape()
    {
        store = 0;
        neighbors = GetNeighbors();

        //bool done1 = false;

        /*while (!done1)
        {
            foreach (var b in neighbors)
            {
                if (neighbors.IndexOf(b) == neighbors.Count - 1)
                {
                    done1 = true;
                }
                if (b.GetComponent<AmorphousGroundTileNormal>() == null)
                {
                    neighbors.Remove(b);
                    break; //do it again later
                }
            }
        }*/

        foreach (var b in neighbors)
        {
            if (b.gameObject != gameObject)
            {
            Vector2 rpos = transform.InverseTransformPoint(b.transform.position);
                if (Fastmath.FastV2Dist(rpos, dir1) <= 1f) { store += 1; }
                if (Fastmath.FastV2Dist(rpos, dir2) <= 1f) { store += 2; }
                if (Fastmath.FastV2Dist(rpos, dir4) <= 1f) { store += 4; }
                if (Fastmath.FastV2Dist(rpos, dir8) <= 1f) { store += 8; }
                if (Fastmath.FastV2Dist(rpos, dir16) <= 1f) { store += 16; }
                if (Fastmath.FastV2Dist(rpos, dir32) <= 1f) { store += 32; }
                if (Fastmath.FastV2Dist(rpos, dir64) <= 1f) { store += 64; }
                if (Fastmath.FastV2Dist(rpos, dir128) <= 1f) { store += 128; }
            }
        }

        AmorphousTileDatabase a = databaseObject;

        bool TheFollowingCodeReallySucks = true;
        #region suck
        if (Top())
        {
            if (Left())
            {
                if (Right())
                {
                    if (Bottom())
                    {
                        if (TopLeft())
                        {
                            if (TopRight())
                            {
                                if (BottomRight())
                                {
                                    if (BottomLeft())
                                    {
                                        Set(a.center);
                                    }
                                    else
                                    {
                                        Set(a.cross_UL_UR_DR);
                                    }
                                }
                                else if (BottomLeft())
                                {
                                    Set(a.cross_UL_UR_DL);
                                }
                                else
                                {
                                    Set(a.cross_UL_UR);
                                }
                            }
                            else if (BottomRight())
                            {
                                if (BottomLeft())
                                {
                                    Set(a.cross_UL_DL_DR);
                                }
                                else
                                {
                                    Set(a.cross_UL_DR);
                                }
                            }
                            else if (BottomLeft())
                            {
                                Set(a.cross_UL_DL);
                            }
                            else
                            {
                                Set(a.cross_UL);
                            }
                        }
                        else if (TopRight())
                        {
                            if (BottomRight())
                            {
                                if (BottomLeft())
                                {
                                    Set(a.cross_UR_DL_DR);
                                }
                                else
                                {
                                    Set(a.cross_UR_DR);
                                }
                            }
                            else if (BottomLeft())
                            {
                                Set(a.cross_UR_DL);
                            }
                            else
                            {
                                Set(a.cross_UR);
                            }
                        }
                        else if (BottomRight())
                        {
                            if (BottomLeft())
                            {
                                Set(a.cross_DL_DR);
                            }
                            else
                            {
                                Set(a.cross_DR);
                            }
                        }
                        else if (BottomLeft())
                        {
                            Set(a.cross_DL);
                        }
                        else
                        {
                            Set(a.cross);
                        }
                    }
                    else
                    {
                        if (TopLeft())
                        {
                            if (TopRight())
                            {
                                Set(a.ceiling);
                            }
                            else
                            {
                                Set(a.TUp_LeftCorner);
                            }
                        }
                        else if (TopRight())
                        {
                            Set(a.TUp_RightCorner);
                        }
                        else
                        {
                            Set(a.TUp);
                        }
                    }
                }
                else if (Bottom())
                {
                    if (TopLeft())
                    {
                        if (BottomLeft())
                        {
                            Set(a.leftWall);
                        }
                        else
                        {
                            Set(a.TLeft_UpCorner);
                        }
                    }
                    else if (BottomLeft())
                    {
                        Set(a.TLeft_DownCorner);
                    }
                    else
                    {
                        Set(a.TLeft);
                    }
                }
                else
                {
                    if (TopLeft())
                    {
                        Set(a.topLeftCorner);
                    }
                    else
                    {
                        Set(a.topLeftElbow);
                    }
                }
            }
            else if (Right())
            {
                if (Bottom())
                {
                    if (TopRight())
                    {
                        if (BottomRight())
                        {
                            Set(a.rightWall);
                        }
                        else
                        {
                            Set(a.TRight_UpCorner);
                        }
                    }
                    else if (BottomRight())
                    {
                        Set(a.TRight_DownCorner);
                    }
                    else
                    {
                        Set(a.TRight);
                    }
                }
                else
                {
                    if (TopRight())
                    {
                        Set(a.topRightCorner);
                    }
                    else
                    {
                        Set(a.topRightElbow);
                    }
                }
            }
            else if (Bottom())
            {
                Set(a.vertical);
            }
            else
            {
                Set(a.stalactite);
            }
        }
        else if(Left())
        {
            if (Right())
            {
                if (Bottom())
                {
                    if (BottomLeft())
                    {
                        if (BottomRight())
                        {
                            Set(a.floor);
                        }
                        else
                        {
                            Set(a.TDown_LeftCorner);
                        }
                    }
                    else if (BottomRight())
                    {
                        Set(a.TDown_RightCorner);
                    }
                    else
                    {
                        Set(a.TDown);
                    }
                }
                else
                {
                    Set(a.horizontal);
                }
            }
            else if (Bottom())
            {
                if (BottomLeft())
                {
                    Set(a.bottomLeftCorner);
                }
                else
                {
                    Set(a.bottomLeftElbow);
                }
            }
            else
            {
                Set(a.leftProtrusion);
            }

        }
        else if (Right())
        {
            if (Bottom())
            {
                if (BottomRight())
                {
                    Set(a.bottomRightCorner);
                }
                else
                {
                    Set(a.bottomRightElbow);
                }
            }
            else
            {
                Set(a.rightProtrusion);
            }
        }
        else if (Bottom())
        {
            Set(a.stalagmite);
        }
        else
        {
            Set(a.alone);
        }
        #endregion

        if (deleteColliderOnceMade && Application.isPlaying)
        {
            Destroy(GetComponent<Collider2D>(),0.1f);
            Destroy(this,0.1f);
        }


    }
	
}
