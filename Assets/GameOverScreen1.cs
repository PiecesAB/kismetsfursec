using UnityEngine;
using System.Collections;

public class GameOverScreen1 : MonoBehaviour {

    public Camera cam;

	// Use this for initialization
	void Start () {
        Time.timeScale = 1f;
	}
	
    IEnumerator Hi()
    {
       // cam = FindObjectsOfType<Camera>()[0];
            yield return new WaitForSeconds(1f);
        for (int i = 0; i < 25; i++)
        {
            GetComponent<RectTransform>().localScale = new Vector3(1, Mathf.Lerp(GetComponent<RectTransform>().localScale.y, 1f, 0.1f), 1);
            yield return new WaitForSeconds(0.01666666f);
        }

        GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        yield return new WaitForSeconds(4f);

    }

	// Update is called once per frame
	void FixedUpdate () {
        StartCoroutine(Hi());
        
	}
}
