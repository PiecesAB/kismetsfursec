using UnityEngine;
using System.Collections;

public class matchdoor : MonoBehaviour {

    public GameObject doorEnd;
    public GameObject doorBody;
    public float pixelsPerSecond;

    public bool disabled;
    private Vector2 dir;

	// Use this for initialization
	void Start () {
        doorBody.GetComponent<LineRenderer>().sortingLayerName = "Objects";
        float z = Mathf.Deg2Rad*transform.eulerAngles.z + Mathf.PI/2;
        dir = new Vector2(Mathf.Cos(z), Mathf.Sin(z));
    }
	
	// Update is called once per frame
	void Update () {
        if (!disabled && !Physics2D.Raycast(doorEnd.transform.position+((Vector3)(dir*4)),dir, pixelsPerSecond * Time.deltaTime, ~(1<<8+1<<9+1<<11),transform.position.z-3, transform.position.z + 3))
        {
            BoxCollider2D b = doorBody.GetComponent<BoxCollider2D>();
            b.offset += new Vector2(0, pixelsPerSecond * Time.deltaTime / 2);
            b.size += new Vector2(0, pixelsPerSecond * Time.deltaTime);
            LineRenderer l = doorBody.GetComponent<LineRenderer>();
            l.SetPosition(1, new Vector3(0,b.size.y,0));
            l.material.mainTextureScale = new Vector2(b.size.y / 16,1);
            doorEnd.transform.localPosition = new Vector3(0, b.size.y, 0) + Vector3.up * 4;
        }
	}
}
