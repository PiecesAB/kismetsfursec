using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InventoryInstructionsScroll : MonoBehaviour {

    public List<Transform> thingsToScroll;


    public float speed;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

        LoopA:
         foreach (Transform thingToScroll in thingsToScroll)
        {
            thingToScroll.localPosition = thingToScroll.transform.localPosition - new Vector3(speed, 0, 0);
            if (thingToScroll.localPosition.x < -420f)
            {
                thingsToScroll.Remove(thingToScroll);
                Destroy(thingToScroll.gameObject);
                goto LoopA;
            }
        }

        if (thingsToScroll[0].localPosition.x < -250f && thingsToScroll.Count == 1)
        {
            
            GameObject g = Instantiate(thingsToScroll[0].gameObject);
            g.transform.parent = thingsToScroll[0].parent;
            g.transform.localPosition = new Vector3(420f, 0f, 0f); //dankk
            g.transform.localScale = new Vector3(1, 1, 1);
            g.name = "Instructions";
            thingsToScroll.Add(g.transform);
            
        }
        

    }
}
