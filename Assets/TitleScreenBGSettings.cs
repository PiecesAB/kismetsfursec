using UnityEngine;
using System.Collections;

public class TitleScreenBGSettings : MonoBehaviour {

    public enum BGs
    {
        TwistTunnel,
    }

    public BGs setting;
    public Gradient colorSamp1;
    public Gradient colorSamp2;
    public bool active;

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (setting == BGs.TwistTunnel)
        {
            float smollest = 1000f;
            foreach (Transform i in transform)
            {
                i.localScale = i.localScale * 1.018f;
                i.GetComponent<SpriteRenderer>().material.SetColor("_IC", colorSamp1.Evaluate(Mathf.PerlinNoise((float)DoubleTime.ScaledTimeSinceLoad*0.2f + Mathf.Log(i.localScale.x, 4), 0)));
                i.GetComponent<SpriteRenderer>().material.SetColor("_OC", colorSamp2.Evaluate(Mathf.PerlinNoise(0, (float)DoubleTime.ScaledTimeSinceLoad*0.2f+Mathf.Log(i.localScale.x,4))));
                if (i.localScale.x < smollest)
                {
                    smollest = i.localScale.x;
                }
                if (i.localScale.x > 96f)
                {
                    Destroy(i.gameObject);
                }
            }

            if (smollest > 6f && active)
            {
                foreach (Transform i in transform)
                {
                    i.GetComponent<SpriteRenderer>().sortingOrder++;
                }
                GameObject n = Instantiate(transform.GetChild(0).gameObject, Vector3.zero, transform.GetChild(0).rotation) as GameObject;
                n.name = "BGObject";
                n.GetComponent<SpriteRenderer>().sortingOrder = 0;
                n.transform.localScale = new Vector3(3, 3, 3);
                if (n.GetComponent<Prim3DRotate>())
                {
                    n.GetComponent<Prim3DRotate>().speed = ((Fakerand.Int(0, 2) * 2) - 1)*0.1f;
                }
                n.transform.SetParent(transform);
            }
        }
	}
}
