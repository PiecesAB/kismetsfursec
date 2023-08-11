using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TangramLaser : MonoBehaviour
{
    public struct LaserPair
    {
        public TangramLaser L1;
        public TangramLaser L2;

        public LaserPair(TangramLaser _L1, TangramLaser _L2)
        {
            if (_L1.myID > _L2.myID) //lower first
            {
                TangramLaser LT = _L1;
                _L1 = _L2;
                _L2 = LT;
            }
            L1 = _L1; //references??
            L2 = _L2;
        }

        public bool Contains(TangramLaser L)
        {
            int id = L.myID;
            return (L1.myID == id) || (L2.myID == id);
        }
    }

    public GameObject fullObject;

    public static int currID = -1;
    public static List<TangramLaser> allLasers = new List<TangramLaser>();
    public static List<TangramLaser> visibleNodes = new List<TangramLaser>();

    public static List<LaserPair> pairs = new List<LaserPair>();
    public static List<int> pairFramesToActive = new List<int>();
    public static List<TangramLaserCollider> pairLCs = new List<TangramLaserCollider>();
    private static bool allZeroCheck; //will save cpu

    public GameObject laserObject;

    public int myID = -1;

    private bool excess = false;
    private Renderer visTestRend;
    private bool lastTestRend = false;

    public const int framesToActive = 50;
    public const float plrDmgMultiplier = 1f;

    public Sprite brokenSprite;

    //LevelInfoContainer clears all lists when the level changes

    public void RemoveMyLasers()
    {
        for (int i = 0; i < pairs.Count; i++)
        {
            if (pairs[i].Contains(this))
            {
                pairs.RemoveAt(i);
                pairFramesToActive.RemoveAt(i);
                Destroy(pairLCs[i].gameObject);
                pairLCs.RemoveAt(i);
                i--;
            }
        }
        visibleNodes.Remove(this);
    }

    public void CreateMyLasers()
    {
        for (int i = 0; i < visibleNodes.Count; i++)
        {
            LaserPair n = new LaserPair(visibleNodes[i], this);
            if (!pairs.Contains(n))
            {
                pairs.Add(n);
                TangramLaserCollider tgc = Instantiate(laserObject, allLasers[0].transform).GetComponent<TangramLaserCollider>();
                pairLCs.Add(tgc);
                tgc.myIndex = pairLCs.Count-1;
            }
            pairFramesToActive.Add(framesToActive);
        }
        visibleNodes.Add(this);
        allZeroCheck = false;
    }

    void Awake()
    {
        excess = false;
        if (currID == int.MaxValue)
        {
            excess = true;
            Destroy(fullObject);
        }
        else
        {
            currID++;
            allLasers.Add(this);
            myID = currID;
        }

        visTestRend = GetComponent<Renderer>();
        lastTestRend = false;
        allZeroCheck = false;
    }

    void OnDestroy()
    {
        if (!excess)
        {
            currID--;
            //allLasers.Remove(this);
            allLasers.RemoveAt(myID);
            for (int i = 0; i < allLasers.Count; i++)
            {
                if (allLasers[i].myID > myID)
                {
                    allLasers[i].myID--;
                }
            }
            RemoveMyLasers();
        }
    }

    void OnCollisionEnter2D(Collision2D c)
    {
        if (c.collider.gameObject.layer == 19 || c.gameObject.CompareTag("SuperRay")) //punch or shot or more stuff soon
        {
            GetComponent<SpriteRenderer>().sprite = brokenSprite;
            Destroy(this);
        }
    }

    void Visible()
    {
        CreateMyLasers();
    }

    void Invisible()
    {
        RemoveMyLasers();
    }

    void Update()
    {
        bool v = visTestRend.isVisible;
        if (v != lastTestRend)
        {
            if (v)
            {
                Visible();
            }
            else
            {
                Invisible();
            }
            lastTestRend = v;
        }

        if (myID == 0)
        {
            for (int i = 0; i < pairs.Count; i++)
            {
                LineRenderer LR = pairLCs[i].GetComponent<LineRenderer>();
                if (LR)
                {
                    LR.SetPosition(0, pairs[i].L1.transform.position);
                    LR.SetPosition(1, pairs[i].L2.transform.position);
                    pairLCs[i].myFramesToActive = pairFramesToActive[i];
                }
                //print("(" + (pairs[i].L1.myID) + ", " + (pairs[i].L2.myID) + ")");
            }
            //print("------------------------------");

            if (!allZeroCheck)
            {
                allZeroCheck = true;
                for (int i = 0; i < pairFramesToActive.Count; i++)
                {
                    if (pairFramesToActive[i] > 0)
                    {
                        allZeroCheck = false;
                        pairFramesToActive[i]--;
                    }
                }
            }
        }
    }
}
