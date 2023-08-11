using UnityEngine;
using System.Collections;

public class BossChp2ForceField : MonoBehaviour {

    public bool on;

	// Use this for initialization
	void Start () {
	
	}
	
    void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player") && on && other.gameObject.GetComponent<KHealth>())
        {
            other.gameObject.GetComponent<KHealth>().SetHealth(Mathf.Infinity,"");
        }
    }


	// Update is called once per frame
	void Update () {
	
	}
}
