using UnityEngine;
using System.Collections;

public class DoubleGuillotineGenerator : MonoBehaviour {

    public float speed;
    public float delay;
    public float startDelay;
    public bool on;
    public GameObject guillotineObj;
    private float top;
    private float bottom;




	// Use this for initialization
	void Start () {
        StartCoroutine(Make(0));
	}
	
    public IEnumerator Make(int hi)
    {
        if (hi==0)
        {
            yield return new WaitForSeconds(startDelay);
        }
        if (on)
        {
            GameObject stuff = Instantiate(guillotineObj, transform.position, Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z)) as GameObject;
            stuff.transform.Translate((Vector3.up * ((16 * transform.localScale.y) + 20)));
            stuff.GetComponent<NormalGuillotine1>().speed = speed;
            stuff.GetComponent<NormalGuillotine1>().Slice(((32 * transform.localScale.y) + 40));
            stuff.GetComponent<SpriteRenderer>().sortingLayerName = "MG";

          
        }
        yield return new WaitForSeconds(delay);
        StartCoroutine(Make(1));
    }

	// Update is called once per frame
	void Update () {
	
	}
}
