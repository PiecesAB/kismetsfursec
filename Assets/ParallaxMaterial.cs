using UnityEngine;
using System.Collections;

public class ParallaxMaterial : MonoBehaviour {

    public Vector2 scrollStrength = new Vector2(1f, 1f);
    public Vector2 selfScrollStrength = Vector2.zero;
    private Vector3 cstart;
    private Vector3 sstart;
    public bool bgItemsTintOn;
    public Color bgItemsTint;
    [Header("leave false if object, else it scrolls the tiling of material")]
    public bool pictureScroll;
    public bool pictureVertLoop;
    public TestScriptRotateScene sceneRotator;
    public bool randSpeed = false;
    public bool randStartPos = false;
    public bool takeFracPartOfPos = false;

    // Use this for initialization
    void Start () {
        cstart = Camera.main.transform.position;
        sstart = transform.position;
        BGTint();
        if (randSpeed && GetComponent<Animator>())
        {
            GetComponent<Animator>().speed = Fakerand.Single(0.9f, 1.1f);
        }
        if (randStartPos)
        {
            GetComponent<Renderer>().material.mainTextureOffset += new Vector2(Fakerand.Single(), Fakerand.Single());
        }
    }

    public void BGTint()
    {
        if (bgItemsTintOn)
        {
            foreach (Transform i in transform)
            {
                if (i.gameObject.name == "fanbg")
                {
                    foreach (Transform j in i)
                    {
                        j.GetComponent<Renderer>().material.color = bgItemsTint;
                    }
                }
                else
                {
                    i.GetComponent<Renderer>().material.color = bgItemsTint;
                }
            }
        }
    }
	
	// Update is called once per frame
	void Update () {
        Transform targT = Camera.main.transform; //Camera.main.GetComponent<FollowThePlayer>().target;
        Vector3 targMart = targT.localPosition;
        //float angle = (sceneRotator != null)?-sceneRotator.transform.eulerAngles.z*Mathf.Deg2Rad:0; //in case the level is rotated
        float angle = 0f;
        Encontrolmentation e = LevelInfoContainer.GetActiveControl();
        if (e) { angle = -e.transform.eulerAngles.z * Mathf.Deg2Rad; }
        Vector3 dif = targMart - cstart;

        if (pictureScroll)
        {
            Vector2 scrol = new Vector2((dif.x * 0.000001f * scrollStrength.x) + (0.00022f*selfScrollStrength.x) , pictureVertLoop ? ((dif.y * 0.000001f * scrollStrength.y) + (0.00022f * selfScrollStrength.y)) : 0);
            float sa = Mathf.Sin(angle);
            float ca = Mathf.Cos(angle);
            scrol = new Vector2(scrol.x * ca - scrol.y* sa, scrol.x * sa + scrol.y * ca);
            GetComponent<Renderer>().material.mainTextureOffset += scrol;
            if (!pictureVertLoop)
            {
                transform.position -= (new Vector3(0f, dif.y * 0.0003f * scrollStrength.y));

            }
        }
        else
        {
            sstart += (new Vector3(dif.x * 0.0001f * scrollStrength.x, dif.y * 0.0001f * scrollStrength.y));
            transform.position = sstart;
        }

        if (takeFracPartOfPos)
        {
            Vector2 o = GetComponent<Renderer>().material.mainTextureOffset;
            GetComponent<Renderer>().material.mainTextureOffset = new Vector2(Mathf.Repeat(o.x, 1f), Mathf.Repeat(o.y, 1f));
        }

        cstart = targMart;


    }
}
