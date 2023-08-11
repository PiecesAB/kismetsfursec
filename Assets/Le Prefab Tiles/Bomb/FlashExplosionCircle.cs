using UnityEngine;
using System.Collections;

public class FlashExplosionCircle : MonoBehaviour {

    public float waitTime;
    public Color color1;
    public Color color2;
    private bool db = false;

    public GameObject obj;
    // Use this for initialization
    void Start () {
        GetComponent<SpriteRenderer>().color = color1;
        if (db == false)
        {
            db = true;
            StartCoroutine(Succ());
        }
        
	}
	
    IEnumerator Succ()
    {
        while (true)
        {
            GetComponent<SpriteRenderer>().color = color1;
            yield return new WaitForSeconds(waitTime);
            GetComponent<SpriteRenderer>().color = color2;
            yield return new WaitForSeconds(waitTime);
        }
    }
	// Update is called once per frame
	void Update () {
        //transform.position = obj.transform.position;
	}
}
