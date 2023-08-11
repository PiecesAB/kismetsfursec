using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIDamageNumber : MonoBehaviour {


    private float velocity;
	// Use this for initialization
	void Start () {
        velocity = 4.5f;
        Destroy(gameObject, 2.1f);
	}
	
	// Update is called once per frame
	void Update()
    {
        float v = 0;
        if (velocity > -3)
        {
            v = velocity;
        }
        GetComponent<RectTransform>().position = GetComponent<RectTransform>().position + (Vector3.up * v);
        GetComponent<Outline>().effectColor = Color.HSVToRGB(((float)DoubleTime.UnscaledTimeRunning*3)%1,1,0.55f);
        velocity -= 0.18f;
    }
}
