using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class LearningGunExperimental : MonoBehaviour {


    public struct Node1
    {
        public float angle { get; set; }
        public float distance { get; set; }

        public float myAngle { get; set; }

        public Node1(float _angle, float _distance)
        {
            angle = _angle;
            distance = _distance;
            myAngle = (_angle+Mathf.PI)%(Mathf.PI*2f);
        }
        public Node1(float _angle, float _distance, float _myAng)
        {
            angle = _angle;
            distance = _distance;
            myAngle = _myAng;
        }
    }

    public struct WatchObject
    {
        public GameObject observe { get; set; }
        public int NodeA { get; set; }
        public int NodeB { get; set; }
        public float lerp { get; set; }

        public WatchObject(GameObject _ob,int _nodeAIndex,int _nodeBIndex,float _lerp)
        {
            observe = _ob;
            NodeA = _nodeAIndex;
            NodeB = _nodeBIndex;
            lerp = _lerp;
        }
    }

    public int brainSize = 12;
    public List<Node1> brain;
    public float gunRange = 220;
    public float gunSpeed = 150;
    public float startDelay = 0f;
    public float gunFireRate = 1.5f;
    public GameObject bullet;
    public float bulletLifetime;
    public GameObject target;

    private float thyme;

    private static List<WatchObject> watchList = new List<WatchObject>();

	// Use this for initialization
	void Start () {
        brain = new List<Node1>(new Node1[brainSize*brainSize]);
        Wipe();
        thyme = startDelay;
	}

    public void Wipe()
    {
        for (int x = 0; x < brainSize*brainSize; x+=brainSize)
        {
            for (int y = 0; y < brainSize; y+=1)
            {
                brain[x+y] = new Node1(Mathf.Deg2Rad* y*(360f/brainSize),(((float)x)/(brainSize*brainSize))*gunRange);
            }
        }
    }

    void Learn(WatchObject w)
    {
        float ma = (w.lerp * brain[w.NodeA].myAngle + (1 - w.lerp) * brain[w.NodeB].myAngle) * Mathf.Rad2Deg;
        while (ma <= 0f)
        {
            ma += 360f;
        }
        float ta = Mathf.Atan2(transform.position.y- target.transform.position.y,  transform.position.x- target.transform.position.x) * Mathf.Rad2Deg;
        while (ta < ma)
        {
            ta += 360f;
        }
        float err = ((ta - ma)-180)*Mathf.Deg2Rad; //range is -pi to pi
        //Node1 comp = new Node1(brain[w.NodeA].angle + (1 - w.lerp) * brain[w.NodeB].angle, w.lerp * brain[w.NodeA].distance + (1 - w.lerp) * brain[w.NodeB].distance);
        for (int x = 0; x < brainSize*brainSize; x += brainSize)
        {
            for (int y = 0; y < brainSize; y += 1)
            {
                Node1 t = brain[x + y];
                float distA = (w.NodeA / brainSize) + w.lerp;
                float angA = (w.NodeA % brainSize) + w.lerp;
                float weigh = Mathf.Max(0f,((5f - 0.5f*System.Math.Abs(distA - (x/brainSize))) - System.Math.Abs(angA - y)) / 5f);
                brain[x + y] = new Node1(brain[x + y].angle, brain[x + y].distance, brain[x + y].myAngle+ err*weigh);
            }
        }
        
    }

    float Pos(float n, float i)
    {
        while (n < i)
        {
            n += i;
        }
        n %= i;
        return n;
    }

	// Update is called once per frame
	void Update () {
        foreach (var i in watchList)
        {
            float d = i.lerp*brain[i.NodeA].distance + (1-i.lerp)*brain[i.NodeB].distance;
            if (i.observe != null)
            {
                if (Fastmath.FastV2Dist(i.observe.transform.position, transform.position) >= d)
                {
                    Learn(i);
                    watchList.Remove(i);
                    break;
                }
            }

        }
        watchList.RemoveAll(item => item.observe == null);

        if (thyme < DoubleTime.ScaledTimeSinceLoad)
        {
                thyme += 1f / gunFireRate;
            if (Fastmath.FastV2Dist(transform.position, target.transform.position) <= gunRange)
            {
                float ang = Mathf.Atan2(target.transform.position.y - transform.position.y, target.transform.position.x - transform.position.x);
                while (ang < 0f)
                {
                    ang += Mathf.PI * 2f;
                }
                int angLOL = (int)Mathf.Floor((ang * Mathf.Rad2Deg) / (360f / brainSize));
                int distLOL = ((int)Mathf.Floor(Fastmath.FastV2Dist(target.transform.position, transform.position) / (gunRange / brainSize))) * brainSize;
                int angLOL2 = ((int)Mathf.Ceil((ang * Mathf.Rad2Deg) / (360f / brainSize))) % brainSize;
                int distLOL2 = (((int)Mathf.Ceil(Fastmath.FastV2Dist(target.transform.position, transform.position) / (gunRange / brainSize))) * brainSize) % (brainSize*brainSize);
                //print(angLOL);
                // print(distLOL);
                // print(angLOL2);
                //  print(distLOL2);
                GameObject x = (GameObject)Instantiate(bullet, transform.position, Quaternion.AngleAxis(ang, Vector3.forward));
                Destroy(x, bulletLifetime);

                WatchObject w = new WatchObject(
                    x,
                    angLOL + distLOL,
                    angLOL2 + distLOL2,
                    Mathf.InverseLerp(angLOL, angLOL2, ang)
                    );

                watchList.Add(w);
                float sa = w.lerp * brain[w.NodeA].myAngle + (1f - w.lerp) * brain[w.NodeB].myAngle;
                x.GetComponent<PrimMovingPlatform>().velocity = new Vector2(gunSpeed * Mathf.Cos(sa), gunSpeed * Mathf.Sin(sa));
                //   print(sa);
            }
        }


	}
}
