using UnityEngine;
using System.Collections;

public class WireMove : MonoBehaviour {

    public Vector2 direction;
    public float speed;

    public GameObject[] all;


	// Use this for initialization
	void Start () {
        all = UnityEngine.Object.FindObjectsOfType<GameObject>();
        foreach (GameObject go in all)
            if (go.activeInHierarchy)
            {
                if (go.transform.position == transform.position && !go.CompareTag("Player") && !(go.CompareTag("Wire")))
                {
                    Move(go);
                }
            }

    }
	
    public void Move(GameObject obj)
    {
        StartCoroutine(xD(obj));
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("AttachableObject"))
        {
            GameObject obj = other.gameObject;
            obj.tag = "Untagged";
            obj.transform.position = transform.position;
            obj.GetComponent<Rigidbody2D>().isKinematic = true;
            StartCoroutine(xD(obj));
        }
    }

    IEnumerator xD(GameObject obj)
    {
        for (int a = 0; a < 8; a++)
        {
            obj.transform.position = obj.transform.position + new Vector3(2*direction.x, 2*direction.y);
            yield return new WaitForSeconds(0.06f/ (Time.timeScale*speed));
        }
        all = UnityEngine.Object.FindObjectsOfType<GameObject>();
        foreach (GameObject go in all)
        {
            if (go.transform.position == transform.position + new Vector3(direction.x * 16,direction.y*16) && go.CompareTag("Wire"))
            {
                go.GetComponent<WireMove>().Move(obj);
            }
        }
    }


	// Update is called once per frame
	void Update () {
	
	}
}
