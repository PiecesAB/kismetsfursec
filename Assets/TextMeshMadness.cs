using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TextMeshMadness : MonoBehaviour {

    private List<GameObject> stuff = new List<GameObject>();
    public byte effect;
    public Color[] colors;
    private float[] noises;
    [SerializeField]
    private bool makeMore = false;
    private Vector3 mp;
    private Vector3 mp2;

	void Start () {
	if (effect == 1 && makeMore)
        {
            mp2 = transform.position;
            noises = new float[4];
            mp = (Vector3.forward * 8);
            for (int i = 0; i <= 3; i++)
            {
                float a = i * 1.5707963f;
                GameObject n = (GameObject)Instantiate(gameObject, transform.localPosition + mp + (new Vector3(Mathf.Cos(a),Mathf.Sin(a))*6), transform.rotation);
                n.GetComponent<TextMeshMadness>().makeMore = false;
                n.GetComponent<TextMesh>().color = colors[i];
                n.transform.SetParent(transform,true);
                stuff.Add(n);
            }
        }
	}
	
	void Update () {
        if (!makeMore)
        {
            foreach (Transform c in transform)
            {
                Destroy(c.gameObject);
            }
        }
        if (effect == 1 && makeMore)
        {
            for(int i = 0; i <= 3; i++)
            {
                noises[i] = Mathf.PerlinNoise(i*10000f, (float)DoubleTime.UnscaledTimeRunning * 1.375f);
            }

            double a = DoubleTime.UnscaledTimeRunning*1.375;
            int z = 0;
            foreach (var c in stuff)
            {
                c.transform.localPosition = mp + (new Vector3((float)System.Math.Cos(a), (float)System.Math.Sin(a)) * 8 * noises[z]);
                a += 1.5707963f;
                z++;
            }
            transform.localPosition = mp2 + (transform.up * (float)System.Math.Cos(a * 2f) * 8) - (transform.right * (float)System.Math.Cos(a * 1.7f)*2f);
        }
	}
}
