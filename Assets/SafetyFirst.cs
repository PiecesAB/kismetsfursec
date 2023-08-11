using UnityEngine;
using System.Collections;

public class SafetyFirst : MonoBehaviour {

    [Range(0,31)]
    public byte ID;
    [Header("Check hub then it's a hub; or else it's a net")]
    public bool hub;

    void Start()
    {
        int n = ID + Utilities.colorCycle.Length;
        GetComponent<SpriteRenderer>().color = Utilities.colorCycle[n%Utilities.colorCycle.Length];
    }

    public void Animate()
    {

    }

	void OnTriggerEnter2D (Collider2D col) {
        {
            if (!hub)
            {
                if (col.gameObject.CompareTag("Player"))
                {
                    foreach (SafetyFirst s in FindObjectsOfType<SafetyFirst>())
                    {
                        if (s.hub && s.ID == ID && s.gameObject.activeInHierarchy)
                        {
                            col.transform.position = s.transform.position;
                        }
                    }
                }
            }
        }
	}
}
