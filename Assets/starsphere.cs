using UnityEngine;
using System.Collections;

public class starsphere : MonoBehaviour {

    public GameObject starPrefab;
    public float innerRadius;
    public float outerRadius;
    public int count;
    public Transform[] parentStars;

	void Start () {

        for (int i = 0; i < count; i++)
        {
            GameObject nw = (GameObject)Instantiate(starPrefab, transform.position + (Fakerand.UnitSphere(true) * Fakerand.Single(innerRadius, outerRadius)), Quaternion.identity);
            nw.transform.rotation = Quaternion.LookRotation((nw.transform.position - transform.position).normalized, Vector3.up);
            int r1 = Fakerand.Int(0, parentStars.Length);
            nw.transform.SetParent(parentStars[r1],true);
        }

	}

}
